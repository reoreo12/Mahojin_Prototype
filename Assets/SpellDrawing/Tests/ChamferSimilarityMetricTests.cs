// 유사도 계산이 사람 감각과 맞는지 확인하는 단위 테스트 (지침 4번의 경우들 + 계산 시간).
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;

namespace VoxelMagic.SpellDrawing.Tests
{
    [TestFixture(DistanceScoring.MeanThenScale)]
    [TestFixture(DistanceScoring.ScalePerPoint)]
    public sealed class ChamferSimilarityMetricTests
    {
        private readonly DistanceScoring _scoring;

        public ChamferSimilarityMetricTests(DistanceScoring scoring)
        {
            _scoring = scoring;
        }

        private JudgeResult Judge(StrokeSet drawn) => SimilarityCases.Judge(drawn, _scoring);

        [Test]
        public void IdenticalShapeScoresNearOne()
        {
            var result = Judge(TestShapes.Template);
            Assert.Greater(result.Normalized.Similarity, 0.98f);
            Assert.Greater(result.Raw.Similarity, 0.98f);
        }

        [Test]
        public void MovedOrScaledShapeKeepsNormalizedScore()
        {
            var moved = TestShapes.Translated(TestShapes.Template, new Vector2(0.05f, -0.03f));
            var scaled = TestShapes.Scaled(TestShapes.Template, 0.7f, new Vector2(0.5f, 0.5f));
            Assert.Greater(Judge(moved).Normalized.Similarity, 0.98f);
            Assert.Greater(Judge(scaled).Normalized.Similarity, 0.98f);
            Assert.Less(Judge(moved).Raw.Similarity, Judge(moved).Normalized.Similarity);
        }

        [Test]
        public void ReversedOrSplitStrokesScoreAlmostTheSame()
        {
            Assert.Greater(Judge(TestShapes.Reversed(TestShapes.Template)).Similarity, 0.97f);
            Assert.Greater(Judge(TestShapes.Split(TestShapes.Template, 3)).Similarity, 0.97f);
        }

        [Test]
        public void SmallNoiseScoresHighButBelowOne()
        {
            float perfect = Judge(TestShapes.Template).Similarity;
            float noisy = Judge(TestShapes.Jittered(TestShapes.Template, 0.01f, 7)).Similarity;
            Assert.Greater(noisy, 0.6f);
            Assert.Less(noisy, perfect);
        }

        [Test]
        public void HalfDrawnHasClearlyLowCoverage()
        {
            var result = Judge(TestShapes.Partial(TestShapes.Template, 0.5f));
            Assert.Less(result.Normalized.Coverage, 0.6f);
            Assert.Less(result.Raw.Coverage, 0.6f);
            Assert.Greater(result.Raw.Precision, 0.9f);
        }

        // 사람 감각과 맞는지 보는 아래 두 테스트는 채택한 방식(평균거리 + 비정규화 주 점수)에만 적용한다.
        // 점별 방식은 낙서에 0.4 이상을 줘서 채택하지 않았다 (TESTING.md).
        [Test]
        public void UnrelatedShapesScoreClearlyLow()
        {
            Assume.That(_scoring == DistanceScoring.MeanThenScale);
            Assert.Less(Judge(TestShapes.HorizontalLine()).Similarity, 0.35f);
            Assert.Less(Judge(TestShapes.Scribble(11)).Similarity, 0.4f);
        }

        [Test]
        public void EmptyDrawingScoresZero()
        {
            var result = Judge(StrokeSet.Empty);
            Assert.IsTrue(result.IsEmpty);
            Assert.AreEqual(0f, result.Similarity);
        }

        [Test]
        public void OrderMatchesHumanExpectation()
        {
            Assume.That(_scoring == DistanceScoring.MeanThenScale);
            float perfect = Judge(TestShapes.Template).Similarity;
            float noisy = Judge(TestShapes.Jittered(TestShapes.Template, 0.01f, 7)).Similarity;
            float half = Judge(TestShapes.Partial(TestShapes.Template, 0.5f)).Similarity;
            float scribble = Judge(TestShapes.Scribble(11)).Similarity;
            float line = Judge(TestShapes.HorizontalLine()).Similarity;
            Assert.Greater(perfect, noisy);
            Assert.Greater(noisy, half);
            Assert.Greater(half, scribble);
            Assert.GreaterOrEqual(scribble, line);
        }

        [Test]
        public void JudgeTakesOnlyAFewMilliseconds()
        {
            var drawn = TestShapes.Jittered(TestShapes.Template, 0.02f, 3);
            for (int i = 0; i < 5; i++) Judge(drawn);

            const int runs = 100;
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < runs; i++) Judge(drawn);
            stopwatch.Stop();
            double averageMs = stopwatch.Elapsed.TotalMilliseconds / runs;
            UnityEngine.Debug.Log($"[SpellDrawing] {_scoring} 판정 평균 {averageMs:F3} ms (점 {SimilarityCases.ResampleCount}개)");
            Assert.Less(averageMs, 5.0);
        }
    }
}
