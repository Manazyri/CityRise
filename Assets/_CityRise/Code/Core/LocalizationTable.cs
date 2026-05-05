#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CityRise.Core;

/// <summary>
/// In-memory localization provider backed by a serializable list of (key, value) pairs.
/// Authored as a ScriptableObject in the editor and assigned at Bootstrap. English-only for MVP;
/// per-language tables come post-MVP.
/// </summary>
[CreateAssetMenu(fileName = "LocalizationTable", menuName = "CityRise/Core/Localization Table", order = 0)]
public sealed class LocalizationTable : ScriptableObject, ILocalizationProvider
{
    [SerializeField] private List<Entry> _entries = new();

    private Dictionary<string, string>? _index;

    [Serializable]
    public struct Entry
    {
        public string Key;
        [TextArea(1, 5)] public string Value;
    }

    public bool TryGet(string key, out string value)
    {
        EnsureIndex();
        if (_index!.TryGetValue(key, out var found))
        {
            value = found;
            return true;
        }
        value = string.Empty;
        return false;
    }

    /// <summary>Force a rebuild of the lookup index. Call after editing entries at runtime.</summary>
    public void Rebuild()
    {
        _index = new Dictionary<string, string>(_entries.Count);
        for (int i = 0; i < _entries.Count; i++)
        {
            var e = _entries[i];
            if (!string.IsNullOrEmpty(e.Key))
                _index[e.Key] = e.Value ?? string.Empty;
        }
    }

    private void EnsureIndex()
    {
        if (_index is null) Rebuild();
    }

    private void OnEnable() => _index = null; // lazy build on first access
    private void OnValidate() => _index = null; // editor edits invalidate the cache
}
