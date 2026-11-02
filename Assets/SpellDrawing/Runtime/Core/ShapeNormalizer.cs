// 무게중심을 원점으로 옮기고 가장 긴 변이 1이 되게 크기를 맞춘다(비율 유지). 위치·크기를 지우고 모양만 남긴다.
using UnityEngine;

namespace VoxelMagic.SpellDrawing
{
    public static class ShapeNormalizer
    {
        private const float Epsilon = 1e-6f;

        public static StrokeSet Normalize(StrokeSet shape)
        {
            if (shape == null || shape.IsEmpty) return StrokeSet.Empty;
            Vector2 centroid = GetCentroid(shape);
            float size = GetSize(shape);
            float scale = size > Epsilon ? 1f / size : 1f;
            return shape.Select(p => (p - centroid) * scale);
        }

        // 리샘플링된 점이면 선 길이 기준 무게중심과 같다.
        public static Vector2 GetCentroid(StrokeSet shape)
        {
            if (shape.IsEmpty) return Vector2.zero;
            Vector2 sum = Vector2.zero;
            foreach (var p in shape.AllPoints()) sum += p;
            return sum / shape.PointCount;
        }

        public static float GetSize(StrokeSet shape)
        {
            var bounds = shape.GetBounds();
            return Mathf.Max(bounds.width, bounds.height);
        }
    }
}
