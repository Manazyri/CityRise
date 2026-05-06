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
    //
    // Integer / float reads accept any compatible numeric type so backends can normalise
    // (the JSON backend stores all integers as Int64 to preserve int64 fidelity; consumers
    // can still ReadInt32 if the value fits).

    public int ReadInt32(string key) => checked((int)ReadIntegral(key));
    public uint ReadUInt32(string key) => checked((uint)ReadIntegral(key));
    public long ReadInt64(string key) => ReadIntegral(key);
    public ulong ReadUInt64(string key) => checked((ulong)ReadIntegral(key));
    public float ReadFloat(string key) => (float)ReadFloatingOrIntegral(key);
    public double ReadDouble(string key) => ReadFloatingOrIntegral(key);
    public bool ReadBool(string key) => ReadExact<bool>(key);
    public string ReadString(string key) => ReadExact<string>(key);
    public SaveBlob ReadBlob(string key) => ReadExact<SaveBlob>(key);

    public IReadOnlyList<SaveBlob> ReadArray(string key) => ReadExact<SaveBlob[]>(key);

    /// <summary>Try-pattern reader. Returns false if key is missing or value can't be widened to int.</summary>
    public bool TryReadInt32(string key, out int value)
    {
        if (_values.TryGetValue(key, out var raw) && TryAsInt64(raw, out var asLong)
            && asLong >= int.MinValue && asLong <= int.MaxValue)
        {
            value = (int)asLong;
            return true;
        }
        value = 0;
        return false;
    }

    private long ReadIntegral(string key)
    {
        var raw = RequireValue(key);
        if (TryAsInt64(raw, out var asLong)) return asLong;
        throw new InvalidCastException(
            $"SaveBlob key '{key}' is {raw?.GetType().Name ?? "null"}, expected an integer type.");
    }

    private double ReadFloatingOrIntegral(string key)
    {
        var raw = RequireValue(key);
        return raw switch
        {
            double d => d,
            float f => f,
            int i => i,
            uint u => u,
            long l => l,
            ulong ul => ul,
            short s => s,
            ushort us => us,
            byte b => b,
            sbyte sb => sb,
            _ => throw new InvalidCastException(
                $"SaveBlob key '{key}' is {raw?.GetType().Name ?? "null"}, expected a numeric type."),
        };
    }

    private T ReadExact<T>(string key)
    {
        var raw = RequireValue(key);
        if (raw is T typed) return typed;
        throw new InvalidCastException(
            $"SaveBlob key '{key}' is {raw?.GetType().Name ?? "null"}, expected {typeof(T).Name}.");
    }

    private object? RequireValue(string key)
    {
        if (!_values.TryGetValue(key, out var raw))
            throw new KeyNotFoundException($"SaveBlob missing key '{key}'.");
        return raw;
    }

    private static bool TryAsInt64(object? raw, out long value)
    {
        switch (raw)
        {
            case int i: value = i; return true;
            case uint u: value = u; return true;
            case long l: value = l; return true;
            case ulong ul when ul <= long.MaxValue: value = (long)ul; return true;
            case short s: value = s; return true;
            case ushort us: value = us; return true;
            case byte b: value = b; return true;
            case sbyte sb: value = sb; return true;
            default: value = 0; return false;
        }
    }

    private static string Require(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("SaveBlob key must be non-empty.", nameof(key));
        return key;
    }
}
