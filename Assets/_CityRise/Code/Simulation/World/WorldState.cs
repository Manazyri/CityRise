#nullable enable

using System;
using Unity.Collections;

namespace CityRise.Simulation.World;

/// <summary>
/// The single authoritative container for every mutable simulation field (Tech Roadmap §4.2).
/// Owns the per-system state objects (Phase 2 ships <see cref="GridState"/>; later phases add
/// RoadState, BuildingState, BudgetState, GameState).
/// </summary>
/// <remarks>
/// Implements both <see cref="IWorldRead"/> and <see cref="IWorldMutate"/>. Bootstrap composes
/// the service graph so non-System code receives the read-only handle and Systems receive the
/// mutating handle (ADR-0007). The class is the same instance either way — the interfaces are
/// purely structural.
/// </remarks>
public sealed class WorldState : IWorldMutate, IDisposable
{
    private readonly GridState _grid;
    private bool _disposed;

    public WorldState(int gridSizeInTiles, Allocator allocator = Allocator.Persistent)
    {
        _grid = new GridState(gridSizeInTiles, allocator);
    }

    /// <summary>Default-constructed map at <see cref="GameConstants.DefaultMapSizeTiles"/>.</summary>
    public static WorldState CreateDefault(Allocator allocator = Allocator.Persistent)
        => new(CityRise.Core.GameConstants.DefaultMapSizeTiles, allocator);

    // IWorldRead.Grid (non-mutating view)
    IGridRead IWorldRead.Grid => _grid;

    // IWorldMutate.Grid (mutating view)
    public IGridMutate Grid => _grid;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _grid.Dispose();
    }
}
