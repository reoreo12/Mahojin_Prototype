// 그린 획과 원본 도안을 양방향 챔퍼 거리(점→선분)로 비교해 커버리지·정밀도를 내고, 조화 평균으로 0~1 유사도를 계산한다.
using UnityEngine;

namespace VoxelMagic.SpellDrawing
{
    public enum DistanceScoring
    {
        MeanThenScale,   // 평균 거리를 허용 거리로 나눔 (지침 방식)
        ScalePerPoint,   // 점마다 허용 거리로 나눠 0~1로 자른 뒤 평균
    }

    public sealed class ChamferSimilarityMetric : ISimilarityMetric
    {
        private readonly DistanceScoring _scoring;

        public ChamferSimilarityMetric(DistanceScoring scoring = DistanceScoring.MeanThenScale)
        {
            _scoring = scoring;
        }

        public SimilarityScore Evaluate(StrokeSet drawn, StrokeSet template, float tolerance)
        {
            if (drawn == null || template == null || drawn.IsEmpty || template.IsEmpty || tolerance <= 0f)
                return SimilarityScore.Zero;

            Measure(template, drawn, tolerance, out float coverageDistance, out float coverage);
            Measure(drawn, template, tolerance, out float precisionDistance, out float precision);
            float similarity = coverage + precision > 0f ? 2f * coverage * precision / (coverage + precision) : 0f;
            return new SimilarityScore(similarity, coverage, precision, coverageDistance, precisionDistance);
        }

        // from의 각 점에서 to 모양까지 거리를 재서 평균 거리와 0~1 점수를 낸다.
        private void Measure(StrokeSet from, StrokeSet to, float tolerance, out float meanDistance, out float score)
        {
            float distanceSum = 0f;
            float perPointScoreSum = 0f;
            for (int s = 0; s < from.StrokeCount; s++)
            {
                var stroke = from.GetStroke(s);
                for (int i = 0; i < stroke.Count; i++)
                {
                    float d = ShapeDistance.ToShape(stroke[i], to);
                    distanceSum += d;
                    perPointScoreSum += Mathf.Clamp01(1f - d / tolerance);
                }
            }
            meanDistance = distanceSum / from.PointCount;
            score = _scoring == DistanceScoring.MeanThenScale
                ? Mathf.Clamp01(1f - meanDistance / tolerance)
                : perPointScoreSum / from.PointCount;
        }
    }
}
