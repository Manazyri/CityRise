#nullable enable

using System;
using System.Collections.Generic;
using CityRise.Core;

namespace CityRise.Persistence;

/// <summary>
/// Per-subsystem schema migrations. A migration takes a SaveBlob at <c>fromVersion</c> and
/// transforms it in place to match <c>toVersion</c>. Composes transitively: loading a v1 save
/// where the current code is v3 runs v1→v2 then v2→v3 (Tech Roadmap section 4.6).
/// </summary>
public sealed class MigrationRegistry
{
    private readonly Dictionary<string, Dictionary<int, Migration>> _migrations = new();

    /// <summary>Register a migration that takes <paramref name="subsystemId"/> from <paramref name="fromVersion"/> to <paramref name="fromVersion"/>+1.</summary>
    public void Register(string subsystemId, int fromVersion, Action<SaveBlob> migrate)
    {
        if (string.IsNullOrEmpty(subsystemId)) throw new ArgumentException("subsystemId must be non-empty.", nameof(subsystemId));
        if (fromVersion < 0) throw new ArgumentOutOfRangeException(nameof(fromVersion));
        if (migrate is null) throw new ArgumentNullException(nameof(migrate));

        if (!_migrations.TryGetValue(subsystemId, out var bySubsystem))
        {
            bySubsystem = new Dictionary<int, Migration>();
            _migrations[subsystemId] = bySubsystem;
        }
        if (bySubsystem.ContainsKey(fromVersion))
        {
            throw new InvalidOperationException(
                $"Migration for '{subsystemId}' v{fromVersion} → v{fromVersion + 1} already registered.");
        }
        bySubsystem[fromVersion] = new Migration(fromVersion, fromVersion + 1, migrate);
    }

    /// <summary>
    /// Apply any registered migrations to bring <paramref name="blob"/> from <paramref name="fromVersion"/>
    /// up to <paramref name="toVersion"/>. Returns Err if a step is missing — Tech Roadmap §4.6
    /// requires "Missing migration = refuse to load".
    /// </summary>
    public Result<Unit> Migrate(string subsystemId, SaveBlob blob, int fromVersion, int toVersion)
    {
        if (fromVersion == toVersion) return Result<Unit>.Ok(Unit.Value);
        if (fromVersion > toVersion)
        {
            return Result<Unit>.Err(
                $"Save for '{subsystemId}' is at v{fromVersion} but current code is older v{toVersion}; downgrade not supported.");
        }

        var bySubsystem = _migrations.GetValueOrDefault(subsystemId);
        var current = fromVersion;
        while (current < toVersion)
        {
            if (bySubsystem is null || !bySubsystem.TryGetValue(current, out var step))
            {
                return Result<Unit>.Err(
                    $"Missing migration for '{subsystemId}' v{current} → v{current + 1}; refusing to load.");
            }
            try
            {
                step.Apply(blob);
            }
            catch (Exception e)
            {
                return Result<Unit>.Err(
                    $"Migration '{subsystemId}' v{step.FromVersion} → v{step.ToVersion} threw: {e.Message}");
            }
            current = step.ToVersion;
        }
        return Result<Unit>.Ok(Unit.Value);
    }

    /// <summary>True if a migration is registered for the given subsystem and starting version.</summary>
    public bool HasMigration(string subsystemId, int fromVersion)
        => _migrations.TryGetValue(subsystemId, out var bySubsystem)
           && bySubsystem.ContainsKey(fromVersion);

    private readonly struct Migration
    {
        public readonly int FromVersion;
        public readonly int ToVersion;
        private readonly Action<SaveBlob> _action;

        public Migration(int fromVersion, int toVersion, Action<SaveBlob> action)
        {
            FromVersion = fromVersion;
            ToVersion = toVersion;
            _action = action;
        }

        public void Apply(SaveBlob blob) => _action(blob);
    }
}
