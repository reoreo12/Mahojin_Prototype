// StrokeRecorder 단위 테스트: 간격 기록, 영역 밖 시작 무시, 경계 잘라내기.
using NUnit.Framework;
using UnityEngine;

namespace VoxelMagic.SpellDrawing.Tests
{
    public sealed class StrokeRecorderTests
    {
        [Test]
        public void SkipsPointsCloserThanSpacing()
        {
            var recorder = new StrokeRecorder(0.1f);
            recorder.BeginStroke(new Vector2(0.1f, 0.1f));
            recorder.AddPoint(new Vector2(0.15f, 0.1f));
            recorder.AddPoint(new Vector2(0.25f, 0.1f));
            recorder.EndStroke();
            Assert.AreEqual(2, recorder.PointCount);
        }

        [Test]
        public void EndPointIsKeptRegardlessOfSpacing()
        {
            var recorder = new StrokeRecorder(0.1f);
            recorder.BeginStroke(new Vector2(0.1f, 0.1f));
            recorder.EndStroke(new Vector2(0.12f, 0.1f));
            Assert.AreEqual(2, recorder.PointCount);
        }

        [Test]
        public void StrokeStartedOutsideAreaIsIgnored()
        {
            var recorder = new StrokeRecorder(0f);
            Assert.IsFalse(recorder.BeginStroke(new Vector2(-0.1f, 0.5f)));
            recorder.AddPoint(new Vector2(0.5f, 0.5f));
            Assert.AreEqual(0, recorder.StrokeCount);
            Assert.AreEqual(0, recorder.PointCount);
        }

        [Test]
        public void PointsOutsideAreaAreClampedToEdge()
        {
            var recorder = new StrokeRecorder(0f);
            recorder.BeginStroke(new Vector2(0.5f, 0.5f));
            recorder.AddPoint(new Vector2(1.4f, -0.2f));
            var stroke = recorder.ToStrokeSet().GetStroke(0);
            Assert.AreEqual(new Vector2(1f, 0f), stroke[1]);
        }

        [Test]
        public void MultipleStrokesAreKeptSeparately()
        {
            var recorder = new StrokeRecorder(0f);
            recorder.BeginStroke(new Vector2(0.1f, 0.1f));
            recorder.AddPoint(new Vector2(0.2f, 0.1f));
            recorder.EndStroke();
            recorder.BeginStroke(new Vector2(0.5f, 0.5f));
            recorder.AddPoint(new Vector2(0.6f, 0.5f));
            recorder.EndStroke();
            Assert.AreEqual(2, recorder.ToStrokeSet().StrokeCount);
        }
    }
}
