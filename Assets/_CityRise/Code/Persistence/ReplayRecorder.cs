#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using CityRise.Core;
using CityRise.Simulation.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CityRise.Persistence;

/// <summary>
/// Append-only ring buffer of recorded commands plus the initial RNG seed. Wired to a
/// <see cref="CommandBus"/> at Bootstrap so every successful Apply lands here. Dump writes a
/// compact JSON file for bug reproduction (Tech Roadmap section 4.10). Phase 1 ships the
/// recorder only — the replay player follows in Phase 2+.
/// </summary>
/// <remarks>
/// Buffer is bounded; once capacity is reached, oldest entries are overwritten. This is by design:
/// long sessions don't grow memory unboundedly, and the most recent N commands are usually
/// what's relevant when reproducing a bug.
/// </remarks>
public sealed class ReplayRecorder
{
    /// <summary>Magic at the head of every replay file.</summary>
    public const string Magic = "CityRiseReplay";

    /// <summary>Replay container format version. Bump when the JSON shape changes.</summary>
    public const int CurrentFormatVersion = 1;

    /// <summary>Default ring-buffer capacity.</summary>
    public const int DefaultCapacity = 10_000;

    private readonly RecordedCommand[] _buffer;
    private readonly Func<ulong>? _simTickProvider;
    private int _writeIndex;
    private int _count;

    /// <summary>RNG seed used to construct the session — required for deterministic replay.</summary>
    public uint InitialSeed { get; }

    /// <summary>Maximum entries the buffer will hold.</summary>
    public int Capacity => _buffer.Length;

    /// <summary>Number of entries currently buffered (saturates at <see cref="Capacity"/>).</summary>
    public int Count => _count;

    /// <summary>True once the buffer has wrapped at least once. Test/diagnostic helper.</summary>
    public bool HasWrapped { get; private set; }

    /// <summary>Tag set per build for future diagnostics; defaults to empty.</summary>
    public string BuildTag { get; set; } = string.Empty;

    public ReplayRecorder(uint initialSeed, Func<ulong>? simTickProvider = null, int capacity = DefaultCapacity)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity), "capacity must be positive.");
        InitialSeed = initialSeed;
        _simTickProvider = simTickProvider;
        _buffer = new RecordedCommand[capacity];
    }

    /// <summary>
    /// Subscribe this recorder to the given <see cref="CommandBus"/>'s OnApplied stream. Each
    /// successful Apply records the command at the current sim tick (queried via the provider
    /// passed to the constructor; defaults to 0 if no provider is set).
    /// </summary>
    public void Bind(CommandBus bus)
    {
        if (bus is null) throw new ArgumentNullException(nameof(bus));
        bus.OnApplied += OnCommandApplied;
    }

    /// <summary>Reverse <see cref="Bind"/>.</summary>
    public void Unbind(CommandBus bus)
    {
        if (bus is null) throw new ArgumentNullException(nameof(bus));
        bus.OnApplied -= OnCommandApplied;
    }

    /// <summary>Manually record an entry. Test-only; production wires via <see cref="Bind"/>.</summary>
    public void Record(ICommand command, ulong simTick)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));
        Record(new RecordedCommand(simTick, command.Name));
    }

    /// <summary>Manually record an entry from raw fields. Test-only.</summary>
    public void Record(in RecordedCommand entry)
    {
        _buffer[_writeIndex] = entry;
        _writeIndex = (_writeIndex + 1) % _buffer.Length;
        if (_count < _buffer.Length)
        {
            _count++;
        }
        else
        {
            HasWrapped = true;
        }
    }

    /// <summary>Return a chronologically-ordered copy of the current buffer contents.</summary>
    public IReadOnlyList<RecordedCommand> Snapshot()
    {
        var result = new RecordedCommand[_count];
        if (_count == 0) return result;

        // If the buffer hasn't wrapped, entries are at indices 0.._count-1.
        // After wrap, oldest is at _writeIndex (next write position), newest just before it.
        if (!HasWrapped)
        {
            Array.Copy(_buffer, 0, result, 0, _count);
        }
        else
        {
            var tailLen = _buffer.Length - _writeIndex;
            Array.Copy(_buffer, _writeIndex, result, 0, tailLen);
            Array.Copy(_buffer, 0, result, tailLen, _writeIndex);
        }
        return result;
    }

    /// <summary>Drop all recorded entries.</summary>
    public void Clear()
    {
        _writeIndex = 0;
        _count = 0;
        HasWrapped = false;
        Array.Clear(_buffer, 0, _buffer.Length);
    }

    /// <summary>Write the recorder's contents as JSON to <paramref name="output"/>.</summary>
    public void Write(TextWriter output)
    {
        if (output is null) throw new ArgumentNullException(nameof(output));

        var entries = Snapshot();
        var commands = new JArray();
        for (int i = 0; i < entries.Count; i++)
        {
            commands.Add(new JObject
            {
                ["simTick"] = entries[i].SimTick,
                ["name"] = entries[i].CommandName,
            });
        }

        var root = new JObject
        {
            ["magic"] = Magic,
            ["format"] = CurrentFormatVersion,
            ["savedAt"] = DateTime.UtcNow.ToString("O"),
            ["buildTag"] = BuildTag,
            ["initialSeed"] = InitialSeed,
            ["capacity"] = Capacity,
            ["wrapped"] = HasWrapped,
            ["commands"] = commands,
        };

        using var writer = new JsonTextWriter(output) { Formatting = Formatting.Indented, CloseOutput = false };
        root.WriteTo(writer);
    }

    /// <summary>Serialize to <paramref name="path"/>. Atomic write via temp file + rename.</summary>
    public Result<Unit> Dump(string path)
    {
        if (string.IsNullOrEmpty(path)) return Result<Unit>.Err("Replay dump path must be non-empty.");

        try
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var tempPath = path + ".tmp";
            using (var writer = new StreamWriter(tempPath))
            {
                Write(writer);
            }
            if (File.Exists(path)) File.Delete(path);
            File.Move(tempPath, path);
            return Result<Unit>.Ok(Unit.Value);
        }
        catch (Exception e)
        {
            return Result<Unit>.Err($"Replay dump failed: {e.Message}");
        }
    }

    private void OnCommandApplied(ICommand command)
    {
        var tick = _simTickProvider?.Invoke() ?? 0UL;
        Record(new RecordedCommand(tick, command.Name));
    }
}
