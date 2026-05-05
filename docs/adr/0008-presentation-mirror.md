# 0008. Presentation mirror — never read WorldState directly

Date: 2026-04-20
Status: Accepted

## Context

Presentation, UI, Tools, and overlays need to display simulation state. The simplest approach is to give them an `IWorldRead` reference and let them poll WorldState directly. But polling from presentation code creates two problems:

1. **Coupling to internal layout**: presentation code becomes dependent on WorldState's internal data structures (NativeArrays, chunk indices, struct layouts). When WorldState evolves (chunked maps, replay, future networking), presentation breaks.
2. **Lifetime hazards**: NativeArrays can be disposed and reallocated (map resize, save load). Presentation polling could observe torn or dangling state.

## Decision

**Presentation never reads WorldState directly.** The Presentation layer maintains a **mirror** — a set of plain managed data structures updated by Event handlers. UI, Tools, and overlays read only the mirror.

The mirror is presentation-shaped: optimized for what the visual layer needs to show (e.g., per-building visual state, per-tile overlay value caches). Sim emits Events when state changes; the mirror updates; presentation reads the mirror.

## Alternatives considered

- **Direct polling of WorldState via IWorldRead**: simpler today; brittle tomorrow.
- **Push-based change tracking via change tokens or version counters**: more complex than events for the same outcome.

## Consequences

**Positive**
- WorldState can change shape (chunking, swapping for replay, networking) without rewriting presentation.
- No dangling NativeArray references in presentation code.
- Clear dependency direction: Simulation → Event → Presentation mirror; never Presentation → Simulation.
- The mirror is a natural place to add presentation-only smoothing, interpolation, or animation state.

**Negative**
- Two representations of state to keep coherent (mitigated by the mirror being event-driven and not authoring its own truth).
- Slight latency: mirror updates on event flush, not synchronously with sim mutation.

**Neutral**
- Tools, as input reducers, also read the mirror (not WorldState). This reinforces the boundary.
