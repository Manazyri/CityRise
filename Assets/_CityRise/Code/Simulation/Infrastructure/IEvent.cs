#nullable enable

namespace CityRise.Simulation.Infrastructure;

/// <summary>
/// Marker for past-tense facts broadcast by sim systems. All events MUST be
/// <c>readonly struct</c> to avoid GC pressure at tick × subscriber cadence
/// (CLAUDE.md coding rule).
/// </summary>
public interface IEvent { }
