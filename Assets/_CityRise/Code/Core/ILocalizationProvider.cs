#nullable enable

namespace CityRise.Core;

/// <summary>
/// Source of localized strings for <see cref="I18n"/>. Production uses the
/// <see cref="LocalizationTable"/> ScriptableObject; tests use an in-memory provider.
/// </summary>
public interface ILocalizationProvider
{
    /// <summary>True if the key was found. <paramref name="value"/> is the raw template (may contain {0}, {1}, ...).</summary>
    bool TryGet(string key, out string value);
}
