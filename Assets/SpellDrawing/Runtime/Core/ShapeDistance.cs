// 점에서 모양(획들의 선분)까지 가장 가까운 거리를 구한다. 유사도 계산과 겹쳐 보기 색칠이 함께 쓴다.
using UnityEngine;

namespace VoxelMagic.SpellDrawing
{
    public static class ShapeDistance
    {
        public static float ToShape(Vector2 point, StrokeSet shape)
        {
            float best = float.MaxValue;
            for (int s = 0; s < shape.StrokeCount; s++)
            {
                var stroke = shape.GetStroke(s);
                if (stroke.Count == 1)
                {
                    best = Mathf.Min(best, (point - stroke[0]).sqrMagnitude);
                    continue;
                }
                for (int i = 1; i < stroke.Count; i++)
                    best = Mathf.Min(best, SegmentDistanceSqr(point, stroke[i - 1], stroke[i]));
            }
            return best == float.MaxValue ? float.MaxValue : Mathf.Sqrt(best);
        }

        public static float SegmentDistanceSqr(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lenSqr = ab.sqrMagnitude;
            if (lenSqr <= 0f) return (p - a).sqrMagnitude;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lenSqr);
            return (p - (a + ab * t)).sqrMagnitude;
        }
    }
}
