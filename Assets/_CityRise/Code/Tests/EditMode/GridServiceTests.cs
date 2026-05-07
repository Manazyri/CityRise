#nullable enable

using System.Collections.Generic;
using System.Linq;
using CityRise.Simulation.World;
using NUnit.Framework;
using Unity.Mathematics;

namespace CityRise.Tests.EditMode;

public sealed class GridServiceTests
{
    [Test]
    public void IndexOf_RowMajor()
    {
        Assert.That(GridService.IndexOf(new int2(0, 0), 4), Is.EqualTo(0));
        Assert.That(GridService.IndexOf(new int2(3, 0), 4), Is.EqualTo(3));
        Assert.That(GridService.IndexOf(new int2(0, 1), 4), Is.EqualTo(4));
        Assert.That(GridService.IndexOf(new int2(2, 3), 4), Is.EqualTo(14));
    }

    [Test]
    public void IndexOf_TileFromIndex_RoundTrip()
    {
        const int size = 17;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            var tile = new int2(x, y);
            var index = GridService.IndexOf(tile, size);
            Assert.That(GridService.TileFromIndex(index, size), Is.EqualTo(tile));
        }
    }

    [Test]
    public void TileCount_IsSquareOfSize()
    {
        Assert.That(GridService.TileCount(1), Is.EqualTo(1));
        Assert.That(GridService.TileCount(8), Is.EqualTo(64));
        Assert.That(GridService.TileCount(256), Is.EqualTo(65_536));
    }

    [Test]
    public void ContainsTile_BoundaryCheck()
    {
        Assert.That(GridService.ContainsTile(new int2(0, 0), 4), Is.True);
        Assert.That(GridService.ContainsTile(new int2(3, 3), 4), Is.True);
        Assert.That(GridService.ContainsTile(new int2(4, 0), 4), Is.False);
        Assert.That(GridService.ContainsTile(new int2(0, 4), 4), Is.False);
        Assert.That(GridService.ContainsTile(new int2(-1, 0), 4), Is.False);
        Assert.That(GridService.ContainsTile(new int2(0, -1), 4), Is.False);
    }

    [Test]
    public void RequireInRange_ThrowsForOutsideTile()
    {
        Assert.That(() => GridService.RequireInRange(new int2(-1, 0), 4),
            Throws.TypeOf<System.ArgumentOutOfRangeException>());
        Assert.That(() => GridService.RequireInRange(new int2(4, 0), 4),
            Throws.TypeOf<System.ArgumentOutOfRangeException>());
        Assert.DoesNotThrow(() => GridService.RequireInRange(new int2(0, 0), 4));
    }

    [Test]
    public void Neighbors4_InteriorTile_ReturnsFour()
    {
        var n = GridService.Neighbors4(new int2(2, 2), 5).ToList();
        Assert.That(n, Has.Count.EqualTo(4));
        Assert.That(n, Has.Member(new int2(3, 2))); // east
        Assert.That(n, Has.Member(new int2(1, 2))); // west
        Assert.That(n, Has.Member(new int2(2, 3))); // north
        Assert.That(n, Has.Member(new int2(2, 1))); // south
    }

    [Test]
    public void Neighbors4_CornerTile_OmitsOutsides()
    {
        var n = GridService.Neighbors4(new int2(0, 0), 5).ToList();
        Assert.That(n, Has.Count.EqualTo(2));
        Assert.That(n, Has.Member(new int2(1, 0)));
        Assert.That(n, Has.Member(new int2(0, 1)));
    }

    [Test]
    public void Neighbors8_InteriorTile_ReturnsEight()
    {
        var n = GridService.Neighbors8(new int2(2, 2), 5).ToList();
        Assert.That(n, Has.Count.EqualTo(8));
    }

    [Test]
    public void Neighbors8_CornerTile_ReturnsThree()
    {
        var n = GridService.Neighbors8(new int2(0, 0), 5).ToList();
        Assert.That(n, Has.Count.EqualTo(3));
        Assert.That(n, Has.Member(new int2(1, 0)));
        Assert.That(n, Has.Member(new int2(0, 1)));
        Assert.That(n, Has.Member(new int2(1, 1)));
    }

    [Test]
    public void IterateRect_InsideGrid_VisitsEveryTile()
    {
        var rect = GridService.IterateRect(new int2(1, 1), new int2(3, 2), 5).ToList();
        // x in [1..3], y in [1..2] → 3*2 = 6 tiles
        Assert.That(rect, Has.Count.EqualTo(6));
        Assert.That(rect, Is.EquivalentTo(new[]
        {
            new int2(1,1), new int2(2,1), new int2(3,1),
            new int2(1,2), new int2(2,2), new int2(3,2),
        }));
    }

    [Test]
    public void IterateRect_ClipsToGrid()
    {
        // Request larger than grid; expect clip to [0..4]^2 = 25 tiles in a 5×5 grid.
        var rect = GridService.IterateRect(new int2(-2, -2), new int2(10, 10), 5).ToList();
        Assert.That(rect, Has.Count.EqualTo(25));
    }

    [Test]
    public void IterateRect_AcceptsReversedMinMax()
    {
        var rect = GridService.IterateRect(new int2(3, 2), new int2(1, 1), 5).ToList();
        Assert.That(rect, Has.Count.EqualTo(6));
    }
}
