// 모든 획을 합친 선 길이를 따라 같은 간격의 점 N개로 다시 뽑는다. 그리는 속도와 상관없게 만들고, 획 구분은 유지한다.
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.SpellDrawing
{
    public static class StrokeResampler
    {
        private const float Epsilon = 1e-6f;

        public static StrokeSet Resample(StrokeSet source, int count)
        {
            if (source == null || source.IsEmpty) return StrokeSet.Empty;
            count = Mathf.Max(2, count);
            float total = source.TotalLength;
            if (total <= Epsilon) return ResampleDots(source, count);

            float interval = total / (count - 1);
            var output = new List<List<Vector2>>();
            int emitted = 0;
            float walked = 0f;
            float nextTarget = 0f;

            for (int s = 0; s < source.StrokeCount; s++)
            {
                var stroke = source.GetStroke(s);
                var current = new List<Vector2>();
                for (int i = 1; i < stroke.Count; i++)
                {
                    Vector2 a = stroke[i - 1], b = stroke[i];
                    float len = Vector2.Distance(a, b);
                    if (len <= 0f) continue;
                    while (emitted < count && nextTarget <= walked + len)
                    {
                        float t = Mathf.Clamp01((nextTarget - walked) / len);
                        current.Add(Vector2.Lerp(a, b, t));
                        emitted++;
                        nextTarget = emitted * interval;
                    }
                    walked += len;
                }
                if (current.Count > 0) output.Add(current);
            }

            // 부동소수점 오차로 마지막 점이 빠지면 끝점으로 채운다.
            var lastStroke = source.GetStroke(source.StrokeCount - 1);
            var lastPoint = lastStroke[lastStroke.Count - 1];
            if (output.Count == 0) output.Add(new List<Vector2>());
            while (emitted < count)
            {
                output[output.Count - 1].Add(lastPoint);
                emitted++;
            }
            return new StrokeSet(output);
        }

        // 길이가 0인 그림(점만 찍음): 찍은 점마다 N개를 나눠 준다.
        private static StrokeSet ResampleDots(StrokeSet source, int count)
        {
            var points = new List<Vector2>(source.AllPoints());
            var output = new List<Vector2[]>();
            for (int i = 0; i < points.Count; i++)
            {
                int n = count / points.Count + (i < count % points.Count ? 1 : 0);
                if (n == 0) continue;
                var stroke = new Vector2[n];
                for (int k = 0; k < n; k++) stroke[k] = points[i];
                output.Add(stroke);
            }
            return new StrokeSet(output);
        }
    }
}
