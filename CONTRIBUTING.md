# Contributing to CityRise

This file is for human contributors. AI assistants should also read [`CLAUDE.md`](CLAUDE.md) for the locked architectural rules.

## Branching

- **Trunk-based.** `main` is always green and always launches.
- Short-lived feature branches: `feat/<phase>-<short-name>`, `fix/<short-name>`, `chore/<short-name>`.
- Solo prototypes can self-merge. PRs are required for anything touching shared systems (Core, Simulation, Persistence, content pipeline, save format).
- Don't push to `main` if CI is red. Don't merge a PR until its checks are green.

## Commits

Use [Conventional Commits](https://www.conventionalcommits.org/):

- `feat(<scope>): ...` — new feature or capability
- `fix(<scope>): ...` — bug fix
- `chore(<scope>): ...` — tooling, config, deps, no code behavior change
- `refactor(<scope>): ...` — code restructure, no behavior change
- `test(<scope>): ...` — tests added or modified
- `docs(<scope>): ...` — docs only
- `perf(<scope>): ...` — performance fix

Scopes follow the layer name where useful: `core`, `sim`, `content`, `presentation`, `ui`, `tools`, `app`, `persistence`, `debug`, `ci`. One commit, one logical change.

## Pull requests

PR checklist (paste into the description):

- [ ] CI is green (build + tests)
- [ ] No new public API outside the layer it belongs to
- [ ] No `IWorldMutate` references outside Systems
- [ ] No direct `WorldState` reads from Presentation/UI/Tools (mirror only)
- [ ] No `System.Random` or `UnityEngine.Random` in `Simulation` asmdef
- [ ] No exceptions thrown from sim tick code (use `Result<T>`)
- [ ] All user-facing strings via `I18n.Get(key)`
- [ ] Allocation-free hot paths verified (Profiler if non-trivial)
- [ ] New constants live in `Core/GameConstants.cs`, not inline magic numbers
- [ ] If this changes save format: migration function added and tested
- [ ] If this changes architecture: ADR added in `docs/adr/`
- [ ] Phase-close commits include attached ms/tick numbers for any modified system

## Architecture Decision Records (ADRs)

Significant technical decisions go in `docs/adr/NNNN-kebab-case-title.md`. Template:

```markdown
# NNNN. Title

Date: YYYY-MM-DD
Status: Proposed | Accepted | Superseded by ADR-XXXX

## Context
What problem are we solving? What constraints are in play?

## Decision
What we're doing and why.

## Alternatives considered
Options we evaluated and why we didn't pick them.

## Consequences
Positive, negative, and neutral outcomes of this decision.
```

Existing ADRs: `0001-engine-choice` through `0009-typed-entity-ids`. Increment the number for each new one. Don't renumber accepted ADRs — supersede them instead.

## Coding rules

The full rule set lives in [`CLAUDE.md`](CLAUDE.md). The high-leverage ones:

- File-scoped namespaces. One public type per file.
- Nullable reference types enabled; warnings-as-errors for nullability.
- All events are `readonly struct`. No GC pressure in tick loops.
- No exceptions from sim tick. Return `Result<T>`.
- Sim asmdef depends on Unity.Mathematics, Unity.Collections, Burst — and nothing else from Unity.
- No iteration of `NativeHashMap` directly. Use `IterateSorted`.
- All logging through the `Log` facade with a category tag.
- All user-facing strings via `I18n.Get(key)`.
- Naming: `*System` for tick-driven mutators; `*Service` for stateless utilities. Mixing is review-blocking.

## Tests

- **Unit tests** (`Tests/EditMode`): Simulation logic, headless, no GameObjects. Fast.
- **Play-mode tests** (`Tests/PlayMode`): Presentation behavior, scene loading.
- Add a test alongside any non-trivial sim change. A bug fix should land with a regression test that fails before the fix and passes after.

## Asset register

External assets (Asset Store packages, freesound CC0 SFX, commissioned art) get a row in `docs/asset-register.md` with name, license, and source URL. This is non-optional if we ever go commercial.

## Issue labels

- Priority: `p0` (release-blocking), `p1` (next milestone), `p2` (later)
- Tier: `mvp`, `v1`, `stretch`
- Phase: `phase-0` through `phase-10`
- Type: `bug`, `feat`, `tech-debt`, `polish`, `tooling`

Labels mirror the GDD's MVP/V1/Stretch tiers. A `mvp` issue is implicitly higher priority than a `v1` issue.

## Performance discipline

- Each system has a soft ms/tick budget in [`docs/perf-budget.md`](docs/perf-budget.md).
- TickMetrics warns when any system exceeds budget for 3 consecutive ticks.
- Phase-close commits attach the ms/tick numbers in the commit message.
- Crossing budget by >2× requires either an optimization PR or an ADR raising the budget with justification.
