// 그리기 공간의 선분을 Color32 픽셀 버퍼에 굵은 선으로 그린다. 썸네일과 PNG가 함께 쓰며 Unity 객체 없이 테스트할 수 있다.
using System;
using UnityEngine;

namespace VoxelMagic.SpellDrawing
{
    public sealed class StrokeRasterizer
    {
        private readonly Color32[] _pixels;

        public int Width { get; }
        public int Height { get; }
        public Color32[] Pixels => _pixels;

        public StrokeRasterizer(int width, int height)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            _pixels = new Color32[Width * Height];
        }

        public void Clear(Color32 color) => Array.Fill(_pixels, color);

        public void DrawStrokes(StrokeSet strokes, float thicknessPx, Color32 color)
        {
            for (int s = 0; s < strokes.StrokeCount; s++)
            {
                var stroke = strokes.GetStroke(s);
                if (stroke.Count == 1) DrawSegment(stroke[0], stroke[0], thicknessPx, color);
                for (int i = 1; i < stroke.Count; i++) DrawSegment(stroke[i - 1], stroke[i], thicknessPx, color);
            }
        }

        // a, b는 그리기 공간(0~1, 아래가 0). 텍스처 픽셀 행 0도 아래라서 그대로 대응한다.
        public void DrawSegment(Vector2 a, Vector2 b, float thicknessPx, Color32 color)
        {
            Vector2 pa = new Vector2(a.x * Width, a.y * Height);
            Vector2 pb = new Vector2(b.x * Width, b.y * Height);
            float radius = Mathf.Max(0.5f, thicknessPx * 0.5f);
            int minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(pa.x, pb.x) - radius - 1));
            int maxX = Mathf.Min(Width - 1, Mathf.CeilToInt(Mathf.Max(pa.x, pb.x) + radius + 1));
            int minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(pa.y, pb.y) - radius - 1));
            int maxY = Mathf.Min(Height - 1, Mathf.CeilToInt(Mathf.Max(pa.y, pb.y) + radius + 1));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    var center = new Vector2(x + 0.5f, y + 0.5f);
                    float distance = Mathf.Sqrt(ShapeDistance.SegmentDistanceSqr(center, pa, pb));
                    float coverage = Mathf.Clamp01(radius + 0.5f - distance);
                    if (coverage <= 0f) continue;
                    Blend(x, y, color, coverage);
                }
            }
        }

        private void Blend(int x, int y, Color32 color, float coverage)
        {
            int index = y * Width + x;
            float alpha = color.a / 255f * coverage;
            Color32 dst = _pixels[index];
            _pixels[index] = new Color32(
                (byte)Mathf.RoundToInt(Mathf.Lerp(dst.r, color.r, alpha)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(dst.g, color.g, alpha)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(dst.b, color.b, alpha)),
                (byte)Mathf.RoundToInt(Mathf.Max(dst.a, alpha * 255f)));
        }
    }
}
