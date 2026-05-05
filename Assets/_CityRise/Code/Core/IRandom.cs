#nullable enable

namespace CityRise.Core;

/// <summary>
/// Seeded RNG used by all sim code. <c>System.Random</c> and <c>UnityEngine.Random</c> are banned
/// in the Simulation asmdef — use this interface so determinism is preserved across replays
/// and saves (Tech Roadmap section 4.9).
/// </summary>
public interface IRandom
{
    /// <summary>Re-seeds the stream. Use at world load to restore a known sequence.</summary>
    void Reseed(uint seed);

    /// <summary>The current seed used to construct or last-Reseed this stream.</summary>
    uint Seed { get; }

    /// <summary>Uniform integer in [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>).</summary>
    int NextInt(int minInclusive, int maxExclusive);

    /// <summary>Uniform float in [0, 1).</summary>
    float NextFloat();

    /// <summary>Uniform float in [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>).</summary>
    float NextFloat(float minInclusive, float maxExclusive);

    /// <summary>Uniform double in [0, 1).</summary>
    double NextDouble();

    /// <summary>True with probability 0.5.</summary>
    bool NextBool();

    /// <summary>True with probability <paramref name="probability"/>; clamped to [0, 1].</summary>
    bool NextBool(float probability);
}
