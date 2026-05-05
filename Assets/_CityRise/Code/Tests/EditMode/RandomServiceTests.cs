#nullable enable

using System;
using CityRise.Core;
using NUnit.Framework;

namespace CityRise.Tests.EditMode;

public sealed class RandomServiceTests
{
    [Test]
    public void SameSeed_ProducesSameSequence()
    {
        var a = new RandomService(12345u);
        var b = new RandomService(12345u);

        for (int i = 0; i < 100; i++)
            Assert.That(a.NextInt(0, 1_000_000), Is.EqualTo(b.NextInt(0, 1_000_000)),
                $"divergence at draw {i}");
    }

    [Test]
    public void DifferentSeeds_ProduceDifferentSequences()
    {
        var a = new RandomService(1u);
        var b = new RandomService(2u);
        var anyDifferent = false;
        for (int i = 0; i < 50; i++)
        {
            if (a.NextInt(0, int.MaxValue) != b.NextInt(0, int.MaxValue)) { anyDifferent = true; break; }
        }
        Assert.That(anyDifferent, Is.True, "Two different seeds produced 50 identical draws — RNG is broken.");
    }

    [Test]
    public void Reseed_RestoresSequence()
    {
        var rng = new RandomService(999u);
        var first = rng.NextInt(0, 1_000_000);
        rng.NextFloat(); rng.NextBool(); // perturb state

        rng.Reseed(999u);
        Assert.That(rng.NextInt(0, 1_000_000), Is.EqualTo(first));
    }

    [Test]
    public void SeedZero_NormalizedToOne_AndUsable()
    {
        var rng = new RandomService(0u);
        Assert.That(rng.Seed, Is.EqualTo(1u));
        Assert.DoesNotThrow(() => rng.NextInt(0, 10));
    }

    [Test]
    public void NextInt_RespectsRange()
    {
        var rng = new RandomService(42u);
        for (int i = 0; i < 1000; i++)
        {
            var n = rng.NextInt(5, 10);
            Assert.That(n, Is.InRange(5, 9));
        }
    }

    [Test]
    public void NextInt_InvalidRange_Throws()
    {
        var rng = new RandomService(1u);
        Assert.That(() => rng.NextInt(10, 5), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => rng.NextInt(5, 5), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void NextFloat_InZeroOne()
    {
        var rng = new RandomService(7u);
        for (int i = 0; i < 1000; i++)
        {
            var f = rng.NextFloat();
            Assert.That(f, Is.GreaterThanOrEqualTo(0f).And.LessThan(1f));
        }
    }

    [Test]
    public void NextBool_ZeroProbability_AlwaysFalse()
    {
        var rng = new RandomService(1u);
        for (int i = 0; i < 100; i++)
            Assert.That(rng.NextBool(0f), Is.False);
    }

    [Test]
    public void NextBool_OneProbability_AlwaysTrue()
    {
        var rng = new RandomService(1u);
        for (int i = 0; i < 100; i++)
            Assert.That(rng.NextBool(1f), Is.True);
    }
}
