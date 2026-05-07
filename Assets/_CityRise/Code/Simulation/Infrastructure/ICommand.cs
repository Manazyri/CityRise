#nullable enable

using CityRise.Core;
using CityRise.Simulation.World;

namespace CityRise.Simulation.Infrastructure;

/// <summary>
/// A serializable description of an intent to mutate WorldState. Per ADR-0005, every command
/// implements <see cref="Apply"/> which both validates against the live state and applies the
/// mutation atomically — there is no separate Validate phase. Failure is signalled via
/// <see cref="Result{Unit}"/> Err with a typed reason; <see cref="CommandBus.OnRejected"/>
/// surfaces the message to the UI through NotificationBus.
/// </summary>
/// <remarks>
/// The <see cref="IWorldMutate"/> reference is valid only during the call. Commands must not
/// store it — they may only use it to read and write tile data (and Phase 3+ road / building /
/// budget data). Phase 2 ships the parameter; Phase 4+ may add an <c>out CommandRecord</c>
/// parameter so commands return their inverse for undo.
/// </remarks>
public interface ICommand
{
    /// <summary>Stable identifier used in logs and replay records. Implementations should return a constant.</summary>
    string Name { get; }

    Result<Unit> Apply(IWorldMutate world);
}
