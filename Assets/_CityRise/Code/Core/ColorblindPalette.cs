#nullable enable

namespace CityRise.Core;

/// <summary>
/// Colorblind-safe palette presets. Overlays and UI consume this via <see cref="IAccessibilityService"/>;
/// hardcoded overlay colors are forbidden (Tech Roadmap section 4.9).
/// </summary>
public enum ColorblindPalette
{
    None,
    Deuteranopia,
    Protanopia,
    Tritanopia,
}
