#nullable enable

using CityRise.Core;
using CityRise.Simulation.World;

namespace CityRise.Simulation.Infrastructure;

/// <summary>
/// A command that does nothing. Phase 2 ships this as the only ICommand implementation —
/// real commands arrive in Phase 5+ when zoning, building, and budget actions land.
/// Used to exercise CommandBus dispatch in tests and to seed the replay log scaffolding.
/// </summary>
public sealed class NoOpCommand : ICommand
{
    public string Name => "NoOp";

    public Result<Unit> Apply(IWorldMutate world) => Result<Unit>.Ok(Unit.Value);
}
