#nullable enable

using Unity.Mathematics;

namespace CityRise.Simulation.World;

/// <summary>
/// Read-only view of <see cref="GridState"/>. Handed to UI, Presentation, Tools, overlays —
/// anything that displays tile data without mutating it (ADR-0007).
/// </summary>
/// <remarks>
/// All getters take a tile coordinate. Out-of-range coordinates throw — sim code should
/// always check via <see cref="GridService"/>. <see cref="ContainsTile"/> before querying.
/// </remarks>
public interface IGridRead
{
    /// <summary>Tile-edge length of the grid (square). Tile (0,0) sits at world origin.</summary>
    int SizeInTiles { get; }

    float GetElevation(int2 tile);
    TerrainType GetTerrainType(int2 tile);
    ZoneType GetZoneType(int2 tile);
    byte GetDensityCap(int2 tile);
    float GetDesirability(int2 tile);
    float GetPollution(int2 tile);
    bool GetPowerCoverage(int2 tile);
    bool GetWaterCoverage(int2 tile);
}
