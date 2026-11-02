// 도안 목록. 1단계는 한 개만 쓰지만, 3단계(여러 도안·인식)에서 그대로 늘려 쓴다.
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.SpellDrawing
{
    [CreateAssetMenu(fileName = "SpellTemplateLibrary", menuName = "VoxelMagic/SpellDrawing/Spell Template Library")]
    public sealed class SpellTemplateLibrary : ScriptableObject
    {
        [SerializeField] private List<SpellTemplate> templates = new List<SpellTemplate>();

        public IReadOnlyList<SpellTemplate> Templates => templates;
        public SpellTemplate Default => templates.Count > 0 ? templates[0] : null;

        public SpellTemplate FindById(string id) => templates.Find(t => t != null && t.Id == id);

        public void Add(SpellTemplate template)
        {
            if (template != null && !templates.Contains(template)) templates.Add(template);
        }
    }
}
