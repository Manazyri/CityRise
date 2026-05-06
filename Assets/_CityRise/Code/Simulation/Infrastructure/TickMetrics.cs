#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Profiling;

namespace CityRise.Simulation.Infrastructure;

/// <summary>
/// Records per-step ms timings and warns when a step exceeds its declared budget for
/// <see cref="OverBudgetWarningThreshold"/> consecutive ticks (Tech Roadmap Appendix C,
/// /docs/perf-budget.md). Wraps each measurement in a <c>UnityEngine.Profiling</c> sample
/// so the Profiler window shows tagged frames.
/// </summary>
public sealed class TickMetrics
{
    /// <summary>Per perf-budget.md: TickMetrics warns after this many consecutive over-budget runs.</summary>
    public const int OverBudgetWarningThreshold = 3;

    private readonly Dictionary<string, int> _consecutive = new();
    private readonly Dictionary<string, TickMetricSample> _last = new();
    private readonly List<TickMetricSample> _snapshot = new();

    /// <summary>Fires the first time a step crosses <see cref="OverBudgetWarningThreshold"/> consecutive over-budget runs.</summary>
    public event Action<TickMetricSample>? OnBudgetWarning;

    /// <summary>Last sample captured per step name. Read-only view for debug overlay.</summary>
    public IReadOnlyDictionary<string, TickMetricSample> LastByStep => _last;

    /// <summary>
    /// Wrap a step's execution. Returns an <see cref="IDisposable"/> that, when disposed,
    /// stops the Profiler sample, computes elapsed ms, and records against the budget.
    /// </summary>
    public IDisposable Measure(string stepName, float budgetMs)
    {
        if (string.IsNullOrEmpty(stepName)) throw new ArgumentException("stepName must be non-empty.", nameof(stepName));
        Profiler.BeginSample(stepName);
        return new Scope(this, stepName, budgetMs, Stopwatch.StartNew());
    }

    /// <summary>Manually record an already-measured sample. Test helper; production uses <see cref="Measure"/>.</summary>
    public void Record(string stepName, float elapsedMs, float budgetMs)
    {
        if (string.IsNullOrEmpty(stepName)) throw new ArgumentException("stepName must be non-empty.", nameof(stepName));

        var consec = _consecutive.GetValueOrDefault(stepName, 0);
        if (elapsedMs > budgetMs)
        {
            consec++;
            _consecutive[stepName] = consec;
        }
        else if (consec > 0)
        {
            consec = 0;
            _consecutive[stepName] = consec;
        }

        var sample = new TickMetricSample(stepName, elapsedMs, budgetMs, consec);
        _last[stepName] = sample;

        if (consec == OverBudgetWarningThreshold)
        {
            OnBudgetWarning?.Invoke(sample);
        }
    }

    /// <summary>Build a stable snapshot of the most recent samples for debug overlay rendering.</summary>
    public IReadOnlyList<TickMetricSample> Snapshot()
    {
        _snapshot.Clear();
        foreach (var kv in _last)
        {
            _snapshot.Add(kv.Value);
        }
        return _snapshot;
    }

    /// <summary>Drop all recorded data. Test helper.</summary>
    public void Reset()
    {
        _consecutive.Clear();
        _last.Clear();
        _snapshot.Clear();
    }

    private sealed class Scope : IDisposable
    {
        private readonly TickMetrics _owner;
        private readonly string _name;
        private readonly float _budget;
        private readonly Stopwatch _stopwatch;
        private bool _disposed;

        public Scope(TickMetrics owner, string name, float budget, Stopwatch stopwatch)
        {
            _owner = owner;
            _name = name;
            _budget = budget;
            _stopwatch = stopwatch;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _stopwatch.Stop();
            Profiler.EndSample();
            var elapsedMs = (float)_stopwatch.Elapsed.TotalMilliseconds;
            _owner.Record(_name, elapsedMs, _budget);
        }
    }
}
