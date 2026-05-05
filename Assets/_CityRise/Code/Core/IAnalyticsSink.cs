#nullable enable

using System.Collections.Generic;

namespace CityRise.Core;

/// <summary>
/// Receives every analytics event dispatched through <see cref="Analytics"/>. Default backend
/// is <see cref="NullAnalyticsSink"/> for MVP; commercial-phase swaps in Unity Analytics, Sentry,
/// or a self-hosted endpoint without changing call sites (Tech Roadmap section 4.9).
/// </summary>
public interface IAnalyticsSink
{
    void Track(string eventName, IReadOnlyDictionary<string, object>? payload);
}
