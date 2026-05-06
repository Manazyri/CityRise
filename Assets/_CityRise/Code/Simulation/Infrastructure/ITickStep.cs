#nullable enable

using CityRise.Core;

namespace CityRise.Simulation.Infrastructure;

/// <summary>
/// One step in a tick pipeline. The pipeline is a composable list registered with
/// <see cref="TickScheduler"/> at Bootstrap (Tech Roadmap section 4.5). Reordering steps,
/// inserting a debug probe, or A/B-testing a System is a Bootstrap change, not a scheduler change.
/// </summary>
/// <remarks>
/// Phase 1: <see cref="Run"/> takes only a <see cref="TickContext"/>. Phase 2 will add an
/// <c>IWorldMutate</c> parameter once WorldState lands. Failure returns <c>Result.Err</c>;
/// the scheduler logs and continues so a single broken step doesn't halt the whole tick.
/// </remarks>
public interface ITickStep
{
    /// <summary>Stable identifier used by <c>TickMetrics</c> and the debug overlay.</summary>
    string Name { get; }

    /// <summary>Soft CPU budget in milliseconds at the 500-pop target (per docs/perf-budget.md).</summary>
    float BudgetMs { get; }

    Result<Unit> Run(in TickContext context);
}
