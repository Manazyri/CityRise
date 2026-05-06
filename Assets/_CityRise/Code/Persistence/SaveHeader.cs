#nullable enable

namespace CityRise.Persistence;

/// <summary>
/// Top-of-file metadata. Every save starts with this so Load can sanity-check the file before
/// touching the body and pick the right migration path per subsystem.
/// </summary>
public sealed class SaveHeader
{
    /// <summary>Magic string at the top of every CityRise save file.</summary>
    public const string Magic = "CityRise";

    /// <summary>Container format version. Bump when <see cref="SaveBlob"/> on-disk shape changes.</summary>
    public const int CurrentFormatVersion = 1;

    public string MagicValue { get; set; } = Magic;
    public int FormatVersion { get; set; } = CurrentFormatVersion;

    /// <summary>UTC ISO-8601 timestamp at save time.</summary>
    public string SavedAtIso { get; set; } = string.Empty;

    /// <summary>Tag for the build that produced this save (commit hash or version).</summary>
    public string BuildTag { get; set; } = string.Empty;
}
