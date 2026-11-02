// 원본 마법진 도안 하나: ID, 표시 이름, 획별 점 목록(그리기 공간 0~1), 가이드 색.
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoxelMagic.SpellDrawing
{
    [CreateAssetMenu(fileName = "SpellTemplate", menuName = "VoxelMagic/SpellDrawing/Spell Template")]
    public sealed class SpellTemplate : ScriptableObject
    {
        [Serializable]
        private struct StrokeData
        {
            [SerializeField] private Vector2[] points;
            public Vector2[] Points => points ?? Array.Empty<Vector2>();
            public StrokeData(Vector2[] points) { this.points = points; }
        }

        [SerializeField] private string id = "capture_placeholder";
        [SerializeField] private string displayName = "포획 마법진 (임시)";
        [SerializeField] private Color guideColor = new Color(0.45f, 0.8f, 1f, 1f);
        [SerializeField] private List<StrokeData> strokes = new List<StrokeData>();

        [NonSerialized] private StrokeSet _cache;

        public string Id => id;
        public string DisplayName => displayName;
        public Color GuideColor => guideColor;
        public StrokeSet Strokes => _cache ??= new StrokeSet(strokes.Select(s => s.Points));

        public void SetData(string newId, string newDisplayName, StrokeSet strokeSet, Color newGuideColor)
        {
            id = newId;
            displayName = newDisplayName;
            guideColor = newGuideColor;
            strokes = new List<StrokeData>();
            for (int i = 0; i < strokeSet.StrokeCount; i++)
                strokes.Add(new StrokeData(strokeSet.GetStroke(i).ToArray()));
            _cache = null;
        }

        private void OnValidate() => _cache = null;
    }
}
