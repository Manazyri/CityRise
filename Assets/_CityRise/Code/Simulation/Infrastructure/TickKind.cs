#nullable enable

namespace CityRise.Simulation.Infrastructure;

/// <summary>
/// Which clock fires a given tick. Step pipelines are registered per-kind on
/// <see cref="TickScheduler"/> so reordering the sim pipeline doesn't touch growth or budget.
/// </summary>
public enum TickKind
{
    Sim,
    Growth,
    Budget,
}
