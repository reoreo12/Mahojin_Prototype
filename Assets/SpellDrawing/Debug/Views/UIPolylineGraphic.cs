// uGUI 안에서 획 목록(그리기 공간 0~1)을 굵은 선 메시로 그린다. 가이드와 실시간 그린 선에 쓴다.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VoxelMagic.SpellDrawing.DebugUI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UIPolylineGraphic : MaskableGraphic
    {
        private const int MaxVertices = 60000;

        [SerializeField, Min(0.5f)] private float thickness = 6f;
        [SerializeField, Tooltip("선분 끝을 굵기 절반만큼 늘려 꺾이는 곳 틈을 메운다. 반투명 선은 겹친 곳이 진해지므로 끈다.")]
        private bool extendEnds = true;

        private IReadOnlyList<IReadOnlyList<Vector2>> _strokes;

        public void SetStrokes(IReadOnlyList<IReadOnlyList<Vector2>> strokes)
        {
            _strokes = strokes;
            SetVerticesDirty();
        }

        public void SetStrokes(StrokeSet strokeSet)
        {
            var list = new List<IReadOnlyList<Vector2>>(strokeSet.StrokeCount);
            for (int i = 0; i < strokeSet.StrokeCount; i++) list.Add(strokeSet.GetStroke(i));
            SetStrokes(list);
        }

        // 획 목록이 제자리에서 바뀌었을 때 다시 그린다.
        public void Refresh() => SetVerticesDirty();

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (_strokes == null) return;

            Rect rect = rectTransform.rect;
            float half = thickness * 0.5f;
            foreach (var stroke in _strokes)
            {
                if (stroke == null || stroke.Count == 0) continue;
                if (stroke.Count == 1)
                {
                    Vector2 p = ToLocal(stroke[0], rect);
                    AddQuad(vh, p + new Vector2(-half, -half), p + new Vector2(half, -half),
                        p + new Vector2(half, half), p + new Vector2(-half, half));
                    continue;
                }
                for (int i = 1; i < stroke.Count; i++)
                {
                    if (vh.currentVertCount > MaxVertices) return;
                    Vector2 a = ToLocal(stroke[i - 1], rect);
                    Vector2 b = ToLocal(stroke[i], rect);
                    Vector2 dir = b - a;
                    if (dir.sqrMagnitude < 1e-6f) continue;
                    dir.Normalize();
                    Vector2 normal = new Vector2(-dir.y, dir.x) * half;
                    Vector2 extend = extendEnds ? dir * half : Vector2.zero;
                    AddQuad(vh, a - extend + normal, a - extend - normal, b + extend - normal, b + extend + normal);
                }
            }
        }

        private static Vector2 ToLocal(Vector2 p, Rect rect) =>
            new Vector2(rect.xMin + p.x * rect.width, rect.yMin + p.y * rect.height);

        private void AddQuad(VertexHelper vh, Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            int start = vh.currentVertCount;
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;
            vertex.position = v0; vh.AddVert(vertex);
            vertex.position = v1; vh.AddVert(vertex);
            vertex.position = v2; vh.AddVert(vertex);
            vertex.position = v3; vh.AddVert(vertex);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start + 2, start + 3, start);
        }
    }
}
