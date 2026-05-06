#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using CityRise.Core;

namespace CityRise.Persistence;

/// <summary>
/// Orchestrates save and load against a <see cref="SaveManifest"/>. Writes are atomic:
/// content goes to a temp file, then atomic rename on success (Tech Roadmap section 4.6).
/// </summary>
public sealed class SaveService
{
    private readonly SaveManifest _manifest;
    private readonly MigrationRegistry _migrations;
    private readonly Func<string> _buildTagProvider;

    public SaveService(SaveManifest manifest, MigrationRegistry migrations, Func<string>? buildTagProvider = null)
    {
        _manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        _migrations = migrations ?? throw new ArgumentNullException(nameof(migrations));
        _buildTagProvider = buildTagProvider ?? (() => string.Empty);
    }

    /// <summary>Serialize the current state of every registered <see cref="ISaveable"/> to <paramref name="path"/>.</summary>
    public Result<Unit> Save(string path)
    {
        if (string.IsNullOrEmpty(path)) return Result<Unit>.Err("Save path must be non-empty.");

        var header = new SaveHeader
        {
            SavedAtIso = DateTime.UtcNow.ToString("O"),
            BuildTag = _buildTagProvider(),
        };

        var entries = new List<SaveEntry>(_manifest.Count);
        for (int i = 0; i < _manifest.Ordered.Count; i++)
        {
            var saveable = _manifest.Ordered[i];
            try
            {
                entries.Add(new SaveEntry(saveable.SubsystemId, saveable.CurrentSchemaVersion, saveable.Serialize()));
            }
            catch (Exception e)
            {
                return Result<Unit>.Err($"Serialize for '{saveable.SubsystemId}' threw: {e.Message}");
            }
        }

        try
        {
            WriteAtomic(path, header, entries);
            return Result<Unit>.Ok(Unit.Value);
        }
        catch (Exception e)
        {
            return Result<Unit>.Err($"Save write failed: {e.Message}");
        }
    }

    /// <summary>Read <paramref name="path"/> and apply each entry to its registered <see cref="ISaveable"/>, running migrations as needed.</summary>
    public Result<Unit> Load(string path)
    {
        if (string.IsNullOrEmpty(path)) return Result<Unit>.Err("Load path must be non-empty.");
        if (!File.Exists(path)) return Result<Unit>.Err($"Save file not found: {path}");

        SaveHeader header;
        List<SaveEntry> entries;
        try
        {
            using var reader = new StreamReader(path);
            (header, entries) = JsonSaveBackend.Read(reader);
        }
        catch (Exception e)
        {
            return Result<Unit>.Err($"Save file parse failed: {e.Message}");
        }

        if (header.MagicValue != SaveHeader.Magic)
            return Result<Unit>.Err($"Save magic mismatch: '{header.MagicValue}' (expected '{SaveHeader.Magic}').");
        if (header.FormatVersion > SaveHeader.CurrentFormatVersion)
            return Result<Unit>.Err($"Save format v{header.FormatVersion} is newer than this build supports (v{SaveHeader.CurrentFormatVersion}).");

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var saveable = _manifest.Find(entry.SubsystemId);
            if (saveable is null)
            {
                // Unknown subsystem — skip but keep going. Forward-compatible saves can carry
                // entries we don't recognize yet.
                continue;
            }

            var migrationResult = _migrations.Migrate(
                entry.SubsystemId, entry.Blob, entry.SchemaVersion, saveable.CurrentSchemaVersion);
            if (migrationResult.IsErr) return migrationResult;

            try
            {
                saveable.Deserialize(entry.Blob, entry.SchemaVersion);
            }
            catch (Exception e)
            {
                return Result<Unit>.Err($"Deserialize for '{entry.SubsystemId}' threw: {e.Message}");
            }
        }

        return Result<Unit>.Ok(Unit.Value);
    }

    private static void WriteAtomic(string path, SaveHeader header, IReadOnlyList<SaveEntry> entries)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = path + ".tmp";
        using (var writer = new StreamWriter(tempPath))
        {
            JsonSaveBackend.Write(writer, header, entries);
        }

        if (File.Exists(path)) File.Delete(path);
        File.Move(tempPath, path);
    }
}
