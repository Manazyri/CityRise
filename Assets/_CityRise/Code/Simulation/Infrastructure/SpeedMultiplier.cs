#nullable enable

namespace CityRise.Simulation.Infrastructure;

/// <summary>
/// Discrete speed levels driven by the time-control UI. Pause halts ticks; 1× is real-time;
/// 2× and 3× scale the wall-clock delta so sim time advances faster (Tech Roadmap section 8.3).
/// Sim time itself is authoritative for inside-tick logic — this multiplier only governs how
/// many ticks a real frame produces.
/// </summary>
public enum SpeedMultiplier
{
    Paused = 0,
    Normal = 1,
    Fast = 2,
    Faster = 3,
}
