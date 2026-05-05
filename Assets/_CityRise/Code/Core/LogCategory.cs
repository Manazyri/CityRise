#nullable enable

namespace CityRise.Core;

/// <summary>
/// Layer or subsystem the log entry came from. Used by sinks to filter, color, or route messages.
/// Mirrors the asmdef layout (Tech Roadmap section 3.3).
/// </summary>
public enum LogCategory
{
    Core,
    Content,
    Sim,
    Persistence,
    Render,
    UI,
    Tools,
    App,
    Net,
    Debug,
}
