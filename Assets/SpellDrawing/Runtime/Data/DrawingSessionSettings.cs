// 그리기 시도 설정: 제한 시간, 카운트다운, 가이드 투명도, 점 기록 간격. 실행 중 슬라이더는 복제본만 바꾼다.
using UnityEngine;

namespace VoxelMagic.SpellDrawing
{
    [CreateAssetMenu(fileName = "DrawingSessionSettings", menuName = "VoxelMagic/SpellDrawing/Drawing Session Settings")]
    public sealed class DrawingSessionSettings : ScriptableObject
    {
        [SerializeField, Min(0.5f)] private float timeLimitSec = 8f;
        [SerializeField] private bool useCountdown = true;
        [SerializeField, Min(0)] private int countdownSec = 3;
        [SerializeField, Range(0f, 1f)] private float guideAlpha = 0.35f;
        [SerializeField, Min(0f), Tooltip("획을 기록할 최소 간격 (화면 픽셀)")] private float minPointSpacingPx = 4f;

        public float TimeLimitSec => timeLimitSec;
        public bool UseCountdown => useCountdown;
        public int CountdownSec => countdownSec;
        public float GuideAlpha => guideAlpha;
        public float MinPointSpacingPx => minPointSpacingPx;

        public void SetTimeLimitSec(float value) => timeLimitSec = Mathf.Max(0.5f, value);
        public void SetGuideAlpha(float value) => guideAlpha = Mathf.Clamp01(value);
    }
}
