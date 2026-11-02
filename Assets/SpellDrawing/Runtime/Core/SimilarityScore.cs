// 한 번 비교한 결과: 최종 유사도, 커버리지(빠뜨림), 정밀도(삐져나옴)와 양방향 평균 거리.
namespace VoxelMagic.SpellDrawing
{
    public readonly struct SimilarityScore
    {
        public static readonly SimilarityScore Zero = new SimilarityScore(0f, 0f, 0f, float.PositiveInfinity, float.PositiveInfinity);

        public float Similarity { get; }
        public float Coverage { get; }
        public float Precision { get; }
        public float CoverageDistance { get; }
        public float PrecisionDistance { get; }

        public SimilarityScore(float similarity, float coverage, float precision, float coverageDistance, float precisionDistance)
        {
            Similarity = similarity;
            Coverage = coverage;
            Precision = precision;
            CoverageDistance = coverageDistance;
            PrecisionDistance = precisionDistance;
        }
    }
}
