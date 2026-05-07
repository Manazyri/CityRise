#nullable enable

namespace CityRise.Simulation.World;

/// <summary>
/// Read-only handle to <see cref="WorldState"/>. Handed to UI, Presentation, Tools, overlays —
/// non-System code that needs to display or query simulation state without mutating it
/// (ADR-0007). Adding a new mutating field type here is a review-blocking issue.
/// </summary>
public interface IWorldRead
{
    IGridRead Grid { get; }
}
