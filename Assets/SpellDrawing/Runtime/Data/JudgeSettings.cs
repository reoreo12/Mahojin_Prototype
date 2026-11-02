// 유사도 계산 설정: 리샘플링 점 개수, 허용 거리(그리기 영역 한 변 = 1), 거리 → 점수 변환 방식.
using UnityEngine;

namespace VoxelMagic.SpellDrawing
{
    [CreateAssetMenu(fileName = "JudgeSettings", menuName = "VoxelMagic/SpellDrawing/Judge Settings")]
    public sealed class JudgeSettings : ScriptableObject
    {
        [SerializeField, Min(2)] private int resampleCount = 64;
        [SerializeField, Range(0.005f, 0.5f)] private float tolerance = 0.08f;
        [SerializeField] private DistanceScoring distanceScoring = DistanceScoring.MeanThenScale;

        public int ResampleCount => resampleCount;
        public float Tolerance => tolerance;
        public DistanceScoring DistanceScoring => distanceScoring;

        public void SetTolerance(float value) => tolerance = Mathf.Clamp(value, 0.005f, 0.5f);
    }
}
