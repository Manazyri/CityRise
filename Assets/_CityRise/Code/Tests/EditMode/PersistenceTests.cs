#nullable enable

using System.Collections.Generic;
using System.IO;
using CityRise.Core;
using CityRise.Persistence;
using NUnit.Framework;

namespace CityRise.Tests.EditMode;

public sealed class SaveBlobTests
{
    [Test]
    public void Write_Then_Read_RoundTrips_TypedValues()
    {
        var b = new SaveBlob();
        b.Write("a", 42);
        b.Write("b", -7L);
        b.Write("c", 3.14f);
        b.Write("d", "hello");
        b.Write("e", true);
        b.Write("f", 999u);
        b.Write("g", 12345UL);
        b.Write("h", 2.71828);

        Assert.That(b.ReadInt32("a"), Is.EqualTo(42));
        Assert.That(b.ReadInt64("b"), Is.EqualTo(-7L));
        Assert.That(b.ReadFloat("c"), Is.EqualTo(3.14f));
        Assert.That(b.ReadString("d"), Is.EqualTo("hello"));
        Assert.That(b.ReadBool("e"), Is.True);
        Assert.That(b.ReadUInt32("f"), Is.EqualTo(999u));
        Assert.That(b.ReadUInt64("g"), Is.EqualTo(12345UL));
        Assert.That(b.ReadDouble("h"), Is.EqualTo(2.71828));
    }

    [Test]
    public void NestedBlob_RoundTrips()
    {
        var inner = new SaveBlob();
        inner.Write("x", 1);

        var outer = new SaveBlob();
        outer.Write("nested", inner);

        var read = outer.ReadBlob("nested");
        Assert.That(read.ReadInt32("x"), Is.EqualTo(1));
    }

    [Test]
    public void ArrayOfBlobs_RoundTrips()
    {
        var a = new SaveBlob(); a.Write("v", 10);
        var b = new SaveBlob(); b.Write("v", 20);
        var c = new SaveBlob(); c.Write("v", 30);

        var outer = new SaveBlob();
        outer.WriteArray("items", new[] { a, b, c });

        var read = outer.ReadArray("items");
        Assert.That(read.Count, Is.EqualTo(3));
        Assert.That(read[0].ReadInt32("v"), Is.EqualTo(10));
        Assert.That(read[1].ReadInt32("v"), Is.EqualTo(20));
        Assert.That(read[2].ReadInt32("v"), Is.EqualTo(30));
    }

    [Test]
    public void Read_MissingKey_Throws()
    {
        var b = new SaveBlob();
        Assert.That(() => b.ReadInt32("nope"), Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void Read_WrongType_Throws()
    {
        var b = new SaveBlob();
        b.Write("x", "not a number");
        Assert.That(() => b.ReadInt32("x"), Throws.TypeOf<System.InvalidCastException>());
    }

    [Test]
    public void Write_EmptyKey_Throws()
    {
        var b = new SaveBlob();
        Assert.That(() => b.Write("", 1), Throws.ArgumentException);
    }

    [Test]
    public void TryReadInt32_Hit_And_Miss()
    {
        var b = new SaveBlob();
        b.Write("a", 5);

        Assert.That(b.TryReadInt32("a", out var hit), Is.True);
        Assert.That(hit, Is.EqualTo(5));

        Assert.That(b.TryReadInt32("missing", out _), Is.False);
    }
}

public sealed class SaveManifestTests
{
    private sealed class StubSaveable : ISaveable
    {
        public string SubsystemId { get; }
        public int CurrentSchemaVersion => 1;
        public StubSaveable(string id) { SubsystemId = id; }
        public SaveBlob Serialize() => new();
        public void Deserialize(SaveBlob blob, int fromVersion) { }
    }

    [Test]
    public void Register_PreservesOrder()
    {
        var m = new SaveManifest();
        m.Register(new StubSaveable("A"));
        m.Register(new StubSaveable("B"));
        m.Register(new StubSaveable("C"));

        Assert.That(m.Count, Is.EqualTo(3));
        Assert.That(m.Ordered[0].SubsystemId, Is.EqualTo("A"));
        Assert.That(m.Ordered[1].SubsystemId, Is.EqualTo("B"));
        Assert.That(m.Ordered[2].SubsystemId, Is.EqualTo("C"));
    }

    [Test]
    public void DuplicateId_Throws()
    {
        var m = new SaveManifest();
        m.Register(new StubSaveable("X"));
        Assert.That(() => m.Register(new StubSaveable("X")), Throws.InvalidOperationException);
    }

    [Test]
    public void Find_HitsAndMisses()
    {
        var m = new SaveManifest();
        var a = new StubSaveable("A");
        m.Register(a);

        Assert.That(m.Find("A"), Is.SameAs(a));
        Assert.That(m.Find("B"), Is.Null);
    }
}

public sealed class MigrationRegistryTests
{
    [Test]
    public void Migrate_NoStepsNeeded_WhenVersionsMatch()
    {
        var r = new MigrationRegistry();
        var blob = new SaveBlob();
        blob.Write("v", 1);

        var result = r.Migrate("S", blob, fromVersion: 3, toVersion: 3);
        Assert.That(result.IsOk, Is.True);
    }

    [Test]
    public void Migrate_AppliesSingleStep()
    {
        var r = new MigrationRegistry();
        r.Register("S", fromVersion: 1, b => b.Write("upgraded", true));

        var blob = new SaveBlob();
        var result = r.Migrate("S", blob, fromVersion: 1, toVersion: 2);
        Assert.That(result.IsOk, Is.True);
        Assert.That(blob.ReadBool("upgraded"), Is.True);
    }

    [Test]
    public void Migrate_AppliesTransitiveSteps()
    {
        var r = new MigrationRegistry();
        r.Register("S", 1, b => b.Write("step1", true));
        r.Register("S", 2, b => b.Write("step2", true));
        r.Register("S", 3, b => b.Write("step3", true));

        var blob = new SaveBlob();
        var result = r.Migrate("S", blob, fromVersion: 1, toVersion: 4);
        Assert.That(result.IsOk, Is.True);
        Assert.That(blob.ReadBool("step1"), Is.True);
        Assert.That(blob.ReadBool("step2"), Is.True);
        Assert.That(blob.ReadBool("step3"), Is.True);
    }

    [Test]
    public void Migrate_MissingStep_ReturnsErr()
    {
        var r = new MigrationRegistry();
        // No registrations.
        var result = r.Migrate("S", new SaveBlob(), fromVersion: 1, toVersion: 2);
        Assert.That(result.IsErr, Is.True);
        StringAssert.Contains("Missing migration", result.Error);
    }

    [Test]
    public void Migrate_DowngradeRejected()
    {
        var r = new MigrationRegistry();
        var result = r.Migrate("S", new SaveBlob(), fromVersion: 5, toVersion: 3);
        Assert.That(result.IsErr, Is.True);
        StringAssert.Contains("downgrade", result.Error);
    }

    [Test]
    public void DuplicateRegistration_Throws()
    {
        var r = new MigrationRegistry();
        r.Register("S", 1, _ => { });
        Assert.That(() => r.Register("S", 1, _ => { }), Throws.InvalidOperationException);
    }
}

public sealed class SaveServiceRoundTripTests
{
    private sealed class CounterState : ISaveable
    {
        public string SubsystemId => "Counter";
        public int CurrentSchemaVersion => 1;
        public int Value;

        public SaveBlob Serialize()
        {
            var b = new SaveBlob();
            b.Write("value", Value);
            return b;
        }

        public void Deserialize(SaveBlob blob, int fromVersion)
        {
            Value = blob.ReadInt32("value");
        }
    }

    [Test]
    public void SaveLoad_RoundTripsState()
    {
        var manifest = new SaveManifest();
        var migrations = new MigrationRegistry();
        var state = new CounterState { Value = 42 };
        manifest.Register(state);

        var path = Path.Combine(Path.GetTempPath(), $"cityrise-test-{System.Guid.NewGuid():N}.json");
        try
        {
            var svc = new SaveService(manifest, migrations);
            var saveResult = svc.Save(path);
            Assert.That(saveResult.IsOk, Is.True, saveResult.IsErr ? saveResult.Error : null);

            // Mutate then load — value should be restored.
            state.Value = 999;
            var loadResult = svc.Load(path);
            Assert.That(loadResult.IsOk, Is.True, loadResult.IsErr ? loadResult.Error : null);
            Assert.That(state.Value, Is.EqualTo(42));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Test]
    public void Load_MissingFile_ReturnsErr()
    {
        var svc = new SaveService(new SaveManifest(), new MigrationRegistry());
        var path = Path.Combine(Path.GetTempPath(), $"cityrise-nope-{System.Guid.NewGuid():N}.json");
        var result = svc.Load(path);
        Assert.That(result.IsErr, Is.True);
    }

    [Test]
    public void Load_BadMagic_ReturnsErr()
    {
        var path = Path.Combine(Path.GetTempPath(), $"cityrise-badmagic-{System.Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(path,
                "{\"header\":{\"magic\":\"NotCityRise\",\"format\":1,\"savedAt\":\"\",\"buildTag\":\"\"},\"entries\":[]}");
            var svc = new SaveService(new SaveManifest(), new MigrationRegistry());
            var result = svc.Load(path);
            Assert.That(result.IsErr, Is.True);
            StringAssert.Contains("magic", result.Error);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Test]
    public void Load_UnknownSubsystem_IsSkipped_NotErrored()
    {
        // Save with a subsystem then load with a manifest that doesn't include it.
        var path = Path.Combine(Path.GetTempPath(), $"cityrise-fwdcompat-{System.Guid.NewGuid():N}.json");
        try
        {
            var writeManifest = new SaveManifest();
            writeManifest.Register(new CounterState { Value = 5 });
            new SaveService(writeManifest, new MigrationRegistry()).Save(path);

            // Reader has no saveables registered.
            var emptyManifest = new SaveManifest();
            var result = new SaveService(emptyManifest, new MigrationRegistry()).Load(path);
            Assert.That(result.IsOk, Is.True);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Test]
    public void Load_OutdatedSchema_RunsMigrations()
    {
        var path = Path.Combine(Path.GetTempPath(), $"cityrise-migration-{System.Guid.NewGuid():N}.json");
        try
        {
            // Write a v1 save.
            var writeManifest = new SaveManifest();
            writeManifest.Register(new CounterState { Value = 7 });
            new SaveService(writeManifest, new MigrationRegistry()).Save(path);

            // Load with a saveable that's now at v2 and a migration that doubles "value".
            var v2State = new VersionedCounter { CurrentVersion = 2 };
            var readManifest = new SaveManifest();
            readManifest.Register(v2State);
            var migrations = new MigrationRegistry();
            migrations.Register("Counter", fromVersion: 1, b => b.Write("value", b.ReadInt32("value") * 2));

            var result = new SaveService(readManifest, migrations).Load(path);
            Assert.That(result.IsOk, Is.True, result.IsErr ? result.Error : null);
            Assert.That(v2State.Value, Is.EqualTo(14));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Test]
    public void Load_OutdatedSchema_NoMigration_ReturnsErr()
    {
        var path = Path.Combine(Path.GetTempPath(), $"cityrise-nomig-{System.Guid.NewGuid():N}.json");
        try
        {
            // Write at v1.
            var writeManifest = new SaveManifest();
            writeManifest.Register(new CounterState { Value = 1 });
            new SaveService(writeManifest, new MigrationRegistry()).Save(path);

            // Load with v2 expectation but no registered migration.
            var v2State = new VersionedCounter { CurrentVersion = 2 };
            var readManifest = new SaveManifest();
            readManifest.Register(v2State);

            var result = new SaveService(readManifest, new MigrationRegistry()).Load(path);
            Assert.That(result.IsErr, Is.True);
            StringAssert.Contains("Missing migration", result.Error);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Test]
    public void Save_AtomicWrite_LeavesNoTempFile_OnSuccess()
    {
        var manifest = new SaveManifest();
        manifest.Register(new CounterState { Value = 1 });
        var path = Path.Combine(Path.GetTempPath(), $"cityrise-atomic-{System.Guid.NewGuid():N}.json");
        try
        {
            new SaveService(manifest, new MigrationRegistry()).Save(path);
            Assert.That(File.Exists(path), Is.True);
            Assert.That(File.Exists(path + ".tmp"), Is.False);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
            if (File.Exists(path + ".tmp")) File.Delete(path + ".tmp");
        }
    }

    private sealed class VersionedCounter : ISaveable
    {
        public string SubsystemId => "Counter";
        public int CurrentVersion = 1;
        public int CurrentSchemaVersion => CurrentVersion;
        public int Value;

        public SaveBlob Serialize()
        {
            var b = new SaveBlob();
            b.Write("value", Value);
            return b;
        }

        public void Deserialize(SaveBlob blob, int fromVersion)
        {
            Value = blob.ReadInt32("value");
        }
    }
}
