// OS에 설치된 맑은 고딕으로 동적 폰트를 만들어 Text에 적용한다. 폰트 파일을 프로젝트에 넣지 않기 위한 방법.
using UnityEngine;
using UnityEngine.UI;

namespace VoxelMagic.SpellDrawing.DebugUI
{
    public sealed class KoreanFontApplier : MonoBehaviour
    {
        [SerializeField] private string[] osFontNames = { "Malgun Gothic", "맑은 고딕" };
        [SerializeField, Min(1)] private int baseFontSize = 16;

        private Font _font;

        public Font Font => EnsureFont();

        private void Awake() => Apply(gameObject);

        public void Apply(GameObject root)
        {
            var font = EnsureFont();
            if (font == null) return;
            foreach (var text in root.GetComponentsInChildren<Text>(true)) text.font = font;
        }

        private Font EnsureFont()
        {
            if (_font == null)
            {
                _font = Font.CreateDynamicFontFromOSFont(osFontNames, baseFontSize);
                if (_font == null) UnityEngine.Debug.LogWarning("[SpellDrawing] 한글 OS 폰트를 찾지 못해 기본 폰트를 씁니다.");
            }
            return _font;
        }
    }
}
