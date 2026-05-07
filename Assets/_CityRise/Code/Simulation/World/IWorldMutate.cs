#nullable enable

namespace CityRise.Simulation.World;

/// <summary>
/// Mutating handle to <see cref="WorldState"/>. Passed only to Systems during their tick step
/// via <c>ITickStep.Run(IWorldMutate, …)</c>; goes out of scope when the step returns. Per
/// ADR-0007 this is the only path through which sim state mutates — UI / Presentation / Tools
/// code receives <see cref="IWorldRead"/> instead and physically cannot mutate state through
/// their handle.
/// </summary>
public interface IWorldMutate : IWorldRead
{
    new IGridMutate Grid { get; }
}
