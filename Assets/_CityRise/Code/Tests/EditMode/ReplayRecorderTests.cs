#nullable enable

using System.IO;
using CityRise.Core;
using CityRise.Persistence;
using CityRise.Simulation.Infrastructure;
using CityRise.Simulation.World;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Unity.Collections;

namespace CityRise.Tests.EditMode;

public sealed class ReplayRecorderTests
{
    private sealed class NamedCommand : ICommand
    {
        public string Name { get; }
        public NamedCommand(string name) { Name = name; }
        public Result<Unit> Apply(IWorldMutate world) => Result<Unit>.Ok(Unit.Value);
    }

    private sealed class FailingCommand : ICommand
    {
        public string Name => "Fail";
        public Result<Unit> Apply(IWorldMutate world) => Result<Unit>.Err("nope");
    }

    private static WorldState NewWorld() => new(1, Allocator.Temp);

    [Test]
    public void Constructor_RejectsNonPositiveCapacity()
    {
        Assert.That(() => new ReplayRecorder(0u, capacity: 0), Throws.TypeOf<System.ArgumentOutOfRangeException>());
        Assert.That(() => new ReplayRecorder(0u, capacity: -1), Throws.TypeOf<System.ArgumentOutOfRangeException>());
    }

    [Test]
    public void Record_PopulatesBuffer_InOrder()
    {
        var r = new ReplayRecorder(0u, capacity: 8);
        r.Record(new RecordedCommand(0, "A"));
        r.Record(new RecordedCommand(1, "B"));
        r.Record(new RecordedCommand(2, "C"));

        Assert.That(r.Count, Is.EqualTo(3));
        Assert.That(r.HasWrapped, Is.False);

        var snap = r.Snapshot();
        Assert.That(snap.Count, Is.EqualTo(3));
        Assert.That(snap[0].CommandName, Is.EqualTo("A"));
        Assert.That(snap[1].CommandName, Is.EqualTo("B"));
        Assert.That(snap[2].CommandName, Is.EqualTo("C"));
        Assert.That(snap[0].SimTick, Is.EqualTo(0UL));
        Assert.That(snap[2].SimTick, Is.EqualTo(2UL));
    }

    [Test]
    public void Record_SaturatesAtCapacity_AndOverwritesOldest()
    {
        var r = new ReplayRecorder(0u, capacity: 3);
        r.Record(new RecordedCommand(0, "A"));
        r.Record(new RecordedCommand(1, "B"));
        r.Record(new RecordedCommand(2, "C"));
        Assert.That(r.HasWrapped, Is.False);

        r.Record(new RecordedCommand(3, "D"));
        r.Record(new RecordedCommand(4, "E"));

        Assert.That(r.Count, Is.EqualTo(3));
        Assert.That(r.HasWrapped, Is.True);

        var snap = r.Snapshot();
        Assert.That(snap.Count, Is.EqualTo(3));
        Assert.That(snap[0].CommandName, Is.EqualTo("C"));
        Assert.That(snap[1].CommandName, Is.EqualTo("D"));
        Assert.That(snap[2].CommandName, Is.EqualTo("E"));
    }

    [Test]
    public void Snapshot_OnEmptyRecorder_ReturnsEmptyArray()
    {
        var r = new ReplayRecorder(0u);
        var snap = r.Snapshot();
        Assert.That(snap.Count, Is.EqualTo(0));
    }

    [Test]
    public void Clear_RemovesAllEntries_AndResetsWrap()
    {
        var r = new ReplayRecorder(0u, capacity: 3);
        for (var i = 0; i < 5; i++) r.Record(new RecordedCommand((ulong)i, $"c{i}"));
        Assert.That(r.HasWrapped, Is.True);

        r.Clear();
        Assert.That(r.Count, Is.EqualTo(0));
        Assert.That(r.HasWrapped, Is.False);
        Assert.That(r.Snapshot().Count, Is.EqualTo(0));
    }

    [Test]
    public void Bind_RecordsSuccessfulCommands_SkipsRejected()
    {
        using var world = NewWorld();
        var bus = new CommandBus();
        ulong tick = 5;
        var r = new ReplayRecorder(0u, simTickProvider: () => tick);
        r.Bind(bus);

        bus.Submit(new NamedCommand("X"));
        bus.Submit(new FailingCommand());
        bus.Submit(new NamedCommand("Y"));
        bus.DrainQueue(world);

        var snap = r.Snapshot();
        Assert.That(snap.Count, Is.EqualTo(2));
        Assert.That(snap[0].CommandName, Is.EqualTo("X"));
        Assert.That(snap[1].CommandName, Is.EqualTo("Y"));
        Assert.That(snap[0].SimTick, Is.EqualTo(5UL));
    }

    [Test]
    public void Unbind_StopsRecording()
    {
        using var world = NewWorld();
        var bus = new CommandBus();
        var r = new ReplayRecorder(0u);
        r.Bind(bus);

        bus.Submit(new NamedCommand("A"));
        bus.DrainQueue(world);
        Assert.That(r.Count, Is.EqualTo(1));

        r.Unbind(bus);
        bus.Submit(new NamedCommand("B"));
        bus.DrainQueue(world);
        Assert.That(r.Count, Is.EqualTo(1), "Unbound recorder must not capture further commands.");
    }

    [Test]
    public void Write_EmitsExpectedJsonShape()
    {
        var r = new ReplayRecorder(initialSeed: 42, capacity: 5) { BuildTag = "test-build" };
        r.Record(new RecordedCommand(10, "First"));
        r.Record(new RecordedCommand(20, "Second"));

        using var sw = new StringWriter();
        r.Write(sw);
        var json = JObject.Parse(sw.ToString());

        Assert.That((string?)json["magic"], Is.EqualTo(ReplayRecorder.Magic));
        Assert.That((int?)json["format"], Is.EqualTo(ReplayRecorder.CurrentFormatVersion));
        Assert.That((uint?)json["initialSeed"], Is.EqualTo(42u));
        Assert.That((int?)json["capacity"], Is.EqualTo(5));
        Assert.That((bool?)json["wrapped"], Is.False);
        Assert.That((string?)json["buildTag"], Is.EqualTo("test-build"));

        var commands = (JArray)json["commands"]!;
        Assert.That(commands.Count, Is.EqualTo(2));
        Assert.That((ulong?)commands[0]["simTick"], Is.EqualTo(10UL));
        Assert.That((string?)commands[0]["name"], Is.EqualTo("First"));
        Assert.That((ulong?)commands[1]["simTick"], Is.EqualTo(20UL));
        Assert.That((string?)commands[1]["name"], Is.EqualTo("Second"));
    }

    [Test]
    public void Dump_WritesFile_AndAtomicTempIsCleanedUp()
    {
        var path = Path.Combine(Path.GetTempPath(), $"cityrise-replay-{System.Guid.NewGuid():N}.json");
        try
        {
            var r = new ReplayRecorder(initialSeed: 1u);
            r.Record(new RecordedCommand(0, "Cmd"));

            var result = r.Dump(path);
            Assert.That(result.IsOk, Is.True, result.IsErr ? result.Error : null);
            Assert.That(File.Exists(path), Is.True);
            Assert.That(File.Exists(path + ".tmp"), Is.False);

            // Round-trip parses without throwing.
            var roundTrip = JObject.Parse(File.ReadAllText(path));
            Assert.That((string?)roundTrip["magic"], Is.EqualTo(ReplayRecorder.Magic));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
            if (File.Exists(path + ".tmp")) File.Delete(path + ".tmp");
        }
    }

    [Test]
    public void Dump_EmptyPath_ReturnsErr()
    {
        var r = new ReplayRecorder(0u);
        var result = r.Dump("");
        Assert.That(result.IsErr, Is.True);
    }

    [Test]
    public void Bind_NullBus_Throws()
    {
        var r = new ReplayRecorder(0u);
        Assert.That(() => r.Bind(null!), Throws.ArgumentNullException);
        Assert.That(() => r.Unbind(null!), Throws.ArgumentNullException);
    }
}
