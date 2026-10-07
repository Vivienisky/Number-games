using Game2048.Core;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Game2048.Controls
{
    
    public sealed class SwipeInput : IInputSource
    {
        private readonly float _minDistanceFraction;
        private Vector2 _startPosition;
        private bool _isTracking;

        public SwipeInput(float minDistanceFraction) => _minDistanceFraction = minDistanceFraction;

        public bool TryGetDirection(out Direction direction)
        {
            direction = default;
            if (!TryReadPointer(out bool pressed, out bool released, out Vector2 position))
                return false;

            if (pressed)
            {
                _startPosition = position;
                _isTracking = true;
                return false;
            }

            if (!released || !_isTracking)
                return false;

            _isTracking = false;
            Vector2 delta = position - _startPosition;
            float minDistance = Mathf.Min(Screen.width, Screen.height) * _minDistanceFraction;
            if (delta.sqrMagnitude < minDistance * minDistance)
                return false;

            direction = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                ? (delta.x > 0f ? Direction.Right : Direction.Left)
                : (delta.y > 0f ? Direction.Up : Direction.Down);
            return true;
        }

        private static bool TryReadPointer(out bool pressed, out bool released, out Vector2 position)
        {
#if ENABLE_INPUT_SYSTEM
            var pointer = Pointer.current;
            if (pointer == null)
            {
                pressed = released = false;
                position = default;
                return false;
            }

            pressed = pointer.press.wasPressedThisFrame;
            released = pointer.press.wasReleasedThisFrame;
            position = pointer.position.ReadValue();
            return true;
#else
            pressed = Input.GetMouseButtonDown(0);
            released = Input.GetMouseButtonUp(0);
            position = Input.mousePosition;
            return true;
#endif
        }
    }
}
