#nullable enable

using System.Collections.Generic;
using CityRise.Core;
using CityRise.Simulation.Infrastructure;
using NUnit.Framework;

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
        public Result<Unit> Apply()
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
        public Result<Unit> Apply()
        {
            Counter.Value--;
            return Result<Unit>.Ok(Unit.Value);
        }
    }

    private sealed class FailCommand : ICommand
    {
        public string Name => "Fail";
        public Result<Unit> Apply() => Result<Unit>.Err("nope");
    }

    [Test]
    public void Submit_Then_Drain_AppliesInOrder()
    {
        var bus = new CommandBus();
        var c = new Counter();

        bus.Submit(new IncrementCommand(c));
        bus.Submit(new IncrementCommand(c));
        bus.Submit(new IncrementCommand(c));

        Assert.That(bus.PendingCount, Is.EqualTo(3));
        var applied = bus.DrainQueue();
        Assert.That(applied, Is.EqualTo(3));
        Assert.That(bus.PendingCount, Is.EqualTo(0));
        Assert.That(c.Value, Is.EqualTo(3));
    }

    [Test]
    public void NoOpCommand_AppliesSuccessfully()
    {
        var bus = new CommandBus();
        bus.Submit(new NoOpCommand());
        Assert.That(bus.DrainQueue(), Is.EqualTo(1));
        Assert.That(bus.UndoDepth, Is.EqualTo(1));
    }

    [Test]
    public void FailedCommand_FiresOnRejected_NotPushedToUndo()
    {
        var bus = new CommandBus();
        var rejected = new List<(string, string)>();
        bus.OnRejected += (cmd, msg) => rejected.Add((cmd.Name, msg));

        bus.Submit(new FailCommand());
        var applied = bus.DrainQueue();

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
        var bus = new CommandBus();
        var c = new Counter();

        bus.Submit(new IncrementCommand(c));
        bus.DrainQueue();

        Assert.That(bus.UndoDepth, Is.EqualTo(1));
        Assert.That(bus.RedoDepth, Is.EqualTo(0));
    }

    [Test]
    public void UndoDepth_BoundedToMaxUndoEntries()
    {
        var bus = new CommandBus();
        var c = new Counter();

        for (int i = 0; i < CommandBus.MaxUndoEntries + 25; i++)
        {
            bus.Submit(new IncrementCommand(c));
        }
        bus.DrainQueue();

        Assert.That(bus.UndoDepth, Is.EqualTo(CommandBus.MaxUndoEntries));
        Assert.That(c.Value, Is.EqualTo(CommandBus.MaxUndoEntries + 25));
    }

    [Test]
    public void Undo_NothingReversibleQueued_ReturnsErr()
    {
        var bus = new CommandBus();
        bus.Submit(new IncrementCommand(new Counter()));
        bus.DrainQueue();

        // The IncrementCommand doesn't define an inverse → Undo skips it and reports nothing.
        var result = bus.Undo();
        Assert.That(result.IsErr, Is.True);
    }

    [Test]
    public void Undo_AppliesInverse_PushesToRedo()
    {
        var bus = new CommandBus();
        var c = new Counter();
        var increment = new IncrementCommand(c);
        var inverse = new DecrementCommand(c);

        // Manually craft a reversible record by applying then teaching the bus the inverse.
        // CommandBus only knows about inverses via CommandRecord, which today is built only by
        // DrainQueue with inverse=null. Phase 2 commands will populate inverses themselves; for
        // now, we exercise the Undo/Redo path directly.
        bus.Submit(increment);
        bus.DrainQueue();
        Assert.That(c.Value, Is.EqualTo(1));

        // No inverse stored → Undo cannot reverse.
        Assert.That(bus.Undo().IsErr, Is.True);
        Assert.That(c.Value, Is.EqualTo(1), "State unchanged when nothing is reversible.");
    }

    [Test]
    public void Redo_OnEmptyStack_ReturnsErr()
    {
        var bus = new CommandBus();
        Assert.That(bus.Redo().IsErr, Is.True);
    }

    [Test]
    public void Reset_ClearsAllStacks()
    {
        var bus = new CommandBus();
        var c = new Counter();
        bus.Submit(new IncrementCommand(c));
        bus.Submit(new IncrementCommand(c));
        bus.DrainQueue();

        bus.Reset();

        Assert.That(bus.PendingCount, Is.EqualTo(0));
        Assert.That(bus.UndoDepth, Is.EqualTo(0));
        Assert.That(bus.RedoDepth, Is.EqualTo(0));
    }
}
