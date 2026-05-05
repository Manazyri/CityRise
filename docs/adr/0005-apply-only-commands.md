# 0005. Command dispatch — Apply-only with Result

Date: 2026-04-20
Status: Accepted

## Context

Player intents need to be modeled in a way that supports validation, rejection with reasons, atomic application, undo/redo, scripting, replay, and unit testing. The naive approach is for tools to mutate state directly, which scatters validation logic and prevents undo. The classical alternative is a Command pattern with `Validate(state)` and `Apply(state)` as separate phases.

## Decision

Each Command implements **`Apply(IWorldMutate, …) → Result<Unit>` only**. Validation happens inside Apply, against the live WorldState, at the moment of application. There is no separate Validate phase. The CommandBus serializes command application on the main thread.

On failure, Apply returns a `Result` carrying a typed `CommandRejectionReason`; the `NotificationBus` surfaces user-facing reasons to the UI. On success, Apply emits a `CommandRecord` containing the inverse command (for undo) and any events to publish.

## Alternatives considered

- **Two-phase Validate → Apply**: classical and clean conceptually, but introduces a time-of-check/time-of-use race when an intervening command mutates state between Validate and Apply. Either you re-validate at apply time (defeating the point) or you accept the race (a correctness bug). Apply-only with embedded validation eliminates the choice.
- **Tools mutate directly**: rejected; loses undo, replay, scripting, testability.

## Consequences

**Positive**
- TOCTOU race is impossible by construction.
- Single source of validation logic per command.
- Undo/redo, replay, debug-console scripting all fall out for free.
- Trivially unit-testable: construct WorldState, call Apply, assert on Result and resulting state.

**Negative**
- Slightly less clear separation of "is this valid?" from "do it" at the call site (caller must always handle Result).

**Neutral**
- Validation logic lives in commands, not in services. This is intentional.
