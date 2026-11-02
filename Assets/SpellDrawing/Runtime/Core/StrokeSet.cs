// 여러 획의 점 목록을 담는 불변 데이터. 그린 그림과 원본 도안을 모두 이 형식(그리기 공간 0~1 좌표)으로 다룬다.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.SpellDrawing
{
    public sealed class StrokeSet
    {
        public static readonly StrokeSet Empty = new StrokeSet(Array.Empty<Vector2[]>());

        private readonly Vector2[][] _strokes;

        public int StrokeCount => _strokes.Length;
        public int PointCount { get; }
        public float TotalLength { get; }
        public bool IsEmpty => PointCount == 0;

        // 점이 하나도 없는 획은 버린다.
        public StrokeSet(IEnumerable<IReadOnlyList<Vector2>> strokes)
        {
            var list = new List<Vector2[]>();
            int pointCount = 0;
            float length = 0f;
            if (strokes != null)
            {
                foreach (var stroke in strokes)
                {
                    if (stroke == null || stroke.Count == 0) continue;
                    var copy = new Vector2[stroke.Count];
                    for (int i = 0; i < copy.Length; i++)
                    {
                        copy[i] = stroke[i];
                        if (i > 0) length += Vector2.Distance(copy[i - 1], copy[i]);
                    }
                    list.Add(copy);
                    pointCount += copy.Length;
                }
            }
            _strokes = list.ToArray();
            PointCount = pointCount;
            TotalLength = length;
        }

        public IReadOnlyList<Vector2> GetStroke(int index) => _strokes[index];

        public IEnumerable<Vector2> AllPoints()
        {
            foreach (var stroke in _strokes)
                foreach (var p in stroke)
                    yield return p;
        }

        public Rect GetBounds()
        {
            if (IsEmpty) return Rect.zero;
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            foreach (var p in AllPoints())
            {
                min = Vector2.Min(min, p);
                max = Vector2.Max(max, p);
            }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        public StrokeSet Select(Func<Vector2, Vector2> transform)
        {
            var result = new Vector2[_strokes.Length][];
            for (int s = 0; s < _strokes.Length; s++)
            {
                result[s] = new Vector2[_strokes[s].Length];
                for (int i = 0; i < result[s].Length; i++)
                    result[s][i] = transform(_strokes[s][i]);
            }
            return new StrokeSet(result);
        }
    }
}
