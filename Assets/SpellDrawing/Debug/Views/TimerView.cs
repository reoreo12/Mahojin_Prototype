// 카운트다운 숫자, 남은 시간 숫자와 줄어드는 바, 현재 상태 안내를 보여 준다.
using UnityEngine;
using UnityEngine.UI;

namespace VoxelMagic.SpellDrawing.DebugUI
{
    public sealed class TimerView : MonoBehaviour
    {
        [SerializeField] private Text timeText;
        [SerializeField] private Image timeBarFill;
        [SerializeField] private Text countdownText;
        [SerializeField] private Text stateText;

        private DrawingSession _session;

        public void Bind(DrawingSession session)
        {
            _session = session;
            _session.StateChanged += OnStateChanged;
            _session.TimeChanged += Refresh;
            OnStateChanged(_session.State);
        }

        private void OnDestroy()
        {
            if (_session == null) return;
            _session.StateChanged -= OnStateChanged;
            _session.TimeChanged -= Refresh;
        }

        private void OnStateChanged(DrawingState state)
        {
            stateText.text = state switch
            {
                DrawingState.Idle => "시작 버튼 또는 Space로 시작",
                DrawingState.Countdown => "준비…",
                DrawingState.Drawing => "그리세요!",
                DrawingState.Judging => "계산 중…",
                _ => "결과 — Space로 다시 하기",
            };
            Refresh();
        }

        private void Refresh()
        {
            var state = _session.State;
            bool counting = state == DrawingState.Countdown;
            countdownText.gameObject.SetActive(counting);
            if (counting) countdownText.text = Mathf.CeilToInt(_session.CountdownRemaining).ToString();

            float remaining = state == DrawingState.Countdown ? _session.TimeLimit : _session.TimeRemaining;
            if (state == DrawingState.Idle) remaining = 0f;
            timeText.text = $"{remaining:F1}초";
            timeBarFill.fillAmount = _session.TimeLimit > 0f ? remaining / _session.TimeLimit : 0f;
        }
    }
}
