#nullable enable

using System.IO;
using CityRise.Persistence;
using CityRise.Simulation.Infrastructure;
using NUnit.Framework;

namespace CityRise.Tests.EditMode;

/// <summary>
/// Phase 1 close acceptance test (Tech Roadmap §6.3): save/load round-trips camera + time
/// speed. Camera state is normally a MonoBehaviour reading a Transform; for the headless
/// EditMode test we use a stub ISaveable that round-trips the same shape (position + euler
/// rotation) without needing a scene.
/// </summary>
public sealed class Phase1RoundTripTests
{
    private sealed class StubCameraSaveState : ISaveable
    {
        public string SubsystemId => "Camera";
        public int CurrentSchemaVersion => 1;
        public float PosX, PosY, PosZ;
        public float RotX, RotY, RotZ;

        public SaveBlob Serialize()
        {
            var b = new SaveBlob();
            b.Write("posX", PosX); b.Write("posY", PosY); b.Write("posZ", PosZ);
            b.Write("rotX", RotX); b.Write("rotY", RotY); b.Write("rotZ", RotZ);
            return b;
        }

        public void Deserialize(SaveBlob blob, int fromVersion)
        {
            PosX = blob.ReadFloat("posX"); PosY = blob.ReadFloat("posY"); PosZ = blob.ReadFloat("posZ");
            RotX = blob.ReadFloat("rotX"); RotY = blob.ReadFloat("rotY"); RotZ = blob.ReadFloat("rotZ");
        }
    }

    [Test]
    public void Save_Then_Load_RoundTrips_TickSpeed_AndCameraState()
    {
        var path = Path.Combine(Path.GetTempPath(), $"cityrise-p1end-{System.Guid.NewGuid():N}.json");
        try
        {
            var manifest = new SaveManifest();
            var migrations = new MigrationRegistry();

            var scheduler = new TickScheduler { Speed = SpeedMultiplier.Fast };
            var timeState = new TimeControlSaveState(scheduler);
            var camera = new StubCameraSaveState
            {
                PosX = 100f, PosY = 50f, PosZ = 100f,
                RotX = 35f, RotY = 0f, RotZ = 0f,
            };

            manifest.Register(timeState);
            manifest.Register(camera);

            var svc = new SaveService(manifest, migrations);
            var saveResult = svc.Save(path);
            Assert.That(saveResult.IsOk, Is.True, saveResult.IsErr ? saveResult.Error : null);

            // Mutate state to known-bad values, then load and check restoration.
            scheduler.Speed = SpeedMultiplier.Paused;
            camera.PosX = -999f; camera.PosY = -999f; camera.PosZ = -999f;
            camera.RotX = 999f; camera.RotY = 999f; camera.RotZ = 999f;

            var loadResult = svc.Load(path);
            Assert.That(loadResult.IsOk, Is.True, loadResult.IsErr ? loadResult.Error : null);

            Assert.That(scheduler.Speed, Is.EqualTo(SpeedMultiplier.Fast));
            Assert.That(camera.PosX, Is.EqualTo(100f));
            Assert.That(camera.PosY, Is.EqualTo(50f));
            Assert.That(camera.PosZ, Is.EqualTo(100f));
            Assert.That(camera.RotX, Is.EqualTo(35f));
            Assert.That(camera.RotY, Is.EqualTo(0f));
            Assert.That(camera.RotZ, Is.EqualTo(0f));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Test]
    public void TimeControlSaveState_RoundTripsAllFourSpeeds()
    {
        foreach (var speed in new[] { SpeedMultiplier.Paused, SpeedMultiplier.Normal, SpeedMultiplier.Fast, SpeedMultiplier.Faster })
        {
            var scheduler = new TickScheduler { Speed = speed };
            var state = new TimeControlSaveState(scheduler);
            var blob = state.Serialize();

            scheduler.Speed = SpeedMultiplier.Normal; // mutate
            state.Deserialize(blob, 1);

            Assert.That(scheduler.Speed, Is.EqualTo(speed), $"Round-trip failed for {speed}.");
        }
    }
}
