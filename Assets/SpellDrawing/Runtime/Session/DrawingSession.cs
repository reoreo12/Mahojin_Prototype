// 그리기 시도의 상태 기계(대기→카운트다운→그리기→판정→결과). 시간은 Tick(dt)로 흘려 테스트에서도 그대로 검증할 수 있다.
// 그리기 중에만 입력을 받고, 시간이 끝나면 판정 → 결과 처리기 실행 → AttemptCompleted 이벤트 순서로 진행한다.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.SpellDrawing
{
    public sealed class DrawingSession
    {
        private readonly DrawingSessionSettings _sessionSettings;
        private readonly JudgeSettings _judgeSettings;
        private readonly SpellJudge _judge;
        private readonly List<IAttemptResultHandler> _handlers = new List<IAttemptResultHandler>();
        private readonly StrokeRecorder _recorder = new StrokeRecorder(0f);
        private readonly Func<DateTime> _clock;

        private SpellTemplate _template;
        private float _areaPixelSize = 640f;
        private int _nextAttemptId = 1;
        private float _timeLimit;
        private float _countdownRemaining;
        private float _drawingElapsed;
        private float _firstInkTime = -1f;
        private float _lastInkTime = -1f;

        public event Action<DrawingState> StateChanged;
        public event Action TimeChanged;
        public event Action StrokesChanged;
        public event Action<AttemptRecord> AttemptCompleted;
        public event Action Cancelled;

        public DrawingState State { get; private set; } = DrawingState.Idle;
        public SpellTemplate Template => _template;
        public float TimeLimit => _timeLimit;
        public float TimeRemaining => Mathf.Max(0f, _timeLimit - _drawingElapsed);
        public float CountdownRemaining => _countdownRemaining;
        public IReadOnlyList<IReadOnlyList<Vector2>> CurrentStrokes => _recorder.Strokes;
        public AttemptRecord LastAttempt { get; private set; }
        public bool CanStart => (State == DrawingState.Idle || State == DrawingState.Result) && _template != null;
        public bool IsAcceptingInput => State == DrawingState.Drawing;

        public DrawingSession(DrawingSessionSettings sessionSettings, JudgeSettings judgeSettings, SpellJudge judge,
            SpellTemplate template, Func<DateTime> clock = null)
        {
            _sessionSettings = sessionSettings;
            _judgeSettings = judgeSettings;
            _judge = judge;
            _template = template;
            _clock = clock ?? (() => DateTime.Now);
            _recorder.Changed += () => StrokesChanged?.Invoke();
        }

        public void AddHandler(IAttemptResultHandler handler)
        {
            if (handler != null && !_handlers.Contains(handler)) _handlers.Add(handler);
        }

        public void SetTemplate(SpellTemplate template)
        {
            if (State == DrawingState.Idle || State == DrawingState.Result) _template = template;
        }

        // 그리기 영역 한 변의 화면 픽셀 크기. 점 기록 간격(px)을 그리기 공간 단위로 바꾸는 데 쓴다.
        public void SetAreaPixelSize(float pixels) => _areaPixelSize = Mathf.Max(1f, pixels);

        public bool Start()
        {
            if (!CanStart) return false;
            _recorder.Clear();
            _recorder.SetMinSpacing(_sessionSettings.MinPointSpacingPx / _areaPixelSize);
            _timeLimit = _sessionSettings.TimeLimitSec;
            _drawingElapsed = 0f;
            _firstInkTime = -1f;
            _lastInkTime = -1f;

            if (_sessionSettings.UseCountdown && _sessionSettings.CountdownSec > 0)
            {
                _countdownRemaining = _sessionSettings.CountdownSec;
                SetState(DrawingState.Countdown);
            }
            else
            {
                _countdownRemaining = 0f;
                SetState(DrawingState.Drawing);
            }
            TimeChanged?.Invoke();
            return true;
        }

        // 카운트다운·그리기 중에만 취소된다. 기록하지 않는다.
        public void Cancel()
        {
            if (State != DrawingState.Countdown && State != DrawingState.Drawing) return;
            _recorder.EndStroke();
            _recorder.Clear();
            SetState(DrawingState.Idle);
            Cancelled?.Invoke();
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f) return;
            if (State == DrawingState.Countdown)
            {
                _countdownRemaining -= deltaTime;
                if (_countdownRemaining > 0f)
                {
                    TimeChanged?.Invoke();
                    return;
                }
                float overflow = -_countdownRemaining;
                _countdownRemaining = 0f;
                SetState(DrawingState.Drawing);
                TimeChanged?.Invoke();
                if (overflow > 0f) Tick(overflow);
                return;
            }
            if (State != DrawingState.Drawing) return;

            _drawingElapsed = Mathf.Min(_timeLimit, _drawingElapsed + deltaTime);
            TimeChanged?.Invoke();
            if (_drawingElapsed >= _timeLimit) Finish();
        }

        public bool BeginStroke(Vector2 point)
        {
            if (!IsAcceptingInput || !_recorder.BeginStroke(point)) return false;
            MarkInk();
            return true;
        }

        public void AddPoint(Vector2 point)
        {
            if (!IsAcceptingInput || !_recorder.IsStroking) return;
            _recorder.AddPoint(point);
            MarkInk();
        }

        public void EndStroke(Vector2? lastPoint = null)
        {
            if (!IsAcceptingInput || !_recorder.IsStroking) return;
            _recorder.EndStroke(lastPoint);
            MarkInk();
        }

        private void MarkInk()
        {
            if (_firstInkTime < 0f) _firstInkTime = _drawingElapsed;
            _lastInkTime = _drawingElapsed;
        }

        private void Finish()
        {
            _recorder.EndStroke();
            SetState(DrawingState.Judging);

            var drawn = _recorder.ToStrokeSet();
            var templateStrokes = _template.Strokes;
            var result = _judge.Judge(drawn, templateStrokes, _judgeSettings.ResampleCount, _judgeSettings.Tolerance);
            float drawSec = _firstInkTime < 0f ? 0f : _lastInkTime - _firstInkTime;
            var record = new AttemptRecord(_nextAttemptId++, _clock(), _template.Id, _timeLimit, drawSec,
                drawn, templateStrokes, result);
            LastAttempt = record;

            foreach (var handler in _handlers)
            {
                try { handler.Handle(record); }
                catch (Exception e) { Debug.LogException(e); }
            }

            AttemptCompleted?.Invoke(record);
            SetState(DrawingState.Result);
        }

        private void SetState(DrawingState state)
        {
            if (State == state) return;
            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
