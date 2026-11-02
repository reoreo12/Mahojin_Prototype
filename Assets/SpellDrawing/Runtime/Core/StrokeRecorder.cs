// 입력 점을 일정 간격마다 기록해 획 목록을 만든다. 영역 밖에서 시작한 획은 무시하고, 밖으로 나간 점은 경계로 잘라낸다.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.SpellDrawing
{
    public sealed class StrokeRecorder
    {
        private readonly List<List<Vector2>> _strokes = new List<List<Vector2>>();
        private List<Vector2> _current;
        private float _minSpacing;

        public event Action Changed;

        public bool IsStroking => _current != null;
        public int StrokeCount => _strokes.Count;
        public int PointCount { get; private set; }
        public IReadOnlyList<IReadOnlyList<Vector2>> Strokes => _strokes;

        public StrokeRecorder(float minSpacing)
        {
            SetMinSpacing(minSpacing);
        }

        // 간격은 그리기 공간 단위 (영역 한 변 = 1).
        public void SetMinSpacing(float minSpacing) => _minSpacing = Mathf.Max(0f, minSpacing);

        public static bool IsInsideArea(Vector2 p) => p.x >= 0f && p.x <= 1f && p.y >= 0f && p.y <= 1f;

        public bool BeginStroke(Vector2 point)
        {
            if (IsStroking) EndStroke();
            if (!IsInsideArea(point)) return false;
            _current = new List<Vector2> { point };
            _strokes.Add(_current);
            PointCount++;
            Changed?.Invoke();
            return true;
        }

        public void AddPoint(Vector2 point)
        {
            if (!IsStroking) return;
            point = Clamp(point);
            if (Vector2.Distance(_current[_current.Count - 1], point) < _minSpacing) return;
            Append(point);
        }

        // 마지막 위치가 주어지면 간격과 상관없이 획 끝점으로 남긴다.
        public void EndStroke(Vector2? lastPoint = null)
        {
            if (!IsStroking) return;
            if (lastPoint.HasValue)
            {
                var p = Clamp(lastPoint.Value);
                if (_current[_current.Count - 1] != p) Append(p);
            }
            _current = null;
        }

        public void Clear()
        {
            _strokes.Clear();
            _current = null;
            PointCount = 0;
            Changed?.Invoke();
        }

        public StrokeSet ToStrokeSet() => new StrokeSet(_strokes);

        private void Append(Vector2 point)
        {
            _current.Add(point);
            PointCount++;
            Changed?.Invoke();
        }

        private static Vector2 Clamp(Vector2 p) => new Vector2(Mathf.Clamp01(p.x), Mathf.Clamp01(p.y));
    }
}
