#nullable enable

using CityRise.Simulation.Infrastructure;

namespace CityRise.Persistence
{
    /// <summary>
    /// ISaveable wrapper around <see cref="TickScheduler"/> for time-control state. Plain
    /// C# class (not a MonoBehaviour) since the scheduler is created and lives in Bootstrap,
    /// not on a scene GameObject.
    /// </summary>
    /// <remarks>
    /// Schema v1 stores only the <see cref="SpeedMultiplier"/>. Tick counters are
    /// re-derived from sim time by the scheduler when restored; we don't persist them yet.
    /// Phase 2+ may extend with sim time, growth/budget counters as they become meaningful
    /// for replay.
    /// </remarks>
    public sealed class TimeControlSaveState : ISaveable
    {
        private readonly TickScheduler _scheduler;

        public TimeControlSaveState(TickScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public string SubsystemId => "TimeControl";
        public int CurrentSchemaVersion => 1;

        public SaveBlob Serialize()
        {
            var blob = new SaveBlob();
            blob.Write("speed", (int)_scheduler.Speed);
            return blob;
        }

        public void Deserialize(SaveBlob blob, int fromVersion)
        {
            var raw = blob.ReadInt32("speed");
            _scheduler.Speed = (SpeedMultiplier)raw;
        }
    }
}
