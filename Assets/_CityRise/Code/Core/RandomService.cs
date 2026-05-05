#nullable enable

using System;
using MathRandom = Unity.Mathematics.Random;

namespace CityRise.Core;

/// <summary>
/// Default <see cref="IRandom"/> backed by <see cref="Unity.Mathematics.Random"/> (xoshiro128**).
/// Deterministic for a given seed; cheap to construct; safe to use from Burst-compiled jobs
/// when the underlying state is exposed via the inner field (do that explicitly when needed).
/// </summary>
public sealed class RandomService : IRandom
{
    private MathRandom _rng;
    private uint _seed;

    public RandomService(uint seed)
    {
        _seed = NormalizeSeed(seed);
        _rng = new MathRandom(_seed);
    }

    public uint Seed => _seed;

    public void Reseed(uint seed)
    {
        _seed = NormalizeSeed(seed);
        _rng = new MathRandom(_seed);
    }

    public int NextInt(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive)
            throw new ArgumentOutOfRangeException(nameof(maxExclusive), "maxExclusive must be greater than minInclusive.");
        return _rng.NextInt(minInclusive, maxExclusive);
    }

    public float NextFloat() => _rng.NextFloat();

    public float NextFloat(float minInclusive, float maxExclusive)
    {
        if (maxExclusive <= minInclusive)
            throw new ArgumentOutOfRangeException(nameof(maxExclusive), "maxExclusive must be greater than minInclusive.");
        return _rng.NextFloat(minInclusive, maxExclusive);
    }

    public double NextDouble() => _rng.NextDouble();

    public bool NextBool() => _rng.NextBool();

    public bool NextBool(float probability)
    {
        if (probability <= 0f) return false;
        if (probability >= 1f) return true;
        return _rng.NextFloat() < probability;
    }

    // Unity.Mathematics.Random rejects 0 — it's a degenerate seed for xoshiro.
    private static uint NormalizeSeed(uint seed) => seed == 0u ? 1u : seed;
}
