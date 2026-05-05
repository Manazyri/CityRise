#nullable enable

namespace CityRise.Core;

/// <summary>
/// Locked numeric constants for CityRise. Single grep target for any tuning conversation.
/// Values mirror CLAUDE.md and Tech Roadmap section 6.4. Changing any of these is an ADR-level event.
/// </summary>
public static class GameConstants
{
    // Spatial grid

    /// <summary>Sim/zoning tile edge length, in meters. A 2x2 tile = small house footprint.</summary>
    public const float TileSizeMeters = 8f;

    /// <summary>Heightmap vertex spacing, in meters. Decoupled from sim grid; one tile spans 2 vertex intervals.</summary>
    public const float HeightmapVertexSpacingMeters = 4f;

    /// <summary>Default map edge length, in meters. 2048 m = 256 tiles.</summary>
    public const int DefaultMapSizeMeters = 2048;

    /// <summary>Default map edge length, in tiles.</summary>
    public const int DefaultMapSizeTiles = DefaultMapSizeMeters / (int)TileSizeMeters; // 256

    /// <summary>Heightmap chunk edge length, in vertices. 32x32 vertices at 4 m = 128 m per chunk.</summary>
    public const int HeightmapChunkVerticesPerEdge = 32;

    /// <summary>Heightmap chunk edge length, in meters.</summary>
    public const int HeightmapChunkMeters =
        (HeightmapChunkVerticesPerEdge - 1) * (int)HeightmapVertexSpacingMeters; // 124 — kept here for grep; chunks index by vertex count, not meters.

    // Tick rates

    /// <summary>Sim tick rate at 1x speed, in hertz.</summary>
    public const float SimTickHz = 1f;

    /// <summary>Growth tick rate at 1x speed, in hertz. Once every 10 sim ticks.</summary>
    public const float GrowthTickHz = 0.1f;

    /// <summary>Number of sim ticks per growth tick, at any speed.</summary>
    public const int SimTicksPerGrowthTick = 10;

    /// <summary>Sim ticks per in-game month at 1x speed. One in-game month = budget period.</summary>
    public const int SimTicksPerInGameMonth = 60;

    // Visual agent caps (post-MVP; constants exposed early so call sites compile)

    /// <summary>Hard cap on simultaneously visible pedestrian agents. Post-MVP.</summary>
    public const int MaxVisiblePedestrians = 200;

    /// <summary>Hard cap on simultaneously visible vehicle agents. Post-MVP.</summary>
    public const int MaxVisibleVehicles = 500;

    // Coordinate convention (informational; helpers in CoordinateConventions.cs)
    //
    //   - Y-up, left-handed, 1 unit = 1 meter (Unity defaults).
    //   - Tile (0,0) covers world rect [0, 8) on X and Z.
    //   - Forward = +Z.
    //
    // No constants here for the convention itself; documenting alongside the spatial grid where readers expect it.
}
