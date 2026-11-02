// Tools/SpellDrawing/Build Test Scene: DrawingTest 씬, HistoryEntry 프리팹, RecordSettings 에셋을 코드로 만들고 참조를 연결한다.
// Debug 컴포넌트를 직접 다뤄야 해서 Editor 어셈블리 대신 Debug 어셈블리에 UNITY_EDITOR 전용으로 둔다.
#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace VoxelMagic.SpellDrawing.DebugUI
{
    public static class DrawingTestSceneBuilder
    {
        private const string Root = "Assets/SpellDrawing";
        private const string ScenePath = Root + "/Scenes/DrawingTest.unity";
        private const string PrefabPath = Root + "/Debug/Prefabs/HistoryEntry.prefab";
        private const string RecordSettingsPath = Root + "/Data/Settings/RecordSettings.asset";

        private static readonly Color PanelColor = new Color(0.11f, 0.12f, 0.16f, 0.95f);
        private static readonly Color TextColor = new Color(0.92f, 0.93f, 0.96f);
        private static readonly Color SubTextColor = new Color(0.7f, 0.73f, 0.8f);

        private static Font _font;
        private static Sprite _uiSprite;
        private static Sprite _backgroundSprite;
        private static Sprite _knobSprite;

        [MenuItem("Tools/SpellDrawing/Build Test Scene")]
        public static void Build()
        {
            // DrawingTest 씬 자체는 어차피 새로 만들므로, 다른 씬이 열려 있을 때만 저장 여부를 묻는다.
            bool rebuildingSelf = SceneManager.GetActiveScene().path == ScenePath;
            if (!rebuildingSelf && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorApplication.ExecuteMenuItem("Tools/SpellDrawing/Create Default Assets");
            LoadBuiltinResources();
            LoadOrCreate<RecordSettings>(RecordSettingsPath);
            BuildHistoryEntryPrefab();

            // 새 씬을 열면 쓰지 않는 메모리 객체가 정리되므로, 에셋은 씬을 연 뒤 경로로 다시 불러온다.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var recordSettings = AssetDatabase.LoadAssetAtPath<RecordSettings>(RecordSettingsPath);
            var entryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath).GetComponent<HistoryEntryView>();
            BuildCamera();
            BuildEventSystem();

            var system = new GameObject("SpellDrawingSystem");
            var bootstrap = system.AddComponent<SpellDrawingBootstrap>();
            var mouseInput = system.AddComponent<MouseStrokeInput>();

            var canvas = BuildCanvas(out var fontApplier);
            var top = BuildTopBar(canvas);
            var area = BuildDrawingArea(canvas);
            var timer = BuildTimer(canvas, area.countdown);
            var result = BuildResultPanel(canvas);
            var history = BuildHistoryPanel(canvas, entryPrefab);
            BuildControlsHint(canvas);

            Wire(bootstrap, "templateLibrary", AssetDatabase.LoadAssetAtPath<SpellTemplateLibrary>(Root + "/Data/Templates/SpellTemplateLibrary.asset"));
            Wire(bootstrap, "sessionSettings", AssetDatabase.LoadAssetAtPath<DrawingSessionSettings>(Root + "/Data/Settings/DrawingSessionSettings.asset"));
            Wire(bootstrap, "judgeSettings", AssetDatabase.LoadAssetAtPath<JudgeSettings>(Root + "/Data/Settings/JudgeSettings.asset"));
            Wire(bootstrap, "recordSettings", recordSettings);
            Wire(bootstrap, "strokeInputSource", mouseInput);
            Wire(bootstrap, "drawingArea", area.view);
            Wire(bootstrap, "timerView", timer);
            Wire(bootstrap, "resultPanel", result);
            Wire(bootstrap, "historyList", history);
            Wire(bootstrap, "fontApplier", fontApplier);
            Wire(bootstrap, "templateNameText", top.templateName);
            Wire(bootstrap, "noticeText", top.notice);
            Wire(bootstrap, "startButton", top.start);
            Wire(bootstrap, "saveTemplateButton", top.saveTemplate);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log($"[SpellDrawing] 테스트 씬 생성 완료: {ScenePath}");
        }

        // ───────────── 씬 기본 ─────────────

        private static void BuildCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.06f, 0.08f);
            cam.orthographic = true;
            go.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static void BuildEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        private static RectTransform BuildCanvas(out KoreanFontApplier fontApplier)
        {
            var go = new GameObject("Canvas", typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand; // 어떤 화면비에서도 1920×1080 배치가 다 보이게
            go.AddComponent<GraphicRaycaster>();
            fontApplier = go.AddComponent<KoreanFontApplier>();
            return (RectTransform)go.transform;
        }

        // ───────────── 화면 구역 ─────────────

        private static (Text templateName, Text notice, Button start, Button saveTemplate) BuildTopBar(RectTransform canvas)
        {
            var templateName = CreateText(canvas, "TemplateNameText", "도안: -", 26, TextAnchor.MiddleLeft);
            PlaceTopLeft(templateName.rectTransform, 30, 20, 620, 56);

            var start = CreateButton(canvas, "StartButton", "시작 (Space)", 24);
            Place(start.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(-170f, -20f), new Vector2(220f, 56f));

            var save = CreateButton(canvas, "SaveTemplateButton", "방금 그린 것을 도안으로 저장", 20);
            Place(save.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(110f, -20f), new Vector2(320f, 56f));

            var notice = CreateText(canvas, "NoticeText", string.Empty, 17, TextAnchor.MiddleCenter, SubTextColor);
            Place(notice.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -84f), new Vector2(1100f, 30f));
            return (templateName, notice, start, save);
        }

        private static (DrawingAreaView view, Text countdown) BuildDrawingArea(RectTransform canvas)
        {
            var stateText = CreateText(canvas, "StateText", "시작 버튼 또는 Space로 시작", 30, TextAnchor.MiddleCenter);
            Place(stateText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 320f), new Vector2(640f, 50f));

            var areaImage = CreateImage(canvas, "DrawingArea", new Color(0.08f, 0.09f, 0.12f, 1f));
            var area = areaImage.rectTransform;
            Place(area, new Vector2(0.5f, 0.5f), new Vector2(0f, -30f), new Vector2(640f, 640f));
            area.gameObject.AddComponent<RectMask2D>();

            var guide = CreatePolyline(area, "GuideLines", 7f, false);
            var drawn = CreatePolyline(area, "DrawnLines", 5f, true);
            drawn.color = new Color(1f, 0.82f, 0.4f, 1f);

            var countdown = CreateText(area, "CountdownText", "3", 220, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.9f));
            Stretch(countdown.rectTransform);
            countdown.gameObject.SetActive(false);

            var view = area.gameObject.AddComponent<DrawingAreaView>();
            Wire(view, "area", area);
            Wire(view, "guideLines", guide);
            Wire(view, "drawnLines", drawn);
            return (view, countdown);
        }

        private static TimerView BuildTimer(RectTransform canvas, Text countdown)
        {
            var panel = CreateImage(canvas, "TimerPanel", PanelColor).rectTransform;
            PlaceTopLeft(panel, 30, 110, 580, 130);

            var timeText = CreateText(panel, "TimeText", "0.0초", 60, TextAnchor.MiddleLeft);
            timeText.fontStyle = FontStyle.Bold;
            PlaceTopLeft(timeText.rectTransform, 20, 8, 540, 72);

            var fill = CreateBar(panel, "TimeBar", out var barBg, new Color(0.4f, 0.75f, 1f));
            PlaceTopLeft(barBg, 20, 90, 540, 24);

            var view = panel.gameObject.AddComponent<TimerView>();
            Wire(view, "timeText", timeText);
            Wire(view, "timeBarFill", fill);
            Wire(view, "countdownText", countdown);
            Wire(view, "stateText", canvas.Find("StateText").GetComponent<Text>());
            return view;
        }

        private static ResultPanelView BuildResultPanel(RectTransform canvas)
        {
            var panel = CreateImage(canvas, "ResultPanel", PanelColor).rectTransform;
            PlaceTopLeft(panel, 30, 255, 580, 805);
            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 14, 14);
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Sized(CreateText(panel, "Title", "결과", 22, TextAnchor.MiddleLeft, SubTextColor).gameObject, 28);
            var similarity = CreateText(panel, "SimilarityText", "-", 56, TextAnchor.MiddleLeft);
            similarity.fontStyle = FontStyle.Bold;
            Sized(similarity.gameObject, 70);
            var barFill = CreateBar(panel, "SimilarityBar", out var barBg, new Color(0.35f, 0.85f, 0.45f));
            Sized(barBg.gameObject, 22);
            var detail = CreateText(panel, "DetailText", string.Empty, 17, TextAnchor.UpperLeft);
            Sized(detail.gameObject, 84);
            var measure = CreateText(panel, "MeasureText", string.Empty, 17, TextAnchor.UpperLeft, SubTextColor);
            Sized(measure.gameObject, 56);

            var overlayRow = CreateRow(panel, "OverlayRow", 200, 12f);
            var overlay = CreateRawImage(overlayRow, "OverlayImage", 200);
            var legend = CreateText(overlayRow, "LegendText", string.Empty, 17, TextAnchor.MiddleLeft);
            legend.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var summary = CreateText(panel, "SummaryText", string.Empty, 17, TextAnchor.MiddleLeft);
            Sized(summary.gameObject, 50);

            var (timeSlider, timeLabel) = CreateSliderRow(panel, "TimeLimit", 3f, 20f);
            var (toleranceSlider, toleranceLabel) = CreateSliderRow(panel, "Tolerance", 0.02f, 0.2f);
            var (alphaSlider, alphaLabel) = CreateSliderRow(panel, "GuideAlpha", 0f, 1f);

            var buttons = CreateRow(panel, "Buttons", 48, 10f);
            var retry = CreateButton(buttons, "RetryButton", "다시 하기", 20);
            var openFolder = CreateButton(buttons, "OpenFolderButton", "로그 폴더 열기", 20);
            var clear = CreateButton(buttons, "ClearListButton", "리스트 지우기", 20);
            foreach (var b in new[] { retry, openFolder, clear })
            {
                var le = b.gameObject.AddComponent<LayoutElement>();
                le.flexibleWidth = 1f;
                le.preferredHeight = 44f;
            }

            var view = panel.gameObject.AddComponent<ResultPanelView>();
            Wire(view, "similarityText", similarity);
            Wire(view, "similarityBarFill", barFill);
            Wire(view, "detailText", detail);
            Wire(view, "measureText", measure);
            Wire(view, "overlayImage", overlay);
            Wire(view, "overlayLegendText", legend);
            Wire(view, "summaryText", summary);
            Wire(view, "timeLimitSlider", timeSlider);
            Wire(view, "timeLimitLabel", timeLabel);
            Wire(view, "toleranceSlider", toleranceSlider);
            Wire(view, "toleranceLabel", toleranceLabel);
            Wire(view, "guideAlphaSlider", alphaSlider);
            Wire(view, "guideAlphaLabel", alphaLabel);
            Wire(view, "retryButton", retry);
            Wire(view, "openFolderButton", openFolder);
            Wire(view, "clearListButton", clear);
            return view;
        }

        private static HistoryListView BuildHistoryPanel(RectTransform canvas, HistoryEntryView entryPrefab)
        {
            var panel = CreateImage(canvas, "HistoryPanel", PanelColor).rectTransform;
            Place(panel, new Vector2(1f, 1f), new Vector2(-30f, -110f), new Vector2(600f, 950f));

            var title = CreateText(panel, "Title", "기록 (최신이 위)", 22, TextAnchor.MiddleLeft);
            PlaceTopLeft(title.rectTransform, 16, 10, 360, 40);
            var collapse = CreateButton(panel, "CollapseAllButton", "모두 접기", 18);
            Place(collapse.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(-16f, -10f), new Vector2(140f, 40f));

            var scrollGo = CreateUI(panel, "Scroll");
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = new Vector2(10f, 10f);
            scrollRt.offsetMax = new Vector2(-10f, -60f);
            var scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;

            var viewport = CreateUI(scrollRt, "Viewport").GetComponent<RectTransform>();
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = CreateUI(viewport, "Content").GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            var contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 6f;
            contentLayout.padding = new RectOffset(2, 2, 2, 2);
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport;
            scroll.content = content;

            var view = panel.gameObject.AddComponent<HistoryListView>();
            Wire(view, "content", content);
            Wire(view, "entryPrefab", entryPrefab);
            Wire(view, "collapseAllButton", collapse);
            Wire(view, "scrollRect", scroll);
            return view;
        }

        private static void BuildControlsHint(RectTransform canvas)
        {
            var hint = CreateText(canvas, "ControlsHint",
                "<b>조작</b>\n시작 버튼 / Space : 새 테스트 시작\n마우스 왼쪽 버튼을 누른 채 이동 : 획 그리기 (제한 시간 동안만)\nEsc : 진행 중인 테스트 취소 (기록하지 않음)",
                18, TextAnchor.UpperCenter, SubTextColor);
            Place(hint.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -400f), new Vector2(680f, 110f));
        }

        // ───────────── 기록 토글 프리팹 ─────────────

        private static HistoryEntryView BuildHistoryEntryPrefab()
        {
            var root = new GameObject("HistoryEntry", typeof(RectTransform));
            var rootRt = (RectTransform)root.transform;
            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.16f, 0.17f, 0.22f, 1f);
            var layout = root.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(4, 4, 4, 4);
            layout.spacing = 4f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var headerImage = CreateImage(rootRt, "Header", new Color(1f, 1f, 1f, 0.03f));
            Sized(headerImage.gameObject, 40);
            var toggle = headerImage.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = headerImage;
            toggle.transition = Selectable.Transition.ColorTint;
            var header = headerImage.rectTransform;

            var scoreBar = CreateImage(header, "ScoreBar", Color.white);
            scoreBar.raycastTarget = false;
            var scoreRt = scoreBar.rectTransform;
            scoreRt.anchorMin = new Vector2(0f, 0f);
            scoreRt.anchorMax = new Vector2(0f, 1f);
            scoreRt.pivot = new Vector2(0f, 0.5f);
            scoreRt.sizeDelta = new Vector2(8f, 0f);
            scoreRt.anchoredPosition = Vector2.zero;

            var arrow = CreateText(header, "ArrowText", "▶", 16, TextAnchor.MiddleCenter, SubTextColor);
            StretchHorizontal(arrow.rectTransform, 14f, 30f, true);
            var summary = CreateText(header, "SummaryText", "#1  00:00:00  유사도 0.00", 18, TextAnchor.MiddleLeft);
            StretchHorizontal(summary.rectTransform, 42f, 8f, false);

            var details = CreateRow(rootRt, "Details", 140, 6f);
            var templateImage = CreateRawImage(details, "TemplateImage", 130, "원본");
            var drawnImage = CreateRawImage(details, "DrawnImage", 130, "그린 것");
            var overlayImage = CreateRawImage(details, "OverlayImage", 130, "겹쳐 보기");
            var detailText = CreateText(details, "DetailText", string.Empty, 14, TextAnchor.UpperLeft);
            detailText.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            details.gameObject.SetActive(false);

            var view = root.AddComponent<HistoryEntryView>();
            Wire(view, "toggle", toggle);
            Wire(view, "scoreBar", scoreBar);
            Wire(view, "summaryText", summary);
            Wire(view, "arrowText", arrow);
            Wire(view, "details", details.gameObject);
            Wire(view, "templateImage", templateImage);
            Wire(view, "drawnImage", drawnImage);
            Wire(view, "overlayImage", overlayImage);
            Wire(view, "detailText", detailText);

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(PrefabPath));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<HistoryEntryView>();
        }

        // ───────────── UI 도우미 ─────────────

        private static void LoadBuiltinResources()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            _backgroundSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            _knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        }

        private static GameObject CreateUI(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static Text CreateText(Transform parent, string name, string text, int size, TextAnchor anchor, Color? color = null)
        {
            var t = CreateUI(parent, name).AddComponent<Text>();
            t.font = _font;
            t.text = text;
            t.fontSize = size;
            t.alignment = anchor;
            t.color = color ?? TextColor;
            t.supportRichText = true;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            var image = CreateUI(parent, name).AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static RawImage CreateRawImage(Transform parent, string name, float size, string caption = null)
        {
            var raw = CreateUI(parent, name).AddComponent<RawImage>();
            raw.raycastTarget = false;
            var le = raw.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = size;
            le.preferredHeight = size;
            le.minWidth = size;
            if (caption != null)
            {
                var label = CreateText(raw.transform, "Caption", caption, 12, TextAnchor.LowerCenter, SubTextColor);
                Stretch(label.rectTransform);
            }
            return raw;
        }

        private static UIPolylineGraphic CreatePolyline(Transform parent, string name, float thickness, bool extendEnds)
        {
            var go = CreateUI(parent, name);
            Stretch((RectTransform)go.transform);
            var line = go.AddComponent<UIPolylineGraphic>();
            line.raycastTarget = false;
            Wire(line, "thickness", thickness);
            Wire(line, "extendEnds", extendEnds);
            return line;
        }

        private static Button CreateButton(Transform parent, string name, string label, int fontSize)
        {
            var image = CreateImage(parent, name, new Color(0.25f, 0.42f, 0.75f, 1f));
            image.sprite = _uiSprite;
            image.type = Image.Type.Sliced;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var text = CreateText(image.transform, "Label", label, fontSize, TextAnchor.MiddleCenter, Color.white);
            Stretch(text.rectTransform);
            return button;
        }

        // 배경 이미지 + 가로로 채워지는 Fill 이미지. Fill을 돌려준다.
        private static Image CreateBar(Transform parent, string name, out RectTransform background, Color fillColor)
        {
            var bg = CreateImage(parent, name, new Color(1f, 1f, 1f, 0.08f));
            background = bg.rectTransform;
            var fill = CreateImage(bg.transform, "Fill", fillColor);
            Stretch(fill.rectTransform);
            fill.sprite = _uiSprite;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;
            return fill;
        }

        private static RectTransform CreateRow(Transform parent, string name, float height, float spacing)
        {
            var row = CreateUI(parent, name);
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            Sized(row, height);
            return (RectTransform)row.transform;
        }

        private static (Slider slider, Text label) CreateSliderRow(Transform parent, string name, float min, float max)
        {
            var row = CreateRow(parent, name + "Row", 34, 10f);
            var label = CreateText(row, "Label", name, 17, TextAnchor.MiddleLeft);
            var le = label.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 270f;
            le.minWidth = 270f;

            var resources = new DefaultControls.Resources { standard = _uiSprite, background = _backgroundSprite, knob = _knobSprite };
            var sliderGo = DefaultControls.CreateSlider(resources);
            sliderGo.name = name + "Slider";
            sliderGo.transform.SetParent(row, false);
            var sliderLe = sliderGo.AddComponent<LayoutElement>();
            sliderLe.flexibleWidth = 1f;
            sliderLe.preferredHeight = 20f;
            var slider = sliderGo.GetComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            return (slider, label);
        }

        private static void Sized(GameObject go, float height)
        {
            if (!go.TryGetComponent(out LayoutElement le)) le = go.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;
        }

        private static void Place(RectTransform rt, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
        }

        // 왼쪽 위 기준 (x, y는 위에서 아래로).
        private static void PlaceTopLeft(RectTransform rt, float x, float y, float width, float height) =>
            Place(rt, new Vector2(0f, 1f), new Vector2(x, -y), new Vector2(width, height));

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // 세로는 부모에 맞추고, 가로는 왼쪽 여백부터. fixedWidth면 rightOrWidth가 너비, 아니면 오른쪽 여백.
        private static void StretchHorizontal(RectTransform rt, float left, float rightOrWidth, bool fixedWidth)
        {
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(fixedWidth ? 0f : 1f, 1f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.offsetMin = new Vector2(left, 0f);
            rt.offsetMax = fixedWidth ? new Vector2(left + rightOrWidth, 0f) : new Vector2(-rightOrWidth, 0f);
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private static void Wire(Object target, string field, object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field) ?? throw new ArgumentException($"{target.GetType().Name}.{field} 필드가 없습니다");
            switch (value)
            {
                case Object obj: prop.objectReferenceValue = obj; break;
                case float f: prop.floatValue = f; break;
                case bool b: prop.boolValue = b; break;
                default: throw new ArgumentException($"지원하지 않는 값: {value}");
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
