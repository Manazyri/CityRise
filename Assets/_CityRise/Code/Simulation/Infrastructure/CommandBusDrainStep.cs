#nullable enable

using System;
using CityRise.Core;
using CityRise.Simulation.World;

namespace CityRise.Simulation.Infrastructure;

/// <summary>
/// First step of every sim tick. Drains the <see cref="CommandBus"/>'s queued commands
/// against the live <see cref="IWorldMutate"/>, so command application happens inside the
/// scheduler's tick budget and after the scheduler has done its accounting (Tech Roadmap §4.5).
/// </summary>
public sealed class CommandBusDrainStep : ITickStep
{
    private readonly CommandBus _bus;

    public CommandBusDrainStep(CommandBus bus)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
    }

    public string Name => "CommandBus.DrainQueue";

    /// <summary>Per perf-budget.md — sim-tick budget at 500 pop is 0.5 ms.</summary>
    public float BudgetMs => 0.5f;

    public Result<Unit> Run(IWorldMutate world, in TickContext context)
    {
        _bus.DrainQueue(world);
        return Result<Unit>.Ok(Unit.Value);
    }
}
