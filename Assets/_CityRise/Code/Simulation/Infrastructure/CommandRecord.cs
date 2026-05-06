#nullable enable

namespace CityRise.Simulation.Infrastructure;

/// <summary>
/// One entry in the command history — the command that ran and its inverse for undo.
/// Phase 2 will additionally carry the events the command emitted, for replay.
/// </summary>
public readonly struct CommandRecord
{
    public readonly ICommand Command;

    /// <summary>Inverse command for undo. Null indicates the command is irreversible (e.g. RNG roll).</summary>
    public readonly ICommand? Inverse;

    public CommandRecord(ICommand command, ICommand? inverse)
    {
        Command = command;
        Inverse = inverse;
    }

    public bool IsReversible => Inverse is not null;
}
