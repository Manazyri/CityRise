#nullable enable

using CityRise.Core;
using NUnit.Framework;
using UnityEngine;

namespace CityRise.Tests.EditMode;

public sealed class FeatureFlagsTests
{
    private FeatureFlags _flags = null!;

    [SetUp]
    public void SetUp() => _flags = ScriptableObject.CreateInstance<FeatureFlags>();

    [TearDown]
    public void TearDown() => Object.DestroyImmediate(_flags);

    [Test]
    public void AllFlags_DefaultOff()
    {
        Assert.That(_flags.PowerEnabled, Is.False);
        Assert.That(_flags.WaterEnabled, Is.False);
        Assert.That(_flags.AgentsEnabled, Is.False);
        Assert.That(_flags.GrowthEnabled, Is.False);
        Assert.That(_flags.BudgetEnabled, Is.False);
        Assert.That(_flags.CoverageEnabled, Is.False);
        Assert.That(_flags.OrdinancesEnabled, Is.False);
        Assert.That(_flags.DisastersEnabled, Is.False);
        Assert.That(_flags.ReplayRecorderEnabled, Is.False);
        Assert.That(_flags.IntegrityCheckerEnabled, Is.False);
    }

    [Test]
    public void Setter_FlipsValue_AndFiresChanged()
    {
        var fired = 0;
        _flags.Changed += () => fired++;

        _flags.PowerEnabled = true;
        Assert.That(_flags.PowerEnabled, Is.True);
        Assert.That(fired, Is.EqualTo(1));
    }

    [Test]
    public void Setter_SameValue_DoesNotFire()
    {
        var fired = 0;
        _flags.Changed += () => fired++;

        _flags.WaterEnabled = false;  // already false
        Assert.That(fired, Is.EqualTo(0));
    }
}
