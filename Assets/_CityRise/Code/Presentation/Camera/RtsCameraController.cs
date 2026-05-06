#nullable enable

using UnityEngine;
using UnityEngine.InputSystem;

namespace CityRise.Presentation.Camera
{
    /// <summary>
    /// Phase 1 RTS-style camera controller. Drives a CinemachineCamera-bearing GameObject
    /// (the "rig"): WASD/arrow pan on the world XZ plane, scroll-wheel zoom on world Y.
    /// Edge-pan triggers when the mouse hovers within <see cref="EdgePanThresholdPixels"/>
    /// of a screen edge.
    /// </summary>
    /// <remarks>
    /// Phase 1 keeps it minimal: pan, zoom, edge-pan. Orbit and full polish (smoothing,
    /// dampening, accel curves) arrive in Phase 10. Sim-side state for save/load lives in
    /// CameraSaveState (CityRise.Persistence) and reads/writes this transform.
    /// </remarks>
    public sealed class RtsCameraController : MonoBehaviour
    {
        [Header("Pan")]
        [SerializeField] private float _panSpeed = 30f;
        [SerializeField] private float _edgePanSpeed = 24f;
        [SerializeField] private float _edgePanThresholdPixels = 24f;
        [SerializeField] private bool _edgePanEnabled = true;

        [Header("Zoom")]
        [SerializeField] private float _zoomSpeed = 12f;
        [SerializeField] private float _minHeight = 12f;
        [SerializeField] private float _maxHeight = 200f;

        [Header("Bounds")]
        [SerializeField] private bool _clampToBounds = true;
        [SerializeField] private Vector2 _minBounds = new(-2048f, -2048f);
        [SerializeField] private Vector2 _maxBounds = new(2048f, 2048f);

        private void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard == null) return;

            var deltaTime = Time.deltaTime;
            var pan = ReadKeyboardPan(keyboard);

            if (_edgePanEnabled && mouse != null)
            {
                pan += ReadMouseEdgePan(mouse);
            }

            if (pan.sqrMagnitude > 0f)
            {
                ApplyPan(pan, deltaTime);
            }

            if (mouse != null)
            {
                var scrollY = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scrollY) > 0.01f)
                {
                    ApplyZoom(scrollY, deltaTime);
                }
            }
        }

        private static Vector2 ReadKeyboardPan(Keyboard keyboard)
        {
            var pan = Vector2.zero;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) pan.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) pan.y -= 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) pan.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) pan.x += 1f;
            return pan.sqrMagnitude > 0f ? pan.normalized : Vector2.zero;
        }

        private Vector2 ReadMouseEdgePan(Mouse mouse)
        {
            var screen = new Vector2(Screen.width, Screen.height);
            var pos = mouse.position.ReadValue();

            // Ignore when the cursor is outside the game window (common during alt-tab or focus loss).
            if (pos.x < 0f || pos.y < 0f || pos.x > screen.x || pos.y > screen.y) return Vector2.zero;

            var pan = Vector2.zero;
            if (pos.x <= _edgePanThresholdPixels) pan.x -= 1f;
            else if (pos.x >= screen.x - _edgePanThresholdPixels) pan.x += 1f;
            if (pos.y <= _edgePanThresholdPixels) pan.y -= 1f;
            else if (pos.y >= screen.y - _edgePanThresholdPixels) pan.y += 1f;
            return pan.sqrMagnitude > 0f ? pan.normalized * (_edgePanSpeed / _panSpeed) : Vector2.zero;
        }

        private void ApplyPan(Vector2 pan, float deltaTime)
        {
            var move = new Vector3(pan.x, 0f, pan.y) * (_panSpeed * deltaTime);
            var p = transform.position + move;
            if (_clampToBounds)
            {
                p.x = Mathf.Clamp(p.x, _minBounds.x, _maxBounds.x);
                p.z = Mathf.Clamp(p.z, _minBounds.y, _maxBounds.y);
            }
            transform.position = p;
        }

        private void ApplyZoom(float scrollY, float deltaTime)
        {
            var p = transform.position;
            // Scroll up zooms in (lowers camera); scroll down zooms out (raises camera).
            p.y -= scrollY * _zoomSpeed * deltaTime;
            p.y = Mathf.Clamp(p.y, _minHeight, _maxHeight);
            transform.position = p;
        }
    }
}
