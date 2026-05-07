#nullable enable

using System.Collections.Generic;
using CityRise.Core;
using CityRise.Simulation.Infrastructure;
using CityRise.Simulation.World;
using NUnit.Framework;
using Unity.Collections;

namespace CityRise.Tests.EditMode;

public sealed class TickSchedulerTests
{
    private sealed class CountingStep : ITickStep
    {
        public string Name { get; }
        public float BudgetMs => 1f;
        public int RunCount;
        public List<TickContext> Contexts = new();

        public CountingStep(string name) { Name = name; }

        public Result<Unit> Run(IWorldMutate world, in TickContext context)
        {
            RunCount++;
            Contexts.Add(context);
            return Result<Unit>.Ok(Unit.Value);
        }
    }

    private sealed class FailingStep : ITickStep
    {
        public string Name => "fail";
        public float BudgetMs => 1f;
        public int RunCount;

        public Result<Unit> Run(IWorldMutate world, in TickContext context)
        {
            RunCount++;
            return Result<Unit>.Err("boom");
        }
    }

    private sealed class RecordingStep : ITickStep
    {
        private readonly List<string> _sink;
        public string Name { get; }
        public float BudgetMs => 1f;
        public RecordingStep(string name, List<string> sink) { Name = name; _sink = sink; }
        public Result<Unit> Run(IWorldMutate world, in TickContext context)
        {
            _sink.Add(Name);
            return Result<Unit>.Ok(Unit.Value);
        }
    }

    /// <summary>One-tile WorldState pinned to Allocator.Temp; passed to every TickScheduler.</summary>
    private static (TickScheduler scheduler, WorldState world) NewScheduler()
    {
        var world = new WorldState(1, Allocator.Temp);
        var scheduler = new TickScheduler(world);
        return (scheduler, world);
    }

    [Test]
    public void Update_OneSecondAtNormalSpeed_Fires_OneSimTick()
    {
        var (s, world) = NewScheduler();
        try
        {
            var step = new CountingStep("a");
            s.RegisterSim(step);

            s.Update(1.0d);

            Assert.That(step.RunCount, Is.EqualTo(1));
            Assert.That(s.SimTickCount, Is.EqualTo(1u));
        }
        finally { world.Dispose(); }
    }

    [Test]
    public void Update_PartialSecond_DoesNotFire()
    {
        var (s, world) = NewScheduler();
        try
        {
            var step = new CountingStep("a");
            s.RegisterSim(step);

            s.Update(0.4d);
            Assert.That(step.RunCount, Is.EqualTo(0));

            s.Update(0.4d);
            Assert.That(step.RunCount, Is.EqualTo(0));

            s.Update(0.3d);
            Assert.That(step.RunCount, Is.EqualTo(1));
        }
        finally { world.Dispose(); }
    }

    [Test]
    public void Update_DoubleSpeed_FiresTwicePerSecond()
    {
        var (s, world) = NewScheduler();
        try
        {
            s.Speed = SpeedMultiplier.Fast;
            var step = new CountingStep("a");
            s.RegisterSim(step);

            s.Update(1.0d);

            Assert.That(step.RunCount, Is.EqualTo(2));
            Assert.That(s.SimTickCount, Is.EqualTo(2u));
        }
        finally { world.Dispose(); }
    }

    [Test]
    public void Paused_NoTicksFire()
    {
        var (s, world) = NewScheduler();
        try
        {
            s.Speed = SpeedMultiplier.Paused;
            var step = new CountingStep("a");
            s.RegisterSim(step);

            s.Update(10d);

            Assert.That(step.RunCount, Is.EqualTo(0));
        }
        finally { world.Dispose(); }
    }

    [Test]
    public void GrowthTick_FiresEvery10thSimTick()
    {
        var (s, world) = NewScheduler();
        try
        {
            var sim = new CountingStep("sim");
            var growth = new CountingStep("growth");
            s.RegisterSim(sim);
            s.RegisterGrowth(growth);

            s.Update(10d);

            Assert.That(sim.RunCount, Is.EqualTo(10));
            Assert.That(growth.RunCount, Is.EqualTo(1));
            Assert.That(s.SimTickCount, Is.EqualTo(10u));
            Assert.That(s.GrowthTickCount, Is.EqualTo(1u));
        }
        finally { world.Dispose(); }
    }

    [Test]
    public void BudgetTick_FiresEvery60thSimTick()
    {
        var (s, world) = NewScheduler();
        try
        {
            s.Speed = SpeedMultiplier.Faster; // 3x = 60 ticks in 20 real seconds
            var budget = new CountingStep("budget");
            s.RegisterBudget(budget);

            s.Update(20d);

            Assert.That(s.SimTickCount, Is.EqualTo(60u));
            Assert.That(s.BudgetTickCount, Is.EqualTo(1u));
            Assert.That(budget.RunCount, Is.EqualTo(1));
        }
        finally { world.Dispose(); }
    }

    [Test]
    public void StepsRunInRegistrationOrder()
    {
        var (s, world) = NewScheduler();
        try
        {
            var order = new List<string>();
            s.RegisterSim(new RecordingStep("first", order));
            s.RegisterSim(new RecordingStep("second", order));
            s.RegisterSim(new RecordingStep("third", order));

            s.Update(1d);

            Assert.That(order, Is.EqualTo(new[] { "first", "second", "third" }));
        }
        finally { world.Dispose(); }
    }

    [Test]
    public void StepError_FiresEvent_ButContinuesPipeline()
    {
        var (s, world) = NewScheduler();
        try
        {
            var failing = new FailingStep();
            var afterFail = new CountingStep("after");
            s.RegisterSim(failing);
            s.RegisterSim(afterFail);

            var errors = new List<string>();
            s.OnStepError += (step, ctx, msg) => errors.Add($"{step.Name}:{msg}");

            s.Update(1d);

            Assert.That(failing.RunCount, Is.EqualTo(1));
            Assert.That(afterFail.RunCount, Is.EqualTo(1), "Pipeline must continue after a step Err.");
            Assert.That(errors, Is.EqualTo(new[] { "fail:boom" }));
        }
        finally { world.Dispose(); }
    }

    [Test]
    public void Reset_ClearsCountersAndAccumulator()
    {
        var (s, world) = NewScheduler();
        try
        {
            s.RegisterSim(new CountingStep("a"));
            s.Update(5d);

            s.Reset();

            Assert.That(s.SimTickCount, Is.EqualTo(0u));
            Assert.That(s.GrowthTickCount, Is.EqualTo(0u));
            Assert.That(s.BudgetTickCount, Is.EqualTo(0u));
        }
        finally { world.Dispose(); }
    }

    [Test]
    public void Register_NullStep_Throws()
    {
        var (s, world) = NewScheduler();
        try
        {
            Assert.That(() => s.RegisterSim(null!), Throws.ArgumentNullException);
        }
        finally { world.Dispose(); }
    }

    [Test]
    public void Constructor_NullWorld_Throws()
    {
        Assert.That(() => new TickScheduler(null!), Throws.ArgumentNullException);
    }

    [Test]
    public void TickContext_CarriesCurrentCounters()
    {
        var (s, world) = NewScheduler();
        try
        {
            var sim = new CountingStep("sim");
            s.RegisterSim(sim);
            s.Update(3d);

            Assert.That(sim.Contexts.Count, Is.EqualTo(3));
            Assert.That(sim.Contexts[0].SimTickCount, Is.EqualTo(0u));
            Assert.That(sim.Contexts[1].SimTickCount, Is.EqualTo(1u));
            Assert.That(sim.Contexts[2].SimTickCount, Is.EqualTo(2u));
            Assert.That(sim.Contexts[0].Kind, Is.EqualTo(TickKind.Sim));
        }
        finally { world.Dispose(); }
    }
}
