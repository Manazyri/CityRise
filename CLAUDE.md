# CityRise — AI Assistant Context (CLAUDE.md)

This file is read by Claude Code on every session. It captures the project's locked decisions and architectural rules so the assistant can work consistently without re-derivation.

## Project at a glance

- **Genre**: 3D city-builder (PC), stylized low-poly
- **Reference**: SimCity 4 + Cities: Skylines hybrid; leans toward SimCity 4
- **Team**: 2 people (Mark — programmer/lead; 3D modeler)
- **Status**: Hobby project with potential commercial pivot (decision deferred to end of Alpha)
- **Working title**: CityRise

## Locked tech stack

- Engine: **Unity 6 LTS**
- Language: **C# 10**, .NET Standard 2.1, nullable reference types enabled, file-scoped namespaces, warnings-as-errors for nullability
- Render pipeline: **URP**
- Input: **Unity Input System** (new input system)
- UI: **UI Toolkit** (UGUI only when a specific widget justifies it)
- Splines (roads): **Unity Splines** package (com.unity.splines)
- Threading: managed C# default; **Jobs + Burst** for hotspots
- Math: **Unity.Mathematics** in sim code
- Save: **MemoryPack** binary (release) + Newtonsoft JSON debug dumps (dev)
- VCS: Git + Git LFS on GitHub (private repo)
- CI: GitHub Actions (Unity headless Windows build per main merge)

## Architectural pillars (do not violate without an ADR)

- **Layered architecture** with strict downward dependencies (asmdefs):
  Core → Content → Simulation → Persistence → Presentation → UI → Tools → App; Debug and Tests are isolated.
- **Single WorldState** owns every mutable simulation field. Exposed as two interfaces:
  - `IWorldRead` — pure queries, handed to UI/Presentation/Tools/overlays
  - `IWorldMutate` — passed only to Systems during their tick step
- **Commands in, Events out**. Player intents are `Commands`, dispatched through a `CommandBus`. Each command implements `Apply(IWorldMutate, …) → Result<Unit>` only — no separate Validate phase. Sim publishes `Events` via the `EventBus`.
- **Presentation never reads WorldState directly.** It maintains a mirror updated from events. UI, Tools, and overlays read only the mirror.
- **Tools-as-reducers.** A Tool reads the presentation mirror and emits Commands; nothing more.
- **Sim is Unity-free.** The Simulation assembly depends only on Unity.Mathematics, Unity.Collections, and Burst — no `GameObject`, `MonoBehaviour`, `Transform`, or scene references.
- **Data-driven content.** Ordinances, coverage contributions, and desirability factors are modifier/contribution lists on ScriptableObjects, not hardcoded C#.
- **Stable typed entity IDs.** `EntityId<T>` (phantom-typed Guid wrapper). Never store raw array indices or transform references in sim state.
- **Sim time is authoritative.** TickScheduler runs N sim ticks per real frame based on speed multiplier; wall-clock never enters sim code.

## Naming conventions

- `*System` — tick-driven, mutates WorldState (UtilitySystem, GrowthSystem, BudgetSystem, …)
- `*Service` — stateless, query- or I/O-oriented (GridService, SaveService, ContentRegistry, …)
- Infrastructure has its own names (TickScheduler, CommandBus, EventBus, NotificationBus, TickMetrics)
- Mixing the suffixes is a review-blocking issue.

## Coding rules (enforced)

- File-scoped namespaces; one public type per file.
- All events are `readonly struct` (no GC pressure at tick cadence).
- No exceptions in the sim tick — return `Result<T>`.
- No `System.Random` or `UnityEngine.Random` in sim code — use the seeded RNG service.
- No iteration of `NativeHashMap` directly — use the `IterateSorted` helper.
- Allocation budget in Update and tick loops: zero. Use pooled collections and struct enumerators.
- All user-facing strings via `I18n.Get(key)`.
- All logging via the `Log` facade with a category tag.

## Constants of note (in `Core/GameConstants.cs`)

- Tile size (sim/zoning grid): **8 m**
- Heightmap vertex spacing: **4 m**
- Default map size: **2048 × 2048 m** (256 × 256 tiles)
- Sim tick rate: 1 Hz at 1× speed
- Growth tick rate: 0.1 Hz at 1× speed
- Budget tick: monthly
- Visual agents (post-MVP): hard caps ~200 pedestrians, ~500 vehicles
- Coordinate system: Y-up, left-handed, 1 unit = 1 m, tile (0,0) at world origin, forward = +Z

## Current phase

**Phase 0 — Setup, tech stack, Phase-0 facades.** See `PHASE_0_TASKS.md` for the sequenced task list and `docs/CityRise_Tech_Roadmap.docx` Section 6.2 for full goals and acceptance criteria.

## Where to find more

- `docs/CityRise_GDD.docx` — game design document
- `docs/CityRise_Tech_Roadmap.docx` — full architecture and phased plan (v0.4)
- `docs/adr/` — Architecture Decision Records
- `docs/perf-budget.md` — per-system CPU budgets
- `docs/analytics-events.md` — event taxonomy (grows over time)

## What NOT to do (common mistakes to avoid)

- Don't mutate `WorldState` from UI, Tools, Presentation, or anywhere except a System receiving `IWorldMutate`.
- Don't poll `WorldState` from Presentation — subscribe to events and update the mirror.
- Don't introduce ECS/DOTS as a blanket architecture. Burst + Jobs only on profiled hotspots.
- Don't add gameplay-rule mod hooks (Lua, custom DSL). Content mods only for V1.
- Don't add features that aren't in the current phase's goal list without an explicit scope conversation.
- Don't ship Unity-side state references in sim code (no Transform, no GameObject, no MonoBehaviour).
- Don't write to main without CI passing.
