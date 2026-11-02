// 유사도 테스트와 기준 비교표가 함께 쓰는 비교 경우 목록 (사람이 기대하는 순서대로).
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.SpellDrawing.Tests
{
    public static class SimilarityCases
    {
        public const int ResampleCount = 64;
        public const float Tolerance = 0.08f;

        public static IEnumerable<(string name, StrokeSet drawn)> All()
        {
            var t = TestShapes.Template;
            var center = new Vector2(0.5f, 0.5f);
            yield return ("완벽 (원본 그대로)", t);
            yield return ("거꾸로 그림", TestShapes.Reversed(t));
            yield return ("획 나눠 그림 (획당 3조각)", TestShapes.Split(t, 3));
            yield return ("옮김 (+0.05, -0.03)", TestShapes.Translated(t, new Vector2(0.05f, -0.03f)));
            yield return ("작게 그림 (0.7배)", TestShapes.Scaled(t, 0.7f, center));
            yield return ("약간 흔들림 (±0.01)", TestShapes.Jittered(t, 0.01f, 7));
            yield return ("많이 흔들림 (±0.03)", TestShapes.Jittered(t, 0.03f, 7));
            yield return ("원만 그림", new StrokeSet(new[] { t.GetStroke(0) }));
            yield return ("절반만 그림", TestShapes.Partial(t, 0.5f));
            yield return ("직선 하나", TestShapes.HorizontalLine());
            yield return ("낙서", TestShapes.Scribble(11));
        }

        public static JudgeResult Judge(StrokeSet drawn, DistanceScoring scoring) =>
            new SpellJudge(new ChamferSimilarityMetric(scoring)).Judge(drawn, TestShapes.Template, ResampleCount, Tolerance);
    }
}
