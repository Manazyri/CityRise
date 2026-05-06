#nullable enable

namespace CityRise.Simulation.Infrastructure;

/// <summary>
/// Read-only context handed to each <see cref="ITickStep"/>. Phase 1 carries only the tick
/// counters and kind; Phase 2 will extend the type with an <c>IWorldMutate</c> reference once
/// WorldState lands (Tech Roadmap section 4.5). Step authors should treat the value as
/// frozen for the duration of the step run.
/// </summary>
public readonly struct TickContext
{
    public readonly TickKind Kind;
    public readonly ulong SimTickCount;
    public readonly ulong GrowthTickCount;
    public readonly ulong BudgetTickCount;

    public TickContext(TickKind kind, ulong simTickCount, ulong growthTickCount, ulong budgetTickCount)
    {
        Kind = kind;
        SimTickCount = simTickCount;
        GrowthTickCount = growthTickCount;
        BudgetTickCount = budgetTickCount;
    }
}
