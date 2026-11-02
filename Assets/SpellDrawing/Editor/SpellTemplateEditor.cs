// SpellTemplate 인스펙터 아래에 도안 점 목록을 그림으로 미리 보여 준다.
using UnityEditor;
using UnityEngine;

namespace VoxelMagic.SpellDrawing.EditorTools
{
    [CustomEditor(typeof(SpellTemplate))]
    public sealed class SpellTemplateEditor : UnityEditor.Editor
    {
        private const float PreviewSize = 200f;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var template = (SpellTemplate)target;
            var strokes = template.Strokes;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"미리보기 — 획 {strokes.StrokeCount}개, 점 {strokes.PointCount}개", EditorStyles.boldLabel);

            Rect rect = GUILayoutUtility.GetRect(PreviewSize, PreviewSize, GUILayout.ExpandWidth(false));
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.12f));
            if (Event.current.type != EventType.Repaint) return;

            Handles.color = template.GuideColor;
            for (int s = 0; s < strokes.StrokeCount; s++)
            {
                var stroke = strokes.GetStroke(s);
                var points = new Vector3[stroke.Count];
                for (int i = 0; i < points.Length; i++)
                    points[i] = new Vector3(rect.x + stroke[i].x * rect.width, rect.yMax - stroke[i].y * rect.height);
                Handles.DrawAAPolyLine(3f, points);
            }
        }
    }
}
