#nullable enable

using CityRise.Simulation.World;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace CityRise.Tests.EditMode;

public sealed class GridStateTests
{
    [Test]
    public void Constructor_RejectsNonPositiveSize()
    {
        Assert.That(() => new GridState(0), Throws.TypeOf<System.ArgumentOutOfRangeException>());
        Assert.That(() => new GridState(-4), Throws.TypeOf<System.ArgumentOutOfRangeException>());
    }

    [Test]
    public void DefaultsAreZeroOrSensible()
    {
        using var grid = new GridState(4, Allocator.Temp);
        for (int y = 0; y < grid.SizeInTiles; y++)
        for (int x = 0; x < grid.SizeInTiles; x++)
        {
            var t = new int2(x, y);
            Assert.That(grid.GetElevation(t), Is.EqualTo(0f));
            Assert.That(grid.GetTerrainType(t), Is.EqualTo(TerrainType.Grass));   // 0 = Grass
            Assert.That(grid.GetZoneType(t), Is.EqualTo(ZoneType.None));          // 0 = None
            Assert.That(grid.GetDensityCap(t), Is.EqualTo(0));
            Assert.That(grid.GetDesirability(t), Is.EqualTo(0f));
            Assert.That(grid.GetPollution(t), Is.EqualTo(0f));
            Assert.That(grid.GetPowerCoverage(t), Is.False);
            Assert.That(grid.GetWaterCoverage(t), Is.False);
        }
    }

    [Test]
    public void SetThenGet_RoundTripsEveryField()
    {
        using var grid = new GridState(4, Allocator.Temp);
        var t = new int2(2, 3);

        grid.SetElevation(t, 12.5f);
        grid.SetTerrainType(t, TerrainType.Sand);
        grid.SetZoneType(t, ZoneType.IndustrialLow);
        grid.SetDensityCap(t, 7);
        grid.SetDesirability(t, 0.42f);
        grid.SetPollution(t, 1.7f);
        grid.SetPowerCoverage(t, true);
        grid.SetWaterCoverage(t, true);

        Assert.That(grid.GetElevation(t), Is.EqualTo(12.5f));
        Assert.That(grid.GetTerrainType(t), Is.EqualTo(TerrainType.Sand));
        Assert.That(grid.GetZoneType(t), Is.EqualTo(ZoneType.IndustrialLow));
        Assert.That(grid.GetDensityCap(t), Is.EqualTo(7));
        Assert.That(grid.GetDesirability(t), Is.EqualTo(0.42f));
        Assert.That(grid.GetPollution(t), Is.EqualTo(1.7f));
        Assert.That(grid.GetPowerCoverage(t), Is.True);
        Assert.That(grid.GetWaterCoverage(t), Is.True);
    }

    [Test]
    public void TilesAreIndependent()
    {
        using var grid = new GridState(4, Allocator.Temp);
        var a = new int2(0, 0);
        var b = new int2(3, 3);

        grid.SetElevation(a, 100f);
        grid.SetZoneType(b, ZoneType.ResidentialLow);

        Assert.That(grid.GetElevation(b), Is.EqualTo(0f));
        Assert.That(grid.GetZoneType(a), Is.EqualTo(ZoneType.None));
    }

    [Test]
    public void OutOfRangeAccess_Throws()
    {
        using var grid = new GridState(4, Allocator.Temp);
        Assert.That(() => grid.GetElevation(new int2(-1, 0)), Throws.TypeOf<System.ArgumentOutOfRangeException>());
        Assert.That(() => grid.GetElevation(new int2(4, 0)), Throws.TypeOf<System.ArgumentOutOfRangeException>());
        Assert.That(() => grid.SetElevation(new int2(0, 4), 1f), Throws.TypeOf<System.ArgumentOutOfRangeException>());
    }

    [Test]
    public void Dispose_IsIdempotent()
    {
        var grid = new GridState(4, Allocator.Temp);
        grid.Dispose();
        Assert.DoesNotThrow(() => grid.Dispose()); // second call is a no-op
    }
}
