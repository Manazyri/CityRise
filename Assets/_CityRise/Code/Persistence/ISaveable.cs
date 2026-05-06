#nullable enable

namespace CityRise.Persistence;

/// <summary>
/// One subsystem's contribution to a save file. Each ISaveable carries its own schema version;
/// the save file records (id, version) per entry so Load can run migrations when versions
/// don't match the current code (Tech Roadmap section 4.6).
/// </summary>
public interface ISaveable
{
    /// <summary>Stable identifier — never change this once a save has been written with it.</summary>
    string SubsystemId { get; }

    /// <summary>Current schema version. Bump whenever <see cref="Serialize"/>'s output shape changes.</summary>
    int CurrentSchemaVersion { get; }

    /// <summary>Build a SaveBlob describing this subsystem's current state.</summary>
    SaveBlob Serialize();

    /// <summary>
    /// Restore subsystem state from <paramref name="blob"/>. The blob has already been migrated
    /// up to <see cref="CurrentSchemaVersion"/> by the SaveService — implementations only need to
    /// handle the current shape.
    /// </summary>
    /// <param name="blob">Migrated payload.</param>
    /// <param name="fromVersion">The version on disk before migration. Implementations rarely need
    /// this since the SaveService applies migrations first; included for diagnostic logging.</param>
    void Deserialize(SaveBlob blob, int fromVersion);
}
