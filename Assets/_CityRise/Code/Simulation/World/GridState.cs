#nullable enable

using System;
using Unity.Collections;
using Unity.Mathematics;

namespace CityRise.Simulation.World;

/// <summary>
/// Authoritative tile data store for the sim grid. Struct-of-Arrays — one
/// <see cref="NativeArray{T}"/> per field — so Burst-compiled hot paths can iterate
/// individual fields without striding through unrelated data (Tech Roadmap §3, §4.2).
/// </summary>
/// <remarks>
/// Square grid; row-major layout; <c>index = y * sizeInTiles + x</c> via <see cref="GridService.IndexOf"/>.
/// The grid is sized at construction and never resized; map-resize semantics belong to a
/// later phase.
///
/// <see cref="Allocator.Persistent"/> is the production allocator. Tests can pass a different
/// allocator (Temp / TempJob) via the constructor for tighter scoping.
///
/// Implements both <see cref="IGridRead"/> and <see cref="IGridMutate"/> so
/// <see cref="WorldState"/> can hand out the appropriate facet per ADR-0007.
/// </remarks>
public sealed class GridState : IGridMutate, IDisposable
{
    public int SizeInTiles { get; }

    private NativeArray<float> _elevation;
    private NativeArray<TerrainType> _terrainType;
    private NativeArray<ZoneType> _zoneType;
    private NativeArray<byte> _densityCap;
    private NativeArray<float> _desirability;
    private NativeArray<float> _pollution;
    private NativeArray<byte> _powerCoverage;
    private NativeArray<byte> _waterCoverage;

    private bool _disposed;

    public GridState(int sizeInTiles, Allocator allocator = Allocator.Persistent)
    {
        if (sizeInTiles <= 0)
            throw new ArgumentOutOfRangeException(nameof(sizeInTiles), "Grid size must be positive.");

        SizeInTiles = sizeInTiles;
        var count = GridService.TileCount(sizeInTiles);

        _elevation     = new NativeArray<float>(count, allocator, NativeArrayOptions.ClearMemory);
        _terrainType   = new NativeArray<TerrainType>(count, allocator, NativeArrayOptions.ClearMemory);
        _zoneType      = new NativeArray<ZoneType>(count, allocator, NativeArrayOptions.ClearMemory);
        _densityCap    = new NativeArray<byte>(count, allocator, NativeArrayOptions.ClearMemory);
        _desirability  = new NativeArray<float>(count, allocator, NativeArrayOptions.ClearMemory);
        _pollution     = new NativeArray<float>(count, allocator, NativeArrayOptions.ClearMemory);
        _powerCoverage = new NativeArray<byte>(count, allocator, NativeArrayOptions.ClearMemory);
        _waterCoverage = new NativeArray<byte>(count, allocator, NativeArrayOptions.ClearMemory);
    }

    // ---------- read ----------

    public float        GetElevation(int2 tile)     => _elevation[Index(tile)];
    public TerrainType  GetTerrainType(int2 tile)   => _terrainType[Index(tile)];
    public ZoneType     GetZoneType(int2 tile)      => _zoneType[Index(tile)];
    public byte         GetDensityCap(int2 tile)    => _densityCap[Index(tile)];
    public float        GetDesirability(int2 tile)  => _desirability[Index(tile)];
    public float        GetPollution(int2 tile)     => _pollution[Index(tile)];
    public bool         GetPowerCoverage(int2 tile) => _powerCoverage[Index(tile)] != 0;
    public bool         GetWaterCoverage(int2 tile) => _waterCoverage[Index(tile)] != 0;

    // ---------- mutate ----------

    public void SetElevation(int2 tile, float value)        { _elevation[Index(tile)] = value; }
    public void SetTerrainType(int2 tile, TerrainType value){ _terrainType[Index(tile)] = value; }
    public void SetZoneType(int2 tile, ZoneType value)      { _zoneType[Index(tile)] = value; }
    public void SetDensityCap(int2 tile, byte value)        { _densityCap[Index(tile)] = value; }
    public void SetDesirability(int2 tile, float value)     { _desirability[Index(tile)] = value; }
    public void SetPollution(int2 tile, float value)        { _pollution[Index(tile)] = value; }
    public void SetPowerCoverage(int2 tile, bool value)     { _powerCoverage[Index(tile)] = (byte)(value ? 1 : 0); }
    public void SetWaterCoverage(int2 tile, bool value)     { _waterCoverage[Index(tile)] = (byte)(value ? 1 : 0); }

    // ---------- lifetime ----------

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_elevation.IsCreated)     _elevation.Dispose();
        if (_terrainType.IsCreated)   _terrainType.Dispose();
        if (_zoneType.IsCreated)      _zoneType.Dispose();
        if (_densityCap.IsCreated)    _densityCap.Dispose();
        if (_desirability.IsCreated)  _desirability.Dispose();
        if (_pollution.IsCreated)     _pollution.Dispose();
        if (_powerCoverage.IsCreated) _powerCoverage.Dispose();
        if (_waterCoverage.IsCreated) _waterCoverage.Dispose();
    }

    private int Index(int2 tile)
    {
        GridService.RequireInRange(tile, SizeInTiles);
        return GridService.IndexOf(tile, SizeInTiles);
    }
}
