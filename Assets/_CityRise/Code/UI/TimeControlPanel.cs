#nullable enable

using System;
using CityRise.Simulation.Infrastructure;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace CityRise.UI
{
    /// <summary>
    /// Pause / 1× / 2× / 3× time-control buttons rendered into <see cref="HudShell.TopBarStats"/>,
    /// plus keyboard hotkeys: Space toggles pause, digits 1/2/3 set the speed.
    /// Bound to a <see cref="TickScheduler"/> by Bootstrap after the Main scene loads.
    /// </summary>
    [RequireComponent(typeof(HudShell))]
    public sealed class TimeControlPanel : MonoBehaviour
    {
        private const string ContainerClass = "time-control";
        private const string ButtonClass = "time-control__button";
        private const string ButtonActiveClass = "time-control__button--active";

        private HudShell _shell = null!;
        private TickScheduler? _scheduler;

        private VisualElement? _container;
        private Button? _pauseButton;
        private Button? _normalButton;
        private Button? _fastButton;
        private Button? _fasterButton;

        private SpeedMultiplier _previousNonPauseSpeed = SpeedMultiplier.Normal;

        private void Awake()
        {
            _shell = GetComponent<HudShell>();
        }

        /// <summary>Wire the controlled scheduler. Called by Bootstrap.</summary>
        public void Bind(TickScheduler scheduler)
        {
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            BuildButtons();
            RefreshActiveState();
        }

        private void OnEnable()
        {
            // OnEnable can fire before Bind (HUD GameObject's OnEnable runs at scene load,
            // Bootstrap's sceneLoaded callback fires shortly after). Build buttons lazily —
            // Bind triggers BuildButtons too, so whichever happens last wins idempotently.
            BuildButtons();
        }

        private void OnDisable()
        {
            DisposeButtons();
        }

        private void Update()
        {
            if (_scheduler == null) return;
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                TogglePause();
            }
            else if (keyboard.digit1Key.wasPressedThisFrame)
            {
                SetSpeed(SpeedMultiplier.Normal);
            }
            else if (keyboard.digit2Key.wasPressedThisFrame)
            {
                SetSpeed(SpeedMultiplier.Fast);
            }
            else if (keyboard.digit3Key.wasPressedThisFrame)
            {
                SetSpeed(SpeedMultiplier.Faster);
            }
        }

        private void BuildButtons()
        {
            if (_shell == null) return;
            var stats = _shell.TopBarStats;
            if (stats == null) return;
            if (_container != null) return; // already built

            _container = new VisualElement { name = "time-control" };
            _container.AddToClassList(ContainerClass);
            // Defensive inline layout — main USS pass arrives in Phase 10.
            _container.style.flexDirection = FlexDirection.Row;

            _pauseButton = MakeButton("Pause", () => SetSpeed(SpeedMultiplier.Paused));
            _normalButton = MakeButton("1x", () => SetSpeed(SpeedMultiplier.Normal));
            _fastButton = MakeButton("2x", () => SetSpeed(SpeedMultiplier.Fast));
            _fasterButton = MakeButton("3x", () => SetSpeed(SpeedMultiplier.Faster));

            _container.Add(_pauseButton);
            _container.Add(_normalButton);
            _container.Add(_fastButton);
            _container.Add(_fasterButton);

            stats.Add(_container);
            RefreshActiveState();
        }

        private void DisposeButtons()
        {
            if (_container != null && _container.parent != null)
            {
                _container.RemoveFromHierarchy();
            }
            _container = null;
            _pauseButton = null;
            _normalButton = null;
            _fastButton = null;
            _fasterButton = null;
        }

        private static Button MakeButton(string label, Action onClick)
        {
            var btn = new Button(onClick) { text = label };
            btn.AddToClassList(ButtonClass);
            // Inline width/height beat the default Button class's USS specificity, which
            // would otherwise pin a small implicit size.
            btn.style.width = 64;
            btn.style.height = 32;
            btn.style.marginLeft = 4;
            btn.style.marginRight = 4;
            btn.style.flexShrink = 0;
            btn.style.flexGrow = 0;
            btn.style.fontSize = 14;
            btn.style.unityTextAlign = TextAnchor.MiddleCenter;
            return btn;
        }

        private void TogglePause()
        {
            if (_scheduler == null) return;
            if (_scheduler.Speed == SpeedMultiplier.Paused)
            {
                SetSpeed(_previousNonPauseSpeed);
            }
            else
            {
                _previousNonPauseSpeed = _scheduler.Speed;
                SetSpeed(SpeedMultiplier.Paused);
            }
        }

        private void SetSpeed(SpeedMultiplier speed)
        {
            if (_scheduler == null) return;
            if (_scheduler.Speed == speed) return;
            _scheduler.Speed = speed;
            if (speed != SpeedMultiplier.Paused)
            {
                _previousNonPauseSpeed = speed;
            }
            RefreshActiveState();
        }

        private void RefreshActiveState()
        {
            if (_scheduler == null) return;
            SetActive(_pauseButton, _scheduler.Speed == SpeedMultiplier.Paused);
            SetActive(_normalButton, _scheduler.Speed == SpeedMultiplier.Normal);
            SetActive(_fastButton, _scheduler.Speed == SpeedMultiplier.Fast);
            SetActive(_fasterButton, _scheduler.Speed == SpeedMultiplier.Faster);
        }

        private static void SetActive(Button? button, bool active)
        {
            if (button == null) return;
            if (active) button.AddToClassList(ButtonActiveClass);
            else button.RemoveFromClassList(ButtonActiveClass);
        }
    }
}
