// 정사각형 그리기 영역: 화면 좌표를 그리기 공간(0~1)으로 바꿔 세션에 넘기고, 반투명 가이드와 그린 선을 보여 준다.
using UnityEngine;

namespace VoxelMagic.SpellDrawing.DebugUI
{
    public sealed class DrawingAreaView : MonoBehaviour
    {
        [SerializeField] private RectTransform area;
        [SerializeField] private UIPolylineGraphic guideLines;
        [SerializeField] private UIPolylineGraphic drawnLines;

        private DrawingSession _session;
        private IStrokeInput _input;
        private DrawingSessionSettings _settings;
        private Canvas _canvas;

        public void Bind(DrawingSession session, IStrokeInput input, DrawingSessionSettings settings)
        {
            _session = session;
            _input = input;
            _settings = settings;
            _canvas = area.GetComponentInParent<Canvas>();

            _input.StrokeBegan += OnStrokeBegan;
            _input.StrokeMoved += OnStrokeMoved;
            _input.StrokeEnded += OnStrokeEnded;
            _session.StrokesChanged += OnStrokesChanged;
            _session.StateChanged += OnStateChanged;

            drawnLines.SetStrokes(_session.CurrentStrokes);
            ShowTemplate();
            UpdateAreaPixelSize();
        }

        private void OnDestroy()
        {
            if (_input != null)
            {
                _input.StrokeBegan -= OnStrokeBegan;
                _input.StrokeMoved -= OnStrokeMoved;
                _input.StrokeEnded -= OnStrokeEnded;
            }
            if (_session != null)
            {
                _session.StrokesChanged -= OnStrokesChanged;
                _session.StateChanged -= OnStateChanged;
            }
        }

        private void OnRectTransformDimensionsChange() => UpdateAreaPixelSize();

        public void ShowTemplate()
        {
            var template = _session?.Template;
            if (template == null) return;
            Color c = template.GuideColor;
            c.a = _settings.GuideAlpha;
            guideLines.color = c;
            guideLines.SetStrokes(template.Strokes);
        }

        private void OnStateChanged(DrawingState state)
        {
            if (state == DrawingState.Countdown || state == DrawingState.Drawing)
            {
                ShowTemplate();
                UpdateAreaPixelSize();
            }
        }

        private void OnStrokesChanged() => drawnLines.Refresh();

        private void OnStrokeBegan(Vector2 screen)
        {
            if (_session.IsAcceptingInput) _session.BeginStroke(ScreenToArea(screen));
        }

        private void OnStrokeMoved(Vector2 screen)
        {
            if (_session.IsAcceptingInput) _session.AddPoint(ScreenToArea(screen));
        }

        private void OnStrokeEnded(Vector2 screen)
        {
            if (_session.IsAcceptingInput) _session.EndStroke(ScreenToArea(screen));
        }

        private Vector2 ScreenToArea(Vector2 screen)
        {
            Camera cam = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay ? _canvas.worldCamera : null;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(area, screen, cam, out Vector2 local);
            Rect rect = area.rect;
            return new Vector2((local.x - rect.xMin) / rect.width, (local.y - rect.yMin) / rect.height);
        }

        private void UpdateAreaPixelSize()
        {
            if (_session == null || area == null) return;
            float scale = _canvas != null ? _canvas.scaleFactor : 1f;
            _session.SetAreaPixelSize(area.rect.width * scale);
        }
    }
}
