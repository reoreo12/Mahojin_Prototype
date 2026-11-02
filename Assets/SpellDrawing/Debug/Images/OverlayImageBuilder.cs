// 시도 하나로 원본·그린 것·겹쳐 보기 픽셀을 만든다. 겹쳐 보기는 빠뜨린 원본(빨강)과 삐져나온 선(주황)을 따로 칠한다.
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.SpellDrawing.DebugUI
{
    public sealed class OverlayImageBuilder
    {
        private readonly RecordSettings _settings;
        private readonly StrokeRasterizer _raster;

        public int Size => _raster.Width;

        public OverlayImageBuilder(RecordSettings settings)
        {
            _settings = settings;
            _raster = new StrokeRasterizer(settings.ThumbnailSize, settings.ThumbnailSize);
        }

        public Color32[] BuildTemplate(AttemptRecord record)
        {
            _raster.Clear(_settings.Background);
            _raster.DrawStrokes(record.Template, _settings.LineThicknessPx, _settings.TemplateLine);
            return Snapshot();
        }

        public Color32[] BuildDrawn(AttemptRecord record)
        {
            _raster.Clear(_settings.Background);
            _raster.DrawStrokes(record.Drawn, _settings.LineThicknessPx, _settings.DrawnLine);
            return Snapshot();
        }

        public Color32[] BuildOverlay(AttemptRecord record)
        {
            float hitDistance = record.Result.Tolerance * _settings.OverlayHitRatio;
            _raster.Clear(_settings.Background);
            DrawColoredByDistance(record.Template, record.Drawn, hitDistance, _settings.Covered, _settings.Missed);
            DrawColoredByDistance(record.Drawn, record.Template, hitDistance, _settings.OnTarget, _settings.Stray);
            return Snapshot();
        }

        public Texture2D CreateTexture(Color32[] pixels)
        {
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            WriteTo(texture, pixels);
            return texture;
        }

        public void WriteTo(Texture2D texture, Color32[] pixels)
        {
            texture.SetPixels32(pixels);
            texture.Apply(false);
        }

        // 선분마다 가운데 점에서 상대 모양까지 거리를 재서, 가까우면 near 색, 멀면 far 색.
        private void DrawColoredByDistance(StrokeSet shape, StrokeSet other, float hitDistance, Color32 near, Color32 far)
        {
            float thickness = _settings.LineThicknessPx;
            for (int s = 0; s < shape.StrokeCount; s++)
            {
                IReadOnlyList<Vector2> stroke = shape.GetStroke(s);
                if (stroke.Count == 1)
                {
                    _raster.DrawSegment(stroke[0], stroke[0], thickness, Pick(stroke[0], other, hitDistance, near, far));
                    continue;
                }
                for (int i = 1; i < stroke.Count; i++)
                {
                    Vector2 mid = (stroke[i - 1] + stroke[i]) * 0.5f;
                    _raster.DrawSegment(stroke[i - 1], stroke[i], thickness, Pick(mid, other, hitDistance, near, far));
                }
            }
        }

        private static Color32 Pick(Vector2 point, StrokeSet other, float hitDistance, Color32 near, Color32 far) =>
            !other.IsEmpty && ShapeDistance.ToShape(point, other) <= hitDistance ? near : far;

        private Color32[] Snapshot() => (Color32[])_raster.Pixels.Clone();
    }
}
