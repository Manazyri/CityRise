#nullable enable

namespace CityRise.Simulation.World;

/// <summary>
/// Tile terrain category. Drives flat-shaded terrain color (Phase 3) and zoning eligibility
/// (Phase 5+). Byte-backed for compact NativeArray packing.
/// </summary>
public enum TerrainType : byte
{
    Grass = 0,
    Dirt = 1,
    Sand = 2,
    Rock = 3,
    Snow = 4,
    Water = 5,
}
