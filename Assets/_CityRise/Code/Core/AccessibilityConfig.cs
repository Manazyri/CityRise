#nullable enable

using UnityEngine;

namespace CityRise.Core;

/// <summary>
/// Authored defaults for accessibility settings. Bootstrap loads this and constructs an
/// <see cref="AccessibilityService"/> initialized from these values; runtime mutations are
/// held in the service, not pushed back to this asset.
/// </summary>
[CreateAssetMenu(fileName = "AccessibilityConfig", menuName = "CityRise/Core/Accessibility Config", order = 1)]
public sealed class AccessibilityConfig : ScriptableObject
{
    [SerializeField] private ColorblindPalette _colorblindPalette = ColorblindPalette.None;
    [SerializeField, Range(IAccessibilityService.MinUiScale, IAccessibilityService.MaxUiScale)]
    private float _uiScale = 1f;
    [SerializeField] private bool _reducedMotion;

    public ColorblindPalette ColorblindPalette => _colorblindPalette;
    public float UiScale => _uiScale;
    public bool ReducedMotion => _reducedMotion;
}
