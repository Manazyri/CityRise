#nullable enable

namespace CityRise.Core;

/// <summary>
/// Receives every log message dispatched through <see cref="Log"/>. Implementations route to
/// UnityEngine.Debug, on-screen consoles, file sinks, or remote crash reporters (later).
/// </summary>
public interface ILogSink
{
    void Write(LogLevel level, LogCategory category, string message);
}
