// 시도 하나의 판정 결과: 정규화 점수, 비정규화(가이드 좌표 그대로) 점수, 계산 시간, 비교에 쓴 리샘플링 점.
namespace VoxelMagic.SpellDrawing
{
    public sealed class JudgeResult
    {
        public SimilarityScore Normalized { get; }
        public SimilarityScore Raw { get; }
        public double JudgeMs { get; }
        public bool IsEmpty { get; }
        public int ResampleCount { get; }
        public float Tolerance { get; }
        public StrokeSet ResampledDrawn { get; }
        public StrokeSet ResampledTemplate { get; }

        // 주 점수. 가이드를 따라 그리는 1단계는 비정규화 점수가 사람 감각과 맞아서 이것을 쓴다 (TESTING.md 기준 비교표).
        public float Similarity => Raw.Similarity;
        public float Coverage => Raw.Coverage;
        public float Precision => Raw.Precision;

        public JudgeResult(SimilarityScore normalized, SimilarityScore raw, double judgeMs, bool isEmpty,
            int resampleCount, float tolerance, StrokeSet resampledDrawn, StrokeSet resampledTemplate)
        {
            Normalized = normalized;
            Raw = raw;
            JudgeMs = judgeMs;
            IsEmpty = isEmpty;
            ResampleCount = resampleCount;
            Tolerance = tolerance;
            ResampledDrawn = resampledDrawn;
            ResampledTemplate = resampledTemplate;
        }
    }
}
