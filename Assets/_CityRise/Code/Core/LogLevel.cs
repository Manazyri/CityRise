#nullable enable

namespace CityRise.Core;

/// <summary>Severity of a log entry. Sinks decide how to render each level.</summary>
public enum LogLevel
{
    Debug,
    Info,
    Warn,
    Error,
}
