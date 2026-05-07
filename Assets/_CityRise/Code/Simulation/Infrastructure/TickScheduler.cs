#nullable enable

using System;
using System.Collections.Generic;
using CityRise.Core;
using CityRise.Simulation.World;

namespace CityRise.Simulation.Infrastructure;

/// <summary>
/// Drives sim, growth, and budget clocks at the rates declared in <see cref="GameConstants"/>.
/// The host (Bootstrap) calls <see cref="Update"/> once per frame with the wall-clock delta;
/// the scheduler converts that into discrete ticks at the appropriate rates and fires each
/// registered <see cref="ITickStep"/>. Sim time is authoritative — wall-clock never enters
/// step code (Tech Roadmap section 4.5).
/// </summary>
public sealed class TickScheduler
{
    private readonly IWorldMutate _world;
    private readonly List<ITickStep> _simSteps = new();
    private readonly List<ITickStep> _growthSteps = new();
    private readonly List<ITickStep> _budgetSteps = new();

    private double _simAccumSeconds;
    private int _simTicksSinceGrowth;
    private int _simTicksSinceBudget;

    public TickScheduler(IWorldMutate world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
    }

    /// <summary>Multiplies real-time deltas to compress sim time. <see cref="SpeedMultiplier.Paused"/> halts ticks.</summary>
    public SpeedMultiplier Speed { get; set; } = SpeedMultiplier.Normal;

    /// <summary>Monotonic count of sim ticks executed since construction.</summary>
    public ulong SimTickCount { get; private set; }

    /// <summary>Monotonic count of growth ticks executed since construction.</summary>
    public ulong GrowthTickCount { get; private set; }

    /// <summary>Monotonic count of budget ticks executed since construction.</summary>
    public ulong BudgetTickCount { get; private set; }

    /// <summary>Fires when an <see cref="ITickStep"/> returns Err. Subscribers (NotificationBus, Log) decide how to surface.</summary>
    public event Action<ITickStep, TickContext, string>? OnStepError;

    /// <summary>Register a step on the sim clock (1 Hz at 1×). Order of registration is order of execution.</summary>
    public void RegisterSim(ITickStep step) => Register(_simSteps, step);

    /// <summary>Register a step on the growth clock (0.1 Hz at 1×). Order of registration is order of execution.</summary>
    public void RegisterGrowth(ITickStep step) => Register(_growthSteps, step);

    /// <summary>Register a step on the budget clock (monthly). Order of registration is order of execution.</summary>
    public void RegisterBudget(ITickStep step) => Register(_budgetSteps, step);

    /// <summary>Number of registered steps for the given clock. Test convenience.</summary>
    public int StepCount(TickKind kind) => kind switch
    {
        TickKind.Sim => _simSteps.Count,
        TickKind.Growth => _growthSteps.Count,
        TickKind.Budget => _budgetSteps.Count,
        _ => 0,
    };

    /// <summary>
    /// Advance the simulation by <paramref name="realDeltaSeconds"/> of wall-clock time.
    /// Multiple sim ticks may fire from a single Update call when speed > 1× or when delta exceeds
    /// the sim interval (e.g. after a frame stall).
    /// </summary>
    public void Update(double realDeltaSeconds)
    {
        if (Speed == SpeedMultiplier.Paused) return;
        if (realDeltaSeconds <= 0d) return;

        _simAccumSeconds += realDeltaSeconds * (int)Speed;
        var simInterval = 1d / GameConstants.SimTickHz;

        while (_simAccumSeconds >= simInterval)
        {
            _simAccumSeconds -= simInterval;
            ExecuteSimTick();
        }
    }

    /// <summary>Reset accumulators and counters. Test-only; production callers reseed via Bootstrap.</summary>
    public void Reset()
    {
        _simAccumSeconds = 0d;
        _simTicksSinceGrowth = 0;
        _simTicksSinceBudget = 0;
        SimTickCount = 0;
        GrowthTickCount = 0;
        BudgetTickCount = 0;
    }

    private void ExecuteSimTick()
    {
        var context = new TickContext(TickKind.Sim, SimTickCount, GrowthTickCount, BudgetTickCount);
        RunSteps(_simSteps, context);
        SimTickCount++;

        _simTicksSinceGrowth++;
        if (_simTicksSinceGrowth >= GameConstants.SimTicksPerGrowthTick)
        {
            _simTicksSinceGrowth = 0;
            ExecuteGrowthTick();
        }

        _simTicksSinceBudget++;
        if (_simTicksSinceBudget >= GameConstants.SimTicksPerInGameMonth)
        {
            _simTicksSinceBudget = 0;
            ExecuteBudgetTick();
        }
    }

    private void ExecuteGrowthTick()
    {
        var context = new TickContext(TickKind.Growth, SimTickCount, GrowthTickCount, BudgetTickCount);
        RunSteps(_growthSteps, context);
        GrowthTickCount++;
    }

    private void ExecuteBudgetTick()
    {
        var context = new TickContext(TickKind.Budget, SimTickCount, GrowthTickCount, BudgetTickCount);
        RunSteps(_budgetSteps, context);
        BudgetTickCount++;
    }

    private void RunSteps(List<ITickStep> steps, in TickContext context)
    {
        for (int i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            var result = step.Run(_world, in context);
            if (result.IsErr)
            {
                OnStepError?.Invoke(step, context, result.Error);
            }
        }
    }

    private static void Register(List<ITickStep> bucket, ITickStep step)
    {
        if (step is null) throw new ArgumentNullException(nameof(step));
        bucket.Add(step);
    }
}
