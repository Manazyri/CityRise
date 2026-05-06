#nullable enable

using CityRise.Core;

namespace CityRise.Simulation.Infrastructure;

/// <summary>
/// A serializable description of an intent to mutate WorldState. Per ADR-0005, every command
/// implements <see cref="Apply"/> which both validates against the live state and applies the
/// mutation atomically — there is no separate Validate phase. Failure is signalled via
/// <c>Result.Err</c> with a typed reason; <see cref="CommandBus.OnRejected"/> surfaces the message
/// to the UI through NotificationBus.
/// </summary>
/// <remarks>
/// Phase 1: signature is parameter-less. Phase 2 will extend it to
/// <c>Apply(IWorldMutate, out CommandRecord) → Result&lt;Unit&gt;</c> once WorldState lands.
/// </remarks>
public interface ICommand
{
    /// <summary>Stable identifier used in logs and replay records. Implementations should return a constant.</summary>
    string Name { get; }

    Result<Unit> Apply();
}
