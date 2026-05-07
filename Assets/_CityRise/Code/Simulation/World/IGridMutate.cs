#nullable enable

using Unity.Mathematics;

namespace CityRise.Simulation.World;

/// <summary>
/// Mutating view of <see cref="GridState"/>. Passed only to Systems during their tick step
/// via <c>ITickStep.Run(IWorldMutate, …)</c> and goes out of scope afterward, so non-System
/// code cannot accidentally hold a mutating reference (ADR-0007).
/// </summary>
public interface IGridMutate : IGridRead
{
    void SetElevation(int2 tile, float value);
    void SetTerrainType(int2 tile, TerrainType value);
    void SetZoneType(int2 tile, ZoneType value);
    void SetDensityCap(int2 tile, byte value);
    void SetDesirability(int2 tile, float value);
    void SetPollution(int2 tile, float value);
    void SetPowerCoverage(int2 tile, bool value);
    void SetWaterCoverage(int2 tile, bool value);
}
