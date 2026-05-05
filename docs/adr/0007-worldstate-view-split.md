# 0007. WorldState view split — IWorldRead and IWorldMutate

Date: 2026-04-20
Status: Accepted

## Context

WorldState is the single authoritative container for all mutable simulation data. The discipline "only Systems mutate WorldState during their tick" was initially a convention. In a multi-month project with multiple authors (and potentially multiple AI assistants) writing code, conventions slip. A single accidental mutation from UI or Presentation could corrupt sim state in a way that's hard to trace.

## Decision

Split WorldState into two interfaces:

- **`IWorldRead`** — pure queries. Handed to UI, Presentation, Tools, overlays, debug inspector. Has no mutation API.
- **`IWorldMutate`** — passed only to Systems via `ITickStep.Run(IWorldMutate, …)`. Goes out of scope after the tick step returns.

WorldState implements both. Bootstrap composes the service graph so non-System code receives `IWorldRead` only.

## Alternatives considered

- **Convention-only**: original approach; loses the bug-prevention guarantee.
- **WorldState as immutable snapshots with copy-on-write**: cleanest semantically but expensive at our scale and complicates incremental tile updates.
- **Lock/sentinel patterns**: add runtime overhead for what should be a static invariant.

## Consequences

**Positive**
- "Only Systems mutate sim state" becomes a compile-time invariant.
- A UI bug literally cannot corrupt WorldState because it doesn't have a reference that can mutate.
- Code review surface narrows: a PR that introduces an `IWorldMutate` reference outside a System is immediately suspicious.

**Negative**
- A small amount of additional plumbing (interface declarations, Bootstrap wiring).
- Some debugging convenience lost (can't easily mutate WorldState from a console command without a privileged Mutate handle — but the debug console can be granted one).

**Neutral**
- The interfaces are purely structural; no runtime cost.
