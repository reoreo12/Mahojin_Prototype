// 결과 패널: 유사도·세부 값·측정값·겹쳐 보기·세션 요약과 설정 슬라이더, 버튼. 세션·통계 이벤트를 구독해서만 갱신한다.
using System;
using UnityEngine;
using UnityEngine.UI;

namespace VoxelMagic.SpellDrawing.DebugUI
{
    public sealed class ResultPanelView : MonoBehaviour
    {
        [Header("결과")]
        [SerializeField] private Text similarityText;
        [SerializeField] private Image similarityBarFill;
        [SerializeField] private Text detailText;
        [SerializeField] private Text measureText;
        [SerializeField] private RawImage overlayImage;
        [SerializeField] private Text overlayLegendText;
        [SerializeField] private Text summaryText;

        [Header("설정 슬라이더 (범위는 Slider에서 조절)")]
        [SerializeField] private Slider timeLimitSlider;
        [SerializeField] private Text timeLimitLabel;
        [SerializeField] private Slider toleranceSlider;
        [SerializeField] private Text toleranceLabel;
        [SerializeField] private Slider guideAlphaSlider;
        [SerializeField] private Text guideAlphaLabel;

        [Header("버튼")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button openFolderButton;
        [SerializeField] private Button clearListButton;

        private DrawingSession _session;
        private SessionStats _stats;
        private DrawingSessionSettings _sessionSettings;
        private JudgeSettings _judgeSettings;
        private RecordSettings _recordSettings;
        private OverlayImageBuilder _imageBuilder;
        private Texture2D _overlayTexture;

        public event Action RetryClicked;
        public event Action OpenFolderClicked;
        public event Action ClearListClicked;
        public event Action GuideAlphaChanged;

        public void Bind(DrawingSession session, SessionStats stats, DrawingSessionSettings sessionSettings,
            JudgeSettings judgeSettings, RecordSettings recordSettings, OverlayImageBuilder imageBuilder)
        {
            _session = session;
            _stats = stats;
            _sessionSettings = sessionSettings;
            _judgeSettings = judgeSettings;
            _recordSettings = recordSettings;
            _imageBuilder = imageBuilder;

            _session.AttemptCompleted += Show;
            _stats.Changed += RefreshSummary;

            retryButton.onClick.AddListener(() => RetryClicked?.Invoke());
            openFolderButton.onClick.AddListener(() => OpenFolderClicked?.Invoke());
            clearListButton.onClick.AddListener(() => ClearListClicked?.Invoke());

            InitSlider(timeLimitSlider, _sessionSettings.TimeLimitSec, v => { _sessionSettings.SetTimeLimitSec(v); RefreshSliderLabels(); });
            InitSlider(toleranceSlider, _judgeSettings.Tolerance, v => { _judgeSettings.SetTolerance(v); RefreshSliderLabels(); });
            InitSlider(guideAlphaSlider, _sessionSettings.GuideAlpha, v =>
            {
                _sessionSettings.SetGuideAlpha(v);
                RefreshSliderLabels();
                GuideAlphaChanged?.Invoke();
            });

            overlayLegendText.text =
                $"{Swatch(_recordSettings.Covered)} 원본: 그림\n{Swatch(_recordSettings.Missed)} 원본: 빠뜨림\n" +
                $"{Swatch(_recordSettings.OnTarget)} 내 선: 맞음\n{Swatch(_recordSettings.Stray)} 내 선: 삐져나옴";
            ShowEmptyState();
            RefreshSummary();
            RefreshSliderLabels();
        }

        private void OnDestroy()
        {
            if (_session != null) _session.AttemptCompleted -= Show;
            if (_stats != null) _stats.Changed -= RefreshSummary;
            if (_overlayTexture != null) Destroy(_overlayTexture);
        }

        private void Show(AttemptRecord record)
        {
            var result = record.Result;
            float similarity = result.Similarity;
            similarityText.text = record.IsEmpty ? "그린 획 없음  0.00" : $"{similarity:F2}  ({similarity * 100f:F0}%)";
            similarityBarFill.fillAmount = similarity;
            similarityBarFill.color = _recordSettings.ScoreColor(similarity);

            detailText.text =
                $"커버리지 {result.Coverage:F2}  ·  정밀도 {result.Precision:F2}   (비정규화 = 주 점수)\n" +
                $"정규화 점수 {result.Normalized.Similarity:F2}  (커버리지 {result.Normalized.Coverage:F2} · 정밀도 {result.Normalized.Precision:F2})\n" +
                $"비정규화 점수 {result.Raw.Similarity:F2}  ·  허용 거리 {result.Tolerance:F3}";
            measureText.text =
                $"획 {record.StrokeCount}개  ·  원래 점 {record.RawPointCount}개  ·  리샘플링 {result.ResampleCount}개\n" +
                $"그린 시간 {record.DrawSec:F1}초 / 제한 {record.TimeLimitSec:F1}초  ·  계산 {result.JudgeMs:F2} ms";

            var pixels = _imageBuilder.BuildOverlay(record);
            if (_overlayTexture == null) _overlayTexture = _imageBuilder.CreateTexture(pixels);
            else _imageBuilder.WriteTo(_overlayTexture, pixels);
            overlayImage.texture = _overlayTexture;
            overlayImage.color = Color.white;
        }

        private void ShowEmptyState()
        {
            similarityText.text = "-";
            similarityBarFill.fillAmount = 0f;
            detailText.text = "그리기를 마치면 결과가 여기에 나옵니다.";
            measureText.text = string.Empty;
            overlayImage.texture = null;
            overlayImage.color = new Color(1f, 1f, 1f, 0.05f);
        }

        private void RefreshSummary()
        {
            summaryText.text = _stats.Count == 0
                ? "세션 요약: 아직 시도 없음"
                : $"세션 요약: 시도 {_stats.Count}회  ·  평균 {_stats.Average:F2}  ·  최고 {_stats.Best:F2}  ·  최저 {_stats.Worst:F2}";
        }

        private void RefreshSliderLabels()
        {
            timeLimitLabel.text = $"제한 시간 {_sessionSettings.TimeLimitSec:F1}초";
            toleranceLabel.text = $"허용 거리 {_judgeSettings.Tolerance:F3} (영역의 {_judgeSettings.Tolerance * 100f:F1}%)";
            guideAlphaLabel.text = $"가이드 투명도 {_sessionSettings.GuideAlpha:F2}";
        }

        private static void InitSlider(Slider slider, float value, Action<float> onChanged)
        {
            slider.SetValueWithoutNotify(value);
            slider.onValueChanged.AddListener(v => onChanged(v));
        }

        private static string Swatch(Color32 c) => $"<color=#{ColorUtility.ToHtmlStringRGB(c)}>■</color>";
    }
}
