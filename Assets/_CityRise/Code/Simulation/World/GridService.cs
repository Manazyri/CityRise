#nullable enable

using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace CityRise.Simulation.World;

/// <summary>
/// Stateless helpers for tile coordinate math: flat-index conversion, range checks, neighbor
/// and rectangle iteration. Sits below <see cref="GridState"/> so storage code and consumer
/// code share one set of conventions (Tech Roadmap §4.4).
/// </summary>
/// <remarks>
/// Square grids only — <c>SizeInTiles</c> is one dimension applied to both axes. The world
/// (X = east, Z = north) maps to tile (x, y) with y = world Z (per CoordinateConventions).
/// </remarks>
public static class GridService
{
    /// <summary>Flat index for a tile in a row-major SoA array. Caller pre-validates with <see cref="ContainsTile"/>.</summary>
    public static int IndexOf(int2 tile, int sizeInTiles)
        => tile.y * sizeInTiles + tile.x;

    /// <summary>Inverse of <see cref="IndexOf"/>. Caller pre-validates the index.</summary>
    public static int2 TileFromIndex(int index, int sizeInTiles)
        => new(index % sizeInTiles, index / sizeInTiles);

    /// <summary>Total tile count — sizeInTiles × sizeInTiles.</summary>
    public static int TileCount(int sizeInTiles) => sizeInTiles * sizeInTiles;

    /// <summary>True if <paramref name="tile"/> is inside [0, sizeInTiles) on both axes.</summary>
    public static bool ContainsTile(int2 tile, int sizeInTiles)
        => tile.x >= 0 && tile.y >= 0 && tile.x < sizeInTiles && tile.y < sizeInTiles;

    /// <summary>Throw <see cref="ArgumentOutOfRangeException"/> if the tile is outside the grid.</summary>
    public static void RequireInRange(int2 tile, int sizeInTiles, string paramName = "tile")
    {
        if (!ContainsTile(tile, sizeInTiles))
        {
            throw new ArgumentOutOfRangeException(paramName,
                $"Tile {tile} outside [0, {sizeInTiles}) on both axes.");
        }
    }

    /// <summary>Yield the four orthogonal neighbors of <paramref name="tile"/>, skipping ones outside the grid.</summary>
    public static IEnumerable<int2> Neighbors4(int2 tile, int sizeInTiles)
    {
        var east = tile + new int2(1, 0);
        var west = tile + new int2(-1, 0);
        var north = tile + new int2(0, 1);
        var south = tile + new int2(0, -1);
        if (ContainsTile(east, sizeInTiles)) yield return east;
        if (ContainsTile(west, sizeInTiles)) yield return west;
        if (ContainsTile(north, sizeInTiles)) yield return north;
        if (ContainsTile(south, sizeInTiles)) yield return south;
    }

    /// <summary>Yield the eight neighbors (orthogonal + diagonal) of <paramref name="tile"/>, skipping ones outside the grid.</summary>
    public static IEnumerable<int2> Neighbors8(int2 tile, int sizeInTiles)
    {
        for (int dy = -1; dy <= 1; dy++)
        for (int dx = -1; dx <= 1; dx++)
        {
            if (dx == 0 && dy == 0) continue;
            var n = tile + new int2(dx, dy);
            if (ContainsTile(n, sizeInTiles)) yield return n;
        }
    }

    /// <summary>
    /// Yield every tile in the inclusive rectangle [<paramref name="min"/>, <paramref name="max"/>],
    /// clipped to the grid bounds.
    /// </summary>
    public static IEnumerable<int2> IterateRect(int2 min, int2 max, int sizeInTiles)
    {
        var x0 = math.max(0, math.min(min.x, max.x));
        var y0 = math.max(0, math.min(min.y, max.y));
        var x1 = math.min(sizeInTiles - 1, math.max(min.x, max.x));
        var y1 = math.min(sizeInTiles - 1, math.max(min.y, max.y));
        for (int y = y0; y <= y1; y++)
        for (int x = x0; x <= x1; x++)
        {
            yield return new int2(x, y);
        }
    }
}
