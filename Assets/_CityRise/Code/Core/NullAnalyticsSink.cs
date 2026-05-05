#nullable enable

using System.Collections.Generic;

namespace CityRise.Core;

/// <summary>
/// No-op analytics sink. Active for MVP; swapped in commercial phase per the analytics audit.
/// </summary>
public sealed class NullAnalyticsSink : IAnalyticsSink
{
    public static readonly NullAnalyticsSink Instance = new();

    public void Track(string eventName, IReadOnlyDictionary<string, object>? payload)
    {
        // intentional no-op
    }
}
