#nullable enable

using System;

namespace CityRise.Core;

/// <summary>
/// User-facing string lookup. Every UI/notification/error string goes through here so
/// localization is a Phase-0 facade, not a late port. Missing keys return
/// <c>[KEY:foo.bar]</c> so they're visible during development instead of silently rendering empty.
/// </summary>
public static class I18n
{
    private static ILocalizationProvider s_provider = NullProvider.Instance;

    /// <summary>Replace the active provider. Call from Bootstrap once a LocalizationTable has loaded.</summary>
    public static void SetProvider(ILocalizationProvider provider)
    {
        s_provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <summary>Reset to the null provider. Test-only.</summary>
    public static void ResetProvider() => s_provider = NullProvider.Instance;

    /// <summary>
    /// Lookup raw value for <paramref name="key"/>. Returns <c>[KEY:&lt;key&gt;]</c> if missing.
    /// </summary>
    public static string Get(string key)
    {
        if (string.IsNullOrEmpty(key)) return "[KEY:]";
        return s_provider.TryGet(key, out var value) ? value : MissingMarker(key);
    }

    /// <summary>
    /// Lookup and format with <see cref="string.Format(string, object[])"/>. Returns
    /// <c>[KEY:&lt;key&gt;]</c> if missing; falls back to the raw template if formatting throws.
    /// </summary>
    public static string Get(string key, params object[] args)
    {
        if (string.IsNullOrEmpty(key)) return "[KEY:]";
        if (!s_provider.TryGet(key, out var template)) return MissingMarker(key);
        if (args is null || args.Length == 0) return template;

        try
        {
            return string.Format(template, args);
        }
        catch (FormatException)
        {
            return template;
        }
    }

    private static string MissingMarker(string key) => $"[KEY:{key}]";

    /// <summary>Default provider. Always misses; every key renders as the missing marker.</summary>
    private sealed class NullProvider : ILocalizationProvider
    {
        public static readonly NullProvider Instance = new();
        public bool TryGet(string key, out string value)
        {
            value = string.Empty;
            return false;
        }
    }
}
