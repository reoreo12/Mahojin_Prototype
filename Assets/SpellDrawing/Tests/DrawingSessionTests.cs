// DrawingSession 상태 흐름 테스트: 카운트다운 → 그리기 → 시간 종료 판정, 취소, 그리기 중에만 입력.
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace VoxelMagic.SpellDrawing.Tests
{
    public sealed class DrawingSessionTests
    {
        private readonly List<Object> _created = new List<Object>();

        private sealed class CountingHandler : IAttemptResultHandler
        {
            public int Count;
            public void Handle(AttemptRecord record) => Count++;
        }

        private DrawingSession CreateSession()
        {
            var sessionSettings = ScriptableObject.CreateInstance<DrawingSessionSettings>();
            var judgeSettings = ScriptableObject.CreateInstance<JudgeSettings>();
            var template = ScriptableObject.CreateInstance<SpellTemplate>();
            template.SetData("test", "test", TestShapes.Template, Color.white);
            _created.AddRange(new Object[] { sessionSettings, judgeSettings, template });
            return new DrawingSession(sessionSettings, judgeSettings, new SpellJudge(new ChamferSimilarityMetric()), template);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var o in _created) Object.DestroyImmediate(o);
            _created.Clear();
        }

        [Test]
        public void CountdownThenDrawingThenResult()
        {
            var session = CreateSession();
            var handler = new CountingHandler();
            session.AddHandler(handler);
            AttemptRecord completed = null;
            session.AttemptCompleted += r => completed = r;

            Assert.IsTrue(session.Start());
            Assert.AreEqual(DrawingState.Countdown, session.State);
            Assert.IsFalse(session.BeginStroke(new Vector2(0.5f, 0.5f)), "카운트다운 중에는 그릴 수 없다");

            session.Tick(3.1f);
            Assert.AreEqual(DrawingState.Drawing, session.State);
            Assert.IsTrue(session.BeginStroke(new Vector2(0.5f, 0.9f)));
            session.AddPoint(new Vector2(0.6f, 0.85f));
            session.EndStroke();

            session.Tick(8f);
            Assert.AreEqual(DrawingState.Result, session.State);
            Assert.IsNotNull(completed);
            Assert.AreEqual(1, completed.Id);
            Assert.AreEqual(1, completed.StrokeCount);
            Assert.AreEqual(1, handler.Count);
        }

        [Test]
        public void TimeoutWithoutDrawingIsRecordedAsEmpty()
        {
            var session = CreateSession();
            AttemptRecord completed = null;
            session.AttemptCompleted += r => completed = r;
            session.Start();
            session.Tick(20f);
            Assert.IsNotNull(completed);
            Assert.IsTrue(completed.IsEmpty);
            Assert.AreEqual(0f, completed.Similarity);
        }

        [Test]
        public void CancelReturnsToIdleWithoutRecord()
        {
            var session = CreateSession();
            int completedCount = 0;
            session.AttemptCompleted += _ => completedCount++;
            session.Start();
            session.Tick(4f);
            session.Cancel();
            Assert.AreEqual(DrawingState.Idle, session.State);
            session.Tick(20f);
            Assert.AreEqual(0, completedCount);
        }
    }
}
