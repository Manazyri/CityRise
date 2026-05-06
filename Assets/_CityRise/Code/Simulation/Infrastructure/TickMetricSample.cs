#nullable enable

namespace CityRise.Simulation.Infrastructure;

/// <summary>One step's measurement for one tick. Read by debug overlay and TickMetrics consumers.</summary>
public readonly struct TickMetricSample
{
    public readonly string StepName;
    public readonly float ElapsedMs;
    public readonly float BudgetMs;
    public readonly int ConsecutiveOverBudget;

    public TickMetricSample(string stepName, float elapsedMs, float budgetMs, int consecutiveOverBudget)
    {
        StepName = stepName;
        ElapsedMs = elapsedMs;
        BudgetMs = budgetMs;
        ConsecutiveOverBudget = consecutiveOverBudget;
    }

    public bool OverBudget => ElapsedMs > BudgetMs;
}
