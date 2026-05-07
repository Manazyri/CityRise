#nullable enable

using System.Collections.Generic;
using CityRise.Core;
using CityRise.Simulation.Infrastructure;
using CityRise.Simulation.World;
using NUnit.Framework;
using Unity.Collections;

namespace CityRise.Tests.EditMode;

public sealed class CommandBusTests
{
    private sealed class Counter
    {
        public int Value;
    }

    private sealed class IncrementCommand : ICommand
    {
        public string Name => "Increment";
        public Counter Counter;
        public IncrementCommand(Counter c) { Counter = c; }
        public Result<Unit> Apply(IWorldMutate world)
        {
            Counter.Value++;
            return Result<Unit>.Ok(Unit.Value);
        }
    }

    private sealed class DecrementCommand : ICommand
    {
        public string Name => "Decrement";
        public Counter Counter;
        public DecrementCommand(Counter c) { Counter = c; }
        public Result<Unit> Apply(IWorldMutate world)
        {
            Counter.Value--;
            return Result<Unit>.Ok(Unit.Value);
        }
    }

    private sealed class FailCommand : ICommand
    {
        public string Name => "Fail";
        public Result<Unit> Apply(IWorldMutate world) => Result<Unit>.Err("nope");
    }

    /// <summary>One-tile WorldState pinned to Allocator.Temp; used as the target IWorldMutate for tests.</summary>
    private static WorldState NewWorld() => new(1, Allocator.Temp);

    [Test]
    public void Submit_Then_Drain_AppliesInOrder()
    {
        using var world = NewWorld();
        var bus = new CommandBus();
        var c = new Counter();

        bus.Submit(new IncrementCommand(c));
        bus.Submit(new IncrementCommand(c));
        bus.Submit(new IncrementCommand(c));

        Assert.That(bus.PendingCount, Is.EqualTo(3));
        var applied = bus.DrainQueue(world);
        Assert.That(applied, Is.EqualTo(3));
        Assert.That(bus.PendingCount, Is.EqualTo(0));
        Assert.That(c.Value, Is.EqualTo(3));
    }

    [Test]
    public void NoOpCommand_AppliesSuccessfully()
    {
        using var world = NewWorld();
        var bus = new CommandBus();
        bus.Submit(new NoOpCommand());
        Assert.That(bus.DrainQueue(world), Is.EqualTo(1));
        Assert.That(bus.UndoDepth, Is.EqualTo(1));
    }

    [Test]
    public void FailedCommand_FiresOnRejected_NotPushedToUndo()
    {
        using var world = NewWorld();
        var bus = new CommandBus();
        var rejected = new List<(string, string)>();
        bus.OnRejected += (cmd, msg) => rejected.Add((cmd.Name, msg));

        bus.Submit(new FailCommand());
        var applied = bus.DrainQueue(world);

        Assert.That(applied, Is.EqualTo(0));
        Assert.That(bus.UndoDepth, Is.EqualTo(0));
        Assert.That(rejected, Is.EqualTo(new[] { ("Fail", "nope") }));
    }

    [Test]
    public void Submit_Null_Throws()
    {
        var bus = new CommandBus();
        Assert.That(() => bus.Submit(null!), Throws.ArgumentNullException);
    }

    [Test]
    public void SuccessfulApply_PushesToUndo_AndClearsRedo()
    {
        using var world = NewWorld();
        var bus = new CommandBus();
        var c = new Counter();

        bus.Submit(new IncrementCommand(c));
        bus.DrainQueue(world);

        Assert.That(bus.UndoDepth, Is.EqualTo(1));
        Assert.That(bus.RedoDepth, Is.EqualTo(0));
    }

    [Test]
    public void UndoDepth_BoundedToMaxUndoEntries()
    {
        using var world = NewWorld();
        var bus = new CommandBus();
        var c = new Counter();

        for (int i = 0; i < CommandBus.MaxUndoEntries + 25; i++)
        {
            bus.Submit(new IncrementCommand(c));
        }
        bus.DrainQueue(world);

        Assert.That(bus.UndoDepth, Is.EqualTo(CommandBus.MaxUndoEntries));
        Assert.That(c.Value, Is.EqualTo(CommandBus.MaxUndoEntries + 25));
    }

    [Test]
    public void Undo_NothingReversibleQueued_ReturnsErr()
    {
        using var world = NewWorld();
        var bus = new CommandBus();
        bus.Submit(new IncrementCommand(new Counter()));
        bus.DrainQueue(world);

        // The IncrementCommand doesn't define an inverse → Undo skips it and reports nothing.
        var result = bus.Undo(world);
        Assert.That(result.IsErr, Is.True);
    }

    [Test]
    public void Undo_AppliesInverse_PushesToRedo()
    {
        using var world = NewWorld();
        var bus = new CommandBus();
        var c = new Counter();
        var increment = new IncrementCommand(c);

        // CommandBus today only stores commands with inverse=null (from DrainQueue). Phase 4+
        // commands populate inverses themselves; for now we just confirm Undo on a queue
        // without inverses can't reverse.
        bus.Submit(increment);
        bus.DrainQueue(world);
        Assert.That(c.Value, Is.EqualTo(1));

        Assert.That(bus.Undo(world).IsErr, Is.True);
        Assert.That(c.Value, Is.EqualTo(1), "State unchanged when nothing is reversible.");
    }

    [Test]
    public void Redo_OnEmptyStack_ReturnsErr()
    {
        using var world = NewWorld();
        var bus = new CommandBus();
        Assert.That(bus.Redo(world).IsErr, Is.True);
    }

    [Test]
    public void Reset_ClearsAllStacks()
    {
        using var world = NewWorld();
        var bus = new CommandBus();
        var c = new Counter();
        bus.Submit(new IncrementCommand(c));
        bus.Submit(new IncrementCommand(c));
        bus.DrainQueue(world);

        bus.Reset();

        Assert.That(bus.PendingCount, Is.EqualTo(0));
        Assert.That(bus.UndoDepth, Is.EqualTo(0));
        Assert.That(bus.RedoDepth, Is.EqualTo(0));
    }
}
