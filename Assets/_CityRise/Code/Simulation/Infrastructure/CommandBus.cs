#nullable enable

using System;
using System.Collections.Generic;
using CityRise.Core;

namespace CityRise.Simulation.Infrastructure;

/// <summary>
/// Serial Apply-only command dispatcher (ADR-0005). Tools and UI <see cref="Submit"/> commands;
/// the sim's <c>CommandBus.DrainQueue</c> step applies them in order at the start of each sim
/// tick. Successful applies push to the undo stack; rejections surface via <see cref="OnRejected"/>.
/// </summary>
public sealed class CommandBus
{
    /// <summary>Per Tech Roadmap section 7 — undo bound to 50 commands.</summary>
    public const int MaxUndoEntries = 50;

    private readonly Queue<ICommand> _pending = new();
    private readonly LinkedList<CommandRecord> _undo = new();
    private readonly LinkedList<CommandRecord> _redo = new();

    /// <summary>Fires for each rejected command: (command, reason). UI subscribes via NotificationBus.</summary>
    public event Action<ICommand, string>? OnRejected;

    /// <summary>Number of commands waiting in the queue.</summary>
    public int PendingCount => _pending.Count;

    /// <summary>Depth of the undo stack.</summary>
    public int UndoDepth => _undo.Count;

    /// <summary>Depth of the redo stack.</summary>
    public int RedoDepth => _redo.Count;

    /// <summary>Queue a command for the next <see cref="DrainQueue"/>.</summary>
    public void Submit(ICommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));
        _pending.Enqueue(command);
    }

    /// <summary>
    /// Apply every pending command in submission order. Returns the number that succeeded.
    /// Designed to run as the first step of each sim tick.
    /// </summary>
    public int DrainQueue()
    {
        var succeeded = 0;
        while (_pending.Count > 0)
        {
            var cmd = _pending.Dequeue();
            var result = cmd.Apply();
            if (result.IsOk)
            {
                PushUndo(new CommandRecord(cmd, inverse: null));
                _redo.Clear();
                succeeded++;
            }
            else
            {
                OnRejected?.Invoke(cmd, result.Error);
            }
        }
        return succeeded;
    }

    /// <summary>Pop the most recent undoable command and apply its inverse. Returns Err if nothing reversible is available.</summary>
    public Result<Unit> Undo()
    {
        while (_undo.Count > 0)
        {
            var last = _undo.Last!.Value;
            _undo.RemoveLast();
            if (last.Inverse is null) continue;

            var result = last.Inverse.Apply();
            if (result.IsErr) return result;

            _redo.AddLast(last);
            return Result<Unit>.Ok(Unit.Value);
        }
        return Result<Unit>.Err("Nothing to undo.");
    }

    /// <summary>Pop the most recent undone command and re-apply it.</summary>
    public Result<Unit> Redo()
    {
        if (_redo.Count == 0) return Result<Unit>.Err("Nothing to redo.");
        var record = _redo.Last!.Value;
        _redo.RemoveLast();
        var result = record.Command.Apply();
        if (result.IsErr) return result;
        PushUndo(record);
        return Result<Unit>.Ok(Unit.Value);
    }

    /// <summary>Drop everything. Test-only helper.</summary>
    public void Reset()
    {
        _pending.Clear();
        _undo.Clear();
        _redo.Clear();
    }

    private void PushUndo(CommandRecord record)
    {
        _undo.AddLast(record);
        while (_undo.Count > MaxUndoEntries)
        {
            _undo.RemoveFirst();
        }
    }
}
