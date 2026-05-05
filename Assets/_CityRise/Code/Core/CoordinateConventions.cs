#nullable enable

using Unity.Mathematics;

namespace CityRise.Core;

/// <summary>
/// World ↔ tile and world ↔ heightmap helpers. Coordinate convention:
/// Y-up, left-handed, 1 unit = 1 meter; tile (0,0) covers world rect [0, 8) × [0, 8) on the X/Z plane;
/// forward = +Z. Tile elevation is the arithmetic mean of the four corner heightmap samples
/// (Tech Roadmap section 4.9).
/// </summary>
public static class CoordinateConventions
{
    /// <summary>Tile edge length, in meters. Mirror of <see cref="GameConstants.TileSizeMeters"/>.</summary>
    public const float TileSizeMeters = GameConstants.TileSizeMeters;

    /// <summary>Heightmap vertex spacing, in meters. Mirror of <see cref="GameConstants.HeightmapVertexSpacingMeters"/>.</summary>
    public const float HeightmapVertexSpacingMeters = GameConstants.HeightmapVertexSpacingMeters;

    /// <summary>
    /// Number of heightmap vertex intervals spanned by one tile edge.
    /// Tile is 8 m, vertex spacing 4 m → 2 intervals (3 vertices per edge counting both ends).
    /// </summary>
    public const int HeightmapVerticesPerTileSpan = 2;

    /// <summary>World position → tile coordinate. Tile (0,0) covers [0, 8) × [0, 8).</summary>
    public static int2 WorldToTile(float3 worldPos)
    {
        var f = new float2(worldPos.x, worldPos.z) / TileSizeMeters;
        return new int2((int)math.floor(f.x), (int)math.floor(f.y));
    }

    /// <summary>Center of a tile in world space. Y is left at 0 — terrain elevation is queried separately.</summary>
    public static float3 TileToWorldCenter(int2 tile)
    {
        return new float3(
            (tile.x + 0.5f) * TileSizeMeters,
            0f,
            (tile.y + 0.5f) * TileSizeMeters);
    }

    /// <summary>South-west corner of a tile in world space (the lower-X, lower-Z corner). Y left at 0.</summary>
    public static float3 TileToWorldCorner(int2 tile)
    {
        return new float3(
            tile.x * TileSizeMeters,
            0f,
            tile.y * TileSizeMeters);
    }

    /// <summary>Heightmap vertex coordinates for the four corners of <paramref name="tile"/>, in vertex-index space.</summary>
    public static (int2 sw, int2 se, int2 ne, int2 nw) TileCornerHeightmapIndices(int2 tile)
    {
        var sw = new int2(
            tile.x * HeightmapVerticesPerTileSpan,
            tile.y * HeightmapVerticesPerTileSpan);
        return (
            sw,
            sw + new int2(HeightmapVerticesPerTileSpan, 0),
            sw + new int2(HeightmapVerticesPerTileSpan, HeightmapVerticesPerTileSpan),
            sw + new int2(0, HeightmapVerticesPerTileSpan));
    }
}
