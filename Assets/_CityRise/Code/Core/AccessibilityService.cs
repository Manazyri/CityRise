#nullable enable

using System;

namespace CityRise.Core;

/// <summary>
/// Default <see cref="IAccessibilityService"/> implementation. Holds settings in-memory; persistence
/// (writing changes back to <see cref="AccessibilityConfig"/> and to disk) is wired at Bootstrap.
/// </summary>
public sealed class AccessibilityService : IAccessibilityService
{
    private ColorblindPalette _palette;
    private float _uiScale = 1f;
    private bool _reducedMotion;

    public event Action? Changed;

    public AccessibilityService() { }

    public AccessibilityService(AccessibilityConfig config)
    {
        if (config is null) throw new ArgumentNullException(nameof(config));
        _palette = config.ColorblindPalette;
        _uiScale = ClampScale(config.UiScale);
        _reducedMotion = config.ReducedMotion;
    }

    public ColorblindPalette ColorblindPalette
    {
        get => _palette;
        set
        {
            if (_palette == value) return;
            _palette = value;
            Changed?.Invoke();
        }
    }

    public float UiScale
    {
        get => _uiScale;
        set
        {
            var clamped = ClampScale(value);
            if (Math.Abs(_uiScale - clamped) < float.Epsilon) return;
            _uiScale = clamped;
            Changed?.Invoke();
        }
    }

    public bool ReducedMotion
    {
        get => _reducedMotion;
        set
        {
            if (_reducedMotion == value) return;
            _reducedMotion = value;
            Changed?.Invoke();
        }
    }

    private static float ClampScale(float scale)
    {
        if (scale < IAccessibilityService.MinUiScale) return IAccessibilityService.MinUiScale;
        if (scale > IAccessibilityService.MaxUiScale) return IAccessibilityService.MaxUiScale;
        return scale;
    }
}
