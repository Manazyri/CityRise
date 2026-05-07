#nullable enable

namespace CityRise.Core;

/// <summary>
/// Tiny coordination point for "is something else capturing keyboard input right now?".
/// Overlays that take focus (debug console, modal dialogs, future menus) flip
/// <see cref="SuppressGameplayHotkeys"/> on while open. Hotkey listeners in the UI / Tools
/// layers check it before consuming keys (e.g. TimeControlPanel skips space/1/2/3 when the
/// debug console is typing).
/// </summary>
/// <remarks>
/// Lives in Core so both the UI layer and the Debug layer can reach it without violating the
/// downward-dependency rule. Static state is acceptable here — this is a UI input mode flag,
/// not gameplay state (CLAUDE.md "no static singletons" rule explicitly carves out
/// infrastructure plumbing).
/// </remarks>
public static class InputContext
{
    /// <summary>True while a fullscreen-capturing UI overlay is active.</summary>
    public static bool SuppressGameplayHotkeys { get; set; }
}
