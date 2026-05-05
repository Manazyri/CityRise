#nullable enable

using System;

namespace CityRise.Core;

/// <summary>
/// Project-wide accessibility settings. Read-or-mutated from UI; consumed by overlays, UI text
/// scaling, and animation systems. Stubbed in Phase 0 — RemappableInput is a Phase-1+ extension.
/// </summary>
public interface IAccessibilityService
{
    ColorblindPalette ColorblindPalette { get; set; }

    /// <summary>UI scale multiplier. Clamped to [<see cref="MinUiScale"/>, <see cref="MaxUiScale"/>].</summary>
    float UiScale { get; set; }

    /// <summary>Reduce non-essential motion (camera shakes, parallax, particle bursts).</summary>
    bool ReducedMotion { get; set; }

    /// <summary>Fired whenever any setting changes. Subscribers re-read via the properties above.</summary>
    event Action? Changed;

    /// <summary>Lower bound for <see cref="UiScale"/>.</summary>
    public const float MinUiScale = 0.8f;

    /// <summary>Upper bound for <see cref="UiScale"/>.</summary>
    public const float MaxUiScale = 1.5f;
}
