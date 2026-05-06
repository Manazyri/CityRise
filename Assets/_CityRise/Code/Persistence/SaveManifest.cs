#nullable enable

using System;
using System.Collections.Generic;

namespace CityRise.Persistence;

/// <summary>
/// Ordered list of <see cref="ISaveable"/> instances participating in save/load.
/// Order matters — saves and loads run in manifest order, so dependent subsystems
/// (e.g. RoadNetwork before BuildingState) register after their prerequisites.
/// </summary>
public sealed class SaveManifest
{
    private readonly List<ISaveable> _ordered = new();
    private readonly Dictionary<string, ISaveable> _byId = new();

    /// <summary>Number of registered saveables.</summary>
    public int Count => _ordered.Count;

    /// <summary>Registered saveables in registration order.</summary>
    public IReadOnlyList<ISaveable> Ordered => _ordered;

    /// <summary>Register a saveable. Throws if its <see cref="ISaveable.SubsystemId"/> is already taken.</summary>
    public void Register(ISaveable saveable)
    {
        if (saveable is null) throw new ArgumentNullException(nameof(saveable));
        var id = saveable.SubsystemId;
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ISaveable.SubsystemId must be non-empty.", nameof(saveable));
        if (_byId.ContainsKey(id))
            throw new InvalidOperationException($"SubsystemId '{id}' is already registered.");
        _byId[id] = saveable;
        _ordered.Add(saveable);
    }

    /// <summary>Resolve a saveable by id, or null if not registered.</summary>
    public ISaveable? Find(string id) => _byId.GetValueOrDefault(id);

    /// <summary>Drop all registrations. Test helper.</summary>
    public void Clear()
    {
        _ordered.Clear();
        _byId.Clear();
    }
}
