#nullable enable

using System;
using System.Collections.Generic;

namespace CityRise.Persistence;

/// <summary>
/// Backend-neutral typed key-value tree for one ISaveable's data. Backends
/// (JSON now, MemoryPack later) translate between SaveBlob and bytes/text.
/// </summary>
/// <remarks>
/// Authoring is two-phase: writers populate via the typed Write methods; readers consume via Read.
/// Nested blobs and arrays of blobs handle composite state. Keys are case-sensitive and unique
/// within a blob — last write wins.
/// </remarks>
public sealed class SaveBlob
{
    private readonly Dictionary<string, object?> _values = new();

    public IReadOnlyDictionary<string, object?> Entries => _values;

    public bool ContainsKey(string key) => _values.ContainsKey(key);

    // ---------- writers ----------

    public void Write(string key, int value) => _values[Require(key)] = value;
    public void Write(string key, uint value) => _values[Require(key)] = value;
    public void Write(string key, long value) => _values[Require(key)] = value;
    public void Write(string key, ulong value) => _values[Require(key)] = value;
    public void Write(string key, float value) => _values[Require(key)] = value;
    public void Write(string key, double value) => _values[Require(key)] = value;
    public void Write(string key, bool value) => _values[Require(key)] = value;
    public void Write(string key, string value) => _values[Require(key)] = value;
    public void Write(string key, SaveBlob nested) => _values[Require(key)] = nested ?? throw new ArgumentNullException(nameof(nested));

    /// <summary>Write an array of nested blobs (e.g. per-tile state).</summary>
    public void WriteArray(string key, IReadOnlyList<SaveBlob> items)
    {
        if (items is null) throw new ArgumentNullException(nameof(items));
        var copy = new SaveBlob[items.Count];
        for (int i = 0; i < items.Count; i++) copy[i] = items[i];
        _values[Require(key)] = copy;
    }

    // ---------- readers ----------

    public int ReadInt32(string key) => Read<int>(key);
    public uint ReadUInt32(string key) => Read<uint>(key);
    public long ReadInt64(string key) => Read<long>(key);
    public ulong ReadUInt64(string key) => Read<ulong>(key);
    public float ReadFloat(string key) => Read<float>(key);
    public double ReadDouble(string key) => Read<double>(key);
    public bool ReadBool(string key) => Read<bool>(key);
    public string ReadString(string key) => Read<string>(key);
    public SaveBlob ReadBlob(string key) => Read<SaveBlob>(key);

    public IReadOnlyList<SaveBlob> ReadArray(string key)
    {
        var raw = Read<SaveBlob[]>(key);
        return raw;
    }

    /// <summary>Try-pattern reader. Returns false if key is missing or type is wrong.</summary>
    public bool TryReadInt32(string key, out int value)
    {
        if (_values.TryGetValue(key, out var raw) && raw is int i)
        {
            value = i;
            return true;
        }
        value = 0;
        return false;
    }

    private T Read<T>(string key)
    {
        if (!_values.TryGetValue(key, out var raw))
            throw new KeyNotFoundException($"SaveBlob missing key '{key}'.");
        if (raw is T typed) return typed;
        throw new InvalidCastException(
            $"SaveBlob key '{key}' is {raw?.GetType().Name ?? "null"}, expected {typeof(T).Name}.");
    }

    private static string Require(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("SaveBlob key must be non-empty.", nameof(key));
        return key;
    }
}
