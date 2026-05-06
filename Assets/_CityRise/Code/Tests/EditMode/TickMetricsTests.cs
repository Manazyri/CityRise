#nullable enable

using System.Collections.Generic;
using CityRise.Simulation.Infrastructure;
using NUnit.Framework;

namespace CityRise.Tests.EditMode;

public sealed class TickMetricsTests
{
    [Test]
    public void Record_TracksLastSample()
    {
        var m = new TickMetrics();
        m.Record("StepA", elapsedMs: 0.5f, budgetMs: 1f);

        var last = m.LastByStep["StepA"];
        Assert.That(last.ElapsedMs, Is.EqualTo(0.5f));
        Assert.That(last.BudgetMs, Is.EqualTo(1f));
        Assert.That(last.OverBudget, Is.False);
        Assert.That(last.ConsecutiveOverBudget, Is.EqualTo(0));
    }

    [Test]
    public void OverBudget_IncrementsConsecutiveCounter()
    {
        var m = new TickMetrics();
        m.Record("Slow", 2f, 1f);
        Assert.That(m.LastByStep["Slow"].ConsecutiveOverBudget, Is.EqualTo(1));

        m.Record("Slow", 2f, 1f);
        Assert.That(m.LastByStep["Slow"].ConsecutiveOverBudget, Is.EqualTo(2));
    }

    [Test]
    public void WithinBudget_ResetsConsecutiveCounter()
    {
        var m = new TickMetrics();
        m.Record("S", 2f, 1f);
        m.Record("S", 2f, 1f);
        Assert.That(m.LastByStep["S"].ConsecutiveOverBudget, Is.EqualTo(2));

        m.Record("S", 0.5f, 1f);
        Assert.That(m.LastByStep["S"].ConsecutiveOverBudget, Is.EqualTo(0));
    }

    [Test]
    public void Warning_FiresExactlyOnce_AtThreshold()
    {
        var m = new TickMetrics();
        var warnings = new List<TickMetricSample>();
        m.OnBudgetWarning += s => warnings.Add(s);

        for (int i = 0; i < TickMetrics.OverBudgetWarningThreshold; i++)
        {
            m.Record("S", 2f, 1f);
        }
        Assert.That(warnings.Count, Is.EqualTo(1));
        Assert.That(warnings[0].StepName, Is.EqualTo("S"));

        // Continued over-budget runs should not re-fire until counter resets.
        m.Record("S", 2f, 1f);
        m.Record("S", 2f, 1f);
        Assert.That(warnings.Count, Is.EqualTo(1));

        // Reset by going under-budget, then go over again.
        m.Record("S", 0.1f, 1f);
        for (int i = 0; i < TickMetrics.OverBudgetWarningThreshold; i++)
        {
            m.Record("S", 2f, 1f);
        }
        Assert.That(warnings.Count, Is.EqualTo(2));
    }

    [Test]
    public void Snapshot_ReturnsAllRecordedSteps()
    {
        var m = new TickMetrics();
        m.Record("A", 0.1f, 1f);
        m.Record("B", 0.2f, 1f);
        m.Record("C", 0.3f, 1f);

        var snap = m.Snapshot();
        Assert.That(snap.Count, Is.EqualTo(3));
    }

    [Test]
    public void Reset_ClearsHistory()
    {
        var m = new TickMetrics();
        m.Record("S", 2f, 1f);
        m.Reset();

        Assert.That(m.LastByStep.Count, Is.EqualTo(0));
        Assert.That(m.Snapshot().Count, Is.EqualTo(0));
    }

    [Test]
    public void Measure_RecordsSampleOnDispose()
    {
        var m = new TickMetrics();
        using (m.Measure("Scoped", budgetMs: 1000f))
        {
            // No-op work.
        }
        Assert.That(m.LastByStep.ContainsKey("Scoped"), Is.True);
        Assert.That(m.LastByStep["Scoped"].BudgetMs, Is.EqualTo(1000f));
    }

    [Test]
    public void Record_EmptyName_Throws()
    {
        var m = new TickMetrics();
        Assert.That(() => m.Record("", 0f, 1f), Throws.ArgumentException);
        Assert.That(() => m.Measure("", 1f), Throws.ArgumentException);
    }
}
