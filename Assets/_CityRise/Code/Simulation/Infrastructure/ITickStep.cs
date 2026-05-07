#nullable enable

using CityRise.Core;
using CityRise.Simulation.World;

namespace CityRise.Simulation.Infrastructure;

/// <summary>
/// One step in a tick pipeline. The pipeline is a composable list registered with
/// <see cref="TickScheduler"/> at Bootstrap (Tech Roadmap §4.5). Reordering steps,
/// inserting a debug probe, or A/B-testing a System is a Bootstrap change, not a scheduler change.
/// </summary>
/// <remarks>
/// <see cref="Run"/> receives an <see cref="IWorldMutate"/> that goes out of scope when the
/// step returns — non-System code never holds a mutating reference (ADR-0007). Failure returns
/// <see cref="Result{Unit}"/> Err; the scheduler logs and continues so a single broken step
/// doesn't halt the whole tick.
/// </remarks>
public interface ITickStep
{
    /// <summary>Stable identifier used by <c>TickMetrics</c> and the debug overlay.</summary>
    string Name { get; }

    /// <summary>Soft CPU budget in milliseconds at the 500-pop target (per docs/perf-budget.md).</summary>
    float BudgetMs { get; }

    Result<Unit> Run(IWorldMutate world, in TickContext context);
}
