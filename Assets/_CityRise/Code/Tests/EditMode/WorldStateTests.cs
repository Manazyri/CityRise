#nullable enable

using CityRise.Simulation.World;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace CityRise.Tests.EditMode;

public sealed class WorldStateTests
{
    [Test]
    public void IWorldRead_ExposesGridReadOnly()
    {
        using var world = new WorldState(4, Allocator.Temp);
        IWorldRead read = world;

        var grid = read.Grid;
        Assert.That(grid, Is.Not.Null);
        Assert.That(grid.SizeInTiles, Is.EqualTo(4));

        // The IGridRead interface has only getters — confirmed at compile time. This test
        // documents the surface so a future regression that adds a setter to IGridRead would
        // be visible.
        Assert.That(grid is IGridMutate, Is.True,
            "GridState happens to implement IGridMutate too; the type-erasure to IGridRead is what enforces read-only access.");
    }

    [Test]
    public void IWorldMutate_GridShadowsBaseInterfaceWithMutateView()
    {
        using var world = new WorldState(4, Allocator.Temp);
        IWorldMutate mutate = world;

        // Through IWorldMutate.Grid we get an IGridMutate; through IWorldRead.Grid we'd get
        // an IGridRead. Both refer to the same instance.
        var asMutate = mutate.Grid;
        var asRead = ((IWorldRead)world).Grid;

        Assert.That(asMutate, Is.SameAs(asRead));

        asMutate.SetElevation(new int2(1, 1), 9f);
        Assert.That(asRead.GetElevation(new int2(1, 1)), Is.EqualTo(9f));
    }

    [Test]
    public void Dispose_ReleasesGrid()
    {
        var world = new WorldState(4, Allocator.Temp);
        world.Dispose();
        Assert.DoesNotThrow(() => world.Dispose()); // idempotent
    }

    [Test]
    public void CreateDefault_UsesGameConstantsSize()
    {
        using var world = WorldState.CreateDefault(Allocator.Temp);
        Assert.That(((IWorldRead)world).Grid.SizeInTiles, Is.EqualTo(CityRise.Core.GameConstants.DefaultMapSizeTiles));
    }
}
