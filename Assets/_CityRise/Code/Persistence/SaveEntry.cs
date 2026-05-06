#nullable enable

namespace CityRise.Persistence;

/// <summary>One subsystem's contribution to a save file: its id, the version of the schema in
/// effect when the save was written, and the data itself.</summary>
public readonly struct SaveEntry
{
    public readonly string SubsystemId;
    public readonly int SchemaVersion;
    public readonly SaveBlob Blob;

    public SaveEntry(string subsystemId, int schemaVersion, SaveBlob blob)
    {
        SubsystemId = subsystemId;
        SchemaVersion = schemaVersion;
        Blob = blob;
    }
}
