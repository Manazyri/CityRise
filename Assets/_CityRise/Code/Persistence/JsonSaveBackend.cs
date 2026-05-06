#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CityRise.Persistence;

/// <summary>
/// JSON serializer for <see cref="SaveBlob"/>, header, and per-subsystem entries. Phase 1 ships
/// with this backend only; MemoryPack binary backend follows in Phase 2 with the same on-blob
/// shape so saves transcribe between the two.
/// </summary>
/// <remarks>
/// File layout:
/// <code>
/// {
///   "header": { "magic": "CityRise", "format": 1, "savedAt": "...", "buildTag": "..." },
///   "entries": [
///     { "id": "TickScheduler", "version": 1, "data": { ... } },
///     { "id": "Camera",        "version": 1, "data": { ... } }
///   ]
/// }
/// </code>
/// Pretty-printed for diffability — Tech Roadmap section 4.6 calls for diffable saves in dev.
/// </remarks>
public static class JsonSaveBackend
{
    /// <summary>Serialize header + entries to JSON. Each entry is (id, version, blob).</summary>
    public static void Write(TextWriter output, SaveHeader header, IReadOnlyList<SaveEntry> entries)
    {
        if (output is null) throw new ArgumentNullException(nameof(output));
        if (header is null) throw new ArgumentNullException(nameof(header));
        if (entries is null) throw new ArgumentNullException(nameof(entries));

        var root = new JObject
        {
            ["header"] = new JObject
            {
                ["magic"] = header.MagicValue,
                ["format"] = header.FormatVersion,
                ["savedAt"] = header.SavedAtIso,
                ["buildTag"] = header.BuildTag,
            },
        };

        var arr = new JArray();
        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            arr.Add(new JObject
            {
                ["id"] = e.SubsystemId,
                ["version"] = e.SchemaVersion,
                ["data"] = BlobToToken(e.Blob),
            });
        }
        root["entries"] = arr;

        using var writer = new JsonTextWriter(output) { Formatting = Formatting.Indented, CloseOutput = false };
        root.WriteTo(writer);
    }

    /// <summary>Parse JSON into a header and entries list.</summary>
    public static (SaveHeader Header, List<SaveEntry> Entries) Read(TextReader input)
    {
        if (input is null) throw new ArgumentNullException(nameof(input));

        using var reader = new JsonTextReader(input) { CloseInput = false };
        var root = JObject.Load(reader);

        var headerObj = root["header"] as JObject ?? throw new InvalidDataException("Save missing 'header'.");
        var header = new SaveHeader
        {
            MagicValue = (string?)headerObj["magic"] ?? string.Empty,
            FormatVersion = (int?)headerObj["format"] ?? 0,
            SavedAtIso = (string?)headerObj["savedAt"] ?? string.Empty,
            BuildTag = (string?)headerObj["buildTag"] ?? string.Empty,
        };

        var entriesToken = root["entries"] as JArray ?? throw new InvalidDataException("Save missing 'entries' array.");
        var entries = new List<SaveEntry>(entriesToken.Count);
        foreach (var token in entriesToken)
        {
            var obj = (JObject)token;
            var id = (string?)obj["id"] ?? throw new InvalidDataException("Save entry missing 'id'.");
            var version = (int?)obj["version"] ?? throw new InvalidDataException($"Save entry '{id}' missing 'version'.");
            var dataToken = obj["data"] as JObject ?? throw new InvalidDataException($"Save entry '{id}' missing 'data' object.");
            entries.Add(new SaveEntry(id, version, TokenToBlob(dataToken)));
        }
        return (header, entries);
    }

    private static JToken BlobToToken(SaveBlob blob)
    {
        var obj = new JObject();
        foreach (var kv in blob.Entries)
        {
            obj[kv.Key] = ValueToToken(kv.Value);
        }
        return obj;
    }

    private static JToken ValueToToken(object? value)
    {
        switch (value)
        {
            case null: return JValue.CreateNull();
            case SaveBlob nested: return BlobToToken(nested);
            case SaveBlob[] array:
                var arr = new JArray();
                foreach (var item in array) arr.Add(BlobToToken(item));
                return arr;
            default: return JToken.FromObject(value);
        }
    }

    private static SaveBlob TokenToBlob(JObject obj)
    {
        var blob = new SaveBlob();
        foreach (var prop in obj.Properties())
        {
            switch (prop.Value.Type)
            {
                case JTokenType.Object:
                    blob.Write(prop.Name, TokenToBlob((JObject)prop.Value));
                    break;
                case JTokenType.Array:
                    var array = (JArray)prop.Value;
                    var items = new SaveBlob[array.Count];
                    for (int i = 0; i < array.Count; i++)
                    {
                        items[i] = TokenToBlob((JObject)array[i]);
                    }
                    blob.WriteArray(prop.Name, items);
                    break;
                case JTokenType.Integer:
                    // JSON int could be int32, int64, uint32, uint64 — preserve fidelity by reading the largest type.
                    blob.Write(prop.Name, (long)prop.Value);
                    break;
                case JTokenType.Float:
                    blob.Write(prop.Name, (double)prop.Value);
                    break;
                case JTokenType.Boolean:
                    blob.Write(prop.Name, (bool)prop.Value);
                    break;
                case JTokenType.String:
                    blob.Write(prop.Name, (string)prop.Value!);
                    break;
                default:
                    throw new InvalidDataException(
                        $"Unsupported JSON token '{prop.Value.Type}' at key '{prop.Name}'.");
            }
        }
        return blob;
    }
}
