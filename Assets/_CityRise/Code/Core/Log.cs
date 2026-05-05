#nullable enable

using System;
using UnityDebug = UnityEngine.Debug;

namespace CityRise.Core;

/// <summary>
/// Project-wide logging facade. All sim, system, and tool code logs through this — never call
/// <c>UnityEngine.Debug.Log</c> directly. Routes to a swappable <see cref="ILogSink"/> so a
/// crash-reporter or in-game console can replace the default sink at Bootstrap.
/// </summary>
public static class Log
{
    private static ILogSink s_sink = new UnityDebugSink();

    /// <summary>Replace the active sink. Call from Bootstrap; thread-safe relative to Write only when callers serialize.</summary>
    public static void SetSink(ILogSink sink)
    {
        s_sink = sink ?? throw new ArgumentNullException(nameof(sink));
    }

    /// <summary>Restore the default UnityEngine.Debug sink. Useful in tests.</summary>
    public static void ResetSink() => s_sink = new UnityDebugSink();

    /// <summary>Read-only handle to the active sink. Intended for tests only.</summary>
    public static ILogSink ActiveSink => s_sink;

    public static void Debug(LogCategory category, string message) => s_sink.Write(LogLevel.Debug, category, message);
    public static void Info(LogCategory category, string message) => s_sink.Write(LogLevel.Info, category, message);
    public static void Warn(LogCategory category, string message) => s_sink.Write(LogLevel.Warn, category, message);
    public static void Error(LogCategory category, string message) => s_sink.Write(LogLevel.Error, category, message);

    /// <summary>Default sink. Routes to UnityEngine.Debug with a tagged prefix.</summary>
    private sealed class UnityDebugSink : ILogSink
    {
        public void Write(LogLevel level, LogCategory category, string message)
        {
            var formatted = $"[{category}] {message}";
            switch (level)
            {
                case LogLevel.Error: UnityDebug.LogError(formatted); break;
                case LogLevel.Warn:  UnityDebug.LogWarning(formatted); break;
                default:             UnityDebug.Log(formatted); break;
            }
        }
    }
}
