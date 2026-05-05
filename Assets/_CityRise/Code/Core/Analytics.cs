#nullable enable

using System;
using System.Collections.Generic;

namespace CityRise.Core;

/// <summary>
/// Project-wide analytics facade. Every event added to <c>docs/analytics-events.md</c> ships
/// through this. Default backend is <see cref="NullAnalyticsSink"/>; Bootstrap may swap in
/// a real sink at session start.
/// </summary>
public static class Analytics
{
    private static IAnalyticsSink s_sink = NullAnalyticsSink.Instance;

    /// <summary>Replace the active sink. Call from Bootstrap.</summary>
    public static void SetSink(IAnalyticsSink sink)
    {
        s_sink = sink ?? throw new ArgumentNullException(nameof(sink));
    }

    /// <summary>Restore the no-op sink. Test-only.</summary>
    public static void ResetSink() => s_sink = NullAnalyticsSink.Instance;

    /// <summary>Read-only handle to the active sink. Tests assert against this.</summary>
    public static IAnalyticsSink ActiveSink => s_sink;

    /// <summary>Track an event with no payload.</summary>
    public static void Track(string eventName)
    {
        if (string.IsNullOrEmpty(eventName)) throw new ArgumentException("eventName must be non-empty.", nameof(eventName));
        s_sink.Track(eventName, null);
    }

    /// <summary>Track an event with a key-value payload.</summary>
    public static void Track(string eventName, IReadOnlyDictionary<string, object> payload)
    {
        if (string.IsNullOrEmpty(eventName)) throw new ArgumentException("eventName must be non-empty.", nameof(eventName));
        if (payload is null) throw new ArgumentNullException(nameof(payload));
        s_sink.Track(eventName, payload);
    }
}
