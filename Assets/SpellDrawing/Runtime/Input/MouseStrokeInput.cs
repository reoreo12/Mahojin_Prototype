// Input System의 마우스 왼쪽 버튼으로 획 입력을 만든다 (IStrokeInput 구현).
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VoxelMagic.SpellDrawing
{
    public sealed class MouseStrokeInput : MonoBehaviour, IStrokeInput
    {
        private bool _pressed;
        private Vector2 _lastPosition;

        public event Action<Vector2> StrokeBegan;
        public event Action<Vector2> StrokeMoved;
        public event Action<Vector2> StrokeEnded;

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                Release(_lastPosition);
                return;
            }

            Vector2 position = mouse.position.ReadValue();
            if (mouse.leftButton.wasPressedThisFrame)
            {
                Release(_lastPosition);
                _pressed = true;
                _lastPosition = position;
                StrokeBegan?.Invoke(position);
                return;
            }
            if (!_pressed) return;

            if (position != _lastPosition)
            {
                _lastPosition = position;
                StrokeMoved?.Invoke(position);
            }
            if (!mouse.leftButton.isPressed) Release(position);
        }

        private void OnDisable() => Release(_lastPosition);

        private void Release(Vector2 position)
        {
            if (!_pressed) return;
            _pressed = false;
            StrokeEnded?.Invoke(position);
        }
    }
}
