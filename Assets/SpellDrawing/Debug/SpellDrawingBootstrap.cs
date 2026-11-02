// 테스트 화면의 컴포지션 루트: 설정을 복제하고 세션·판정·기록·뷰를 만들어 연결한다. 매 프레임 Tick과 Space/Esc를 처리한다.
// 객체 생성과 연결은 이곳에서만 한다 (싱글턴·Find 없음).
using System;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace VoxelMagic.SpellDrawing.DebugUI
{
    public sealed class SpellDrawingBootstrap : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private SpellTemplateLibrary templateLibrary;
        [SerializeField] private DrawingSessionSettings sessionSettings;
        [SerializeField] private JudgeSettings judgeSettings;
        [SerializeField] private RecordSettings recordSettings;

        [Header("입력 (IStrokeInput 구현 컴포넌트)")]
        [SerializeField] private MonoBehaviour strokeInputSource;

        [Header("화면")]
        [SerializeField] private DrawingAreaView drawingArea;
        [SerializeField] private TimerView timerView;
        [SerializeField] private ResultPanelView resultPanel;
        [SerializeField] private HistoryListView historyList;
        [SerializeField] private KoreanFontApplier fontApplier;

        [Header("상단 바")]
        [SerializeField] private Text templateNameText;
        [SerializeField] private Text noticeText;
        [SerializeField] private Button startButton;
        [SerializeField] private Button saveTemplateButton;

        private DrawingSessionSettings _sessionSettings;
        private JudgeSettings _judgeSettings;
        private DrawingSession _session;
        private SessionStats _stats;
        private ThumbnailCache _thumbnailCache;
        private AttemptFileLogger _fileLogger;

        private void Awake()
        {
            var template = templateLibrary != null ? templateLibrary.Default : null;
            var input = strokeInputSource as IStrokeInput;
            if (template == null || input == null || sessionSettings == null || judgeSettings == null || recordSettings == null)
            {
                UnityEngine.Debug.LogError("[SpellDrawing] 부트스트랩 설정이 비어 있습니다. Tools/SpellDrawing/Build Test Scene으로 씬을 다시 만드세요.");
                enabled = false;
                return;
            }

            // 슬라이더가 원본 에셋을 덮어쓰지 않도록 복제본을 쓴다.
            _sessionSettings = Instantiate(sessionSettings);
            _judgeSettings = Instantiate(judgeSettings);

            var judge = new SpellJudge(new ChamferSimilarityMetric(_judgeSettings.DistanceScoring));
            _session = new DrawingSession(_sessionSettings, _judgeSettings, judge, template);
            _stats = new SessionStats();
            _session.AttemptCompleted += _stats.Add;

            var imageBuilder = new OverlayImageBuilder(recordSettings);
            string folder = AttemptFileLogger.CreateSessionFolderPath(Application.persistentDataPath, recordSettings, DateTime.Now);
            _fileLogger = new AttemptFileLogger(recordSettings, folder, new IAttemptRecorder[]
            {
                new ImageAttemptRecorder(recordSettings, imageBuilder, folder),
                new StrokePointsRecorder(recordSettings, folder),
                new CsvAttemptRecorder(recordSettings, folder),
                new JsonlAttemptRecorder(recordSettings, folder),
            });
            _session.AddHandler(_fileLogger);
            _thumbnailCache = new ThumbnailCache(imageBuilder, recordSettings.ThumbnailCacheAttempts);

            drawingArea.Bind(_session, input, _sessionSettings);
            timerView.Bind(_session);
            resultPanel.Bind(_session, _stats, _sessionSettings, _judgeSettings, recordSettings, imageBuilder);
            historyList.Bind(_session, _thumbnailCache, recordSettings, fontApplier);

            resultPanel.RetryClicked += TryStart;
            resultPanel.OpenFolderClicked += OpenLogFolder;
            resultPanel.ClearListClicked += ClearList;
            resultPanel.GuideAlphaChanged += drawingArea.ShowTemplate;
            startButton.onClick.AddListener(TryStart);
            saveTemplateButton.onClick.AddListener(SaveLastAsTemplate);
            saveTemplateButton.gameObject.SetActive(TemplateAssetSaver.IsSupported);
            _session.StateChanged += OnStateChanged;

            templateNameText.text = $"도안: {template.DisplayName}";
            Notice(recordSettings.RecordEnabled ? $"기록 폴더: {folder}" : "파일 기록 꺼짐 (RecordSettings)");
            OnStateChanged(_session.State);
        }

        private void Update()
        {
            if (_session == null) return;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.spaceKey.wasPressedThisFrame) TryStart();
                if (keyboard.escapeKey.wasPressedThisFrame && _session.State != DrawingState.Idle && _session.State != DrawingState.Result)
                {
                    _session.Cancel();
                    Notice("테스트를 취소했습니다 (기록하지 않음).");
                }
            }
            _session.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            _thumbnailCache?.Clear();
            if (_sessionSettings != null) Destroy(_sessionSettings);
            if (_judgeSettings != null) Destroy(_judgeSettings);
        }

        private void TryStart()
        {
            // 버튼이 선택된 채로 남으면 Space가 버튼까지 눌러서 선택을 푼다.
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            _session.Start();
        }

        private void OnStateChanged(DrawingState state)
        {
            startButton.interactable = _session.CanStart;
            saveTemplateButton.interactable = state == DrawingState.Result && _session.LastAttempt != null && !_session.LastAttempt.IsEmpty;
        }

        private void ClearList()
        {
            historyList.Clear();
            _stats.Clear();
            Notice("기록 리스트와 세션 요약을 지웠습니다 (파일은 그대로).");
        }

        private void OpenLogFolder()
        {
            string path = Directory.Exists(_fileLogger.SessionFolder)
                ? _fileLogger.SessionFolder
                : Path.GetDirectoryName(_fileLogger.SessionFolder);
            Directory.CreateDirectory(path);
            Application.OpenURL(new Uri(path).AbsoluteUri);
        }

        private void SaveLastAsTemplate()
        {
            var last = _session.LastAttempt;
            if (last == null || last.IsEmpty)
            {
                Notice("저장할 그림이 없습니다.");
                return;
            }
            string path = TemplateAssetSaver.Save(last.Drawn, _session.Template.GuideColor, templateLibrary);
            Notice(path != null ? $"도안으로 저장했습니다: {path}" : "에디터에서만 저장할 수 있습니다.");
        }

        private void Notice(string message) => noticeText.text = message;
    }
}
