#nullable enable

namespace CityRise.Persistence;

/// <summary>
/// One entry in <see cref="ReplayRecorder"/>'s ring buffer. Phase 1 records only the sim tick
/// and command name — enough for diagnostic dumps and confirming a replay's command stream
/// matches expectations. Phase 2 will extend with a serialized payload so the replay player
/// can re-construct the command and re-apply it.
/// </summary>
public readonly struct RecordedCommand
{
    public readonly ulong SimTick;
    public readonly string CommandName;

    public RecordedCommand(ulong simTick, string commandName)
    {
        SimTick = simTick;
        CommandName = commandName ?? string.Empty;
    }
}
