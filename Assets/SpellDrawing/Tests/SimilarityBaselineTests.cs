// 기준 비교표를 만들어 콘솔에 마크다운 표로 출력한다. TESTING.md에 붙여 사람이 순서를 확인하는 용도.
using System.Text;
using NUnit.Framework;

namespace VoxelMagic.SpellDrawing.Tests
{
    public sealed class SimilarityBaselineTests
    {
        [Test]
        public void PrintBaselineTable()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[SpellDrawing] 기준 비교표 (N={SimilarityCases.ResampleCount}, 허용 거리={SimilarityCases.Tolerance})");
            sb.AppendLine("| 경우 | 평균거리: 유사도 | 커버리지 | 정밀도 | 비정규화 | 점별: 유사도 | 커버리지 | 정밀도 | 비정규화 |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|---|");
            foreach (var (name, drawn) in SimilarityCases.All())
            {
                var mean = SimilarityCases.Judge(drawn, DistanceScoring.MeanThenScale);
                var perPoint = SimilarityCases.Judge(drawn, DistanceScoring.ScalePerPoint);
                sb.AppendLine($"| {name} | {Row(mean)} | {Row(perPoint)} |");
            }
            UnityEngine.Debug.Log(sb.ToString());
            Assert.Pass();
        }

        private static string Row(JudgeResult r) =>
            $"**{r.Normalized.Similarity:F2}** | {r.Normalized.Coverage:F2} | {r.Normalized.Precision:F2} | {r.Raw.Similarity:F2}";
    }
}
