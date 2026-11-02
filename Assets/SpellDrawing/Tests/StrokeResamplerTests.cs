// StrokeResampler·ShapeNormalizer 단위 테스트: 항상 N개, 등간격, 점만 찍은 입력, 정규화 결과.
using NUnit.Framework;
using UnityEngine;

namespace VoxelMagic.SpellDrawing.Tests
{
    public sealed class StrokeResamplerTests
    {
        [TestCase(2)]
        [TestCase(16)]
        [TestCase(64)]
        [TestCase(257)]
        public void AlwaysProducesExactCount(int count)
        {
            Assert.AreEqual(count, StrokeResampler.Resample(TestShapes.Template, count).PointCount);
            Assert.AreEqual(count, StrokeResampler.Resample(TestShapes.Split(TestShapes.Template, 5), count).PointCount);
            Assert.AreEqual(count, StrokeResampler.Resample(TestShapes.Scribble(3), count).PointCount);
            Assert.AreEqual(count, StrokeResampler.Resample(TestShapes.HorizontalLine(), count).PointCount);
        }

        [Test]
        public void PointsAreEvenlySpacedAlongLine()
        {
            var line = new StrokeSet(new[] { new[] { Vector2.zero, new Vector2(0.1f, 0f), new Vector2(1f, 0f) } });
            var resampled = StrokeResampler.Resample(line, 11).GetStroke(0);
            for (int i = 0; i < resampled.Count; i++)
                Assert.AreEqual(i * 0.1f, resampled[i].x, 1e-4f);
        }

        [Test]
        public void DotsOnlyInputStillProducesExactCount()
        {
            var dots = new StrokeSet(new[] { new[] { new Vector2(0.2f, 0.2f) }, new[] { new Vector2(0.8f, 0.8f) } });
            Assert.AreEqual(64, StrokeResampler.Resample(dots, 64).PointCount);
        }

        [Test]
        public void EmptyInputGivesEmpty()
        {
            Assert.IsTrue(StrokeResampler.Resample(StrokeSet.Empty, 64).IsEmpty);
        }

        [Test]
        public void NormalizeRemovesPositionAndScale()
        {
            var a = ShapeNormalizer.Normalize(StrokeResampler.Resample(TestShapes.Template, 64));
            var moved = TestShapes.Scaled(TestShapes.Translated(TestShapes.Template, new Vector2(0.1f, -0.05f)), 0.5f, Vector2.zero);
            var b = ShapeNormalizer.Normalize(StrokeResampler.Resample(moved, 64));
            Assert.AreEqual(1f, ShapeNormalizer.GetSize(a), 1e-4f);
            Assert.Less(ShapeNormalizer.GetCentroid(a).magnitude, 1e-4f);
            using (var ea = a.AllPoints().GetEnumerator())
            using (var eb = b.AllPoints().GetEnumerator())
            {
                while (ea.MoveNext() && eb.MoveNext())
                    Assert.Less(Vector2.Distance(ea.Current, eb.Current), 1e-3f);
            }
        }
    }
}
