// 리샘플링 → 비정규화 비교 → 정규화 비교를 차례로 실행해 판정 결과를 만들고 계산 시간을 잰다.
using System.Diagnostics;
using UnityEngine;

namespace VoxelMagic.SpellDrawing
{
    public sealed class SpellJudge
    {
        private const float Epsilon = 1e-6f;
        private readonly ISimilarityMetric _metric;

        public SpellJudge(ISimilarityMetric metric)
        {
            _metric = metric;
        }

        // tolerance는 그리기 공간 단위. 정규화 비교에서는 원본 크기로 나눠 같은 뜻이 되게 한다.
        public JudgeResult Judge(StrokeSet drawn, StrokeSet template, int resampleCount, float tolerance)
        {
            var stopwatch = Stopwatch.StartNew();
            resampleCount = Mathf.Max(2, resampleCount);
            var resampledTemplate = StrokeResampler.Resample(template, resampleCount);

            if (drawn == null || drawn.IsEmpty)
            {
                stopwatch.Stop();
                return new JudgeResult(SimilarityScore.Zero, SimilarityScore.Zero, stopwatch.Elapsed.TotalMilliseconds,
                    true, resampleCount, tolerance, StrokeSet.Empty, resampledTemplate);
            }

            var resampledDrawn = StrokeResampler.Resample(drawn, resampleCount);
            var raw = _metric.Evaluate(resampledDrawn, resampledTemplate, tolerance);

            float templateSize = ShapeNormalizer.GetSize(resampledTemplate);
            float normalizedTolerance = tolerance / Mathf.Max(templateSize, Epsilon);
            var normalized = _metric.Evaluate(ShapeNormalizer.Normalize(resampledDrawn),
                ShapeNormalizer.Normalize(resampledTemplate), normalizedTolerance);

            stopwatch.Stop();
            return new JudgeResult(normalized, raw, stopwatch.Elapsed.TotalMilliseconds, false,
                resampleCount, tolerance, resampledDrawn, resampledTemplate);
        }
    }
}
