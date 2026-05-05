#nullable enable

using CityRise.Core;
using NUnit.Framework;
using Unity.Mathematics;

namespace CityRise.Tests.EditMode;

public sealed class CoordinateConventionsTests
{
    [Test]
    public void Constants_MatchGameConstants()
    {
        Assert.That(CoordinateConventions.TileSizeMeters, Is.EqualTo(GameConstants.TileSizeMeters));
        Assert.That(CoordinateConventions.HeightmapVertexSpacingMeters, Is.EqualTo(GameConstants.HeightmapVertexSpacingMeters));
        Assert.That(CoordinateConventions.HeightmapVerticesPerTileSpan, Is.EqualTo(2),
            "Tile is 8 m, vertex spacing is 4 m → 2 intervals per tile edge.");
    }

    [Test]
    public void WorldOrigin_IsTileZeroZero()
    {
        Assert.That(CoordinateConventions.WorldToTile(new float3(0f, 0f, 0f)),
            Is.EqualTo(new int2(0, 0)));
    }

    [Test]
    public void WorldToTile_FloorsTowardNegativeInfinity()
    {
        // Just below zero on X should be tile -1 on X.
        Assert.That(CoordinateConventions.WorldToTile(new float3(-0.001f, 0f, 0f)).x, Is.EqualTo(-1));
        Assert.That(CoordinateConventions.WorldToTile(new float3(-8f, 0f, -8f)),
            Is.EqualTo(new int2(-1, -1)));
        Assert.That(CoordinateConventions.WorldToTile(new float3(-8.001f, 0f, -8.001f)),
            Is.EqualTo(new int2(-2, -2)));
    }

    [Test]
    public void TileSpan_IsHalfOpenInterval()
    {
        // [0, 8) on tile 0; [8, 16) on tile 1.
        Assert.That(CoordinateConventions.WorldToTile(new float3(7.999f, 0f, 7.999f)),
            Is.EqualTo(new int2(0, 0)));
        Assert.That(CoordinateConventions.WorldToTile(new float3(8f, 0f, 8f)),
            Is.EqualTo(new int2(1, 1)));
    }

    [Test]
    public void TileToWorldCenter_IsTileMidpoint()
    {
        var center = CoordinateConventions.TileToWorldCenter(new int2(0, 0));
        Assert.That(center.x, Is.EqualTo(4f));
        Assert.That(center.z, Is.EqualTo(4f));
        Assert.That(center.y, Is.EqualTo(0f));
    }

    [Test]
    public void TileToWorldCorner_IsSouthWestCorner()
    {
        var corner = CoordinateConventions.TileToWorldCorner(new int2(3, 5));
        Assert.That(corner.x, Is.EqualTo(24f));
        Assert.That(corner.z, Is.EqualTo(40f));
    }

    [Test]
    public void WorldToTile_TileToWorldCenter_RoundTrips()
    {
        for (int tx = -10; tx <= 10; tx++)
        for (int ty = -10; ty <= 10; ty++)
        {
            var t = new int2(tx, ty);
            var center = CoordinateConventions.TileToWorldCenter(t);
            Assert.That(CoordinateConventions.WorldToTile(center), Is.EqualTo(t),
                $"Tile {t} center {center} did not round-trip.");
        }
    }

    [Test]
    public void HeightmapCorners_FormSquare_AlignedToTile()
    {
        var (sw, se, ne, nw) = CoordinateConventions.TileCornerHeightmapIndices(new int2(0, 0));
        Assert.That(sw, Is.EqualTo(new int2(0, 0)));
        Assert.That(se, Is.EqualTo(new int2(2, 0)));
        Assert.That(ne, Is.EqualTo(new int2(2, 2)));
        Assert.That(nw, Is.EqualTo(new int2(0, 2)));

        var (sw1, se1, ne1, nw1) = CoordinateConventions.TileCornerHeightmapIndices(new int2(1, 1));
        Assert.That(sw1, Is.EqualTo(new int2(2, 2)));
        Assert.That(se1, Is.EqualTo(new int2(4, 2)));
        Assert.That(ne1, Is.EqualTo(new int2(4, 4)));
        Assert.That(nw1, Is.EqualTo(new int2(2, 4)));
    }

    [Test]
    public void HeightmapCorners_AreSharedBetweenAdjacentTiles()
    {
        // Tile (0,0)'s east corners must equal tile (1,0)'s west corners.
        var (_, se00, ne00, _) = CoordinateConventions.TileCornerHeightmapIndices(new int2(0, 0));
        var (sw10, _, _, nw10) = CoordinateConventions.TileCornerHeightmapIndices(new int2(1, 0));
        Assert.That(sw10, Is.EqualTo(se00));
        Assert.That(nw10, Is.EqualTo(ne00));
    }
}
