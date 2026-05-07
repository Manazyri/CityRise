**CityRise — Technical Roadmap**

*Tech Stack, Architecture, and Phased Implementation Plan to MVP*

Version 0.4 — Third-Pass Refinements — April 2026

**1. Document Information**

|  |  |
|----|----|
| **Companion to** | CityRise Game Design Document v0.1 |
| **Audience** | Programmer (Mark); reference for future collaborators |
| **Scope** | From empty Unity project through MVP |
| **MVP definition** | Terrain + terraforming, roads, zoning, ploppables, zone growth, power, water |
| **Engine** | Unity 6 LTS (locked) |
| **Language** | C# 10 / .NET Standard 2.1 |
| **Render pipeline** | URP |
| **Target platform** | Windows 64-bit first; Linux/Mac best-effort later |
| **Build output** | Standalone Player (Mono for dev, IL2CPP for release builds) |
| **Changes in v0.4** | Read/mutate WorldState views (compile-time enforcement); presentation mirror rule (no direct WorldState access from presentation); Tools-as-reducers boundary; spatial indexing for ploppables and road nodes; Phase 2 map generator; per-System sim invariants with debug integrity checker; typed EntityId\<T\> via phantom types; tick pipeline as composable list of steps; intra-tick communication rule (WorldState only); events as readonly struct; runtime-mutable feature flags; IContentRegistry interface for tests; EntityId.ToString debug format; worked save-migration example; debugging-workflow section; commercialization checklist appendix; Phase 4a shipped with primitive curves instead of straight-only; hot-reload desync mitigation; per-phase acceptance-test format. |
| **Changes in v0.3** | System vs Service naming split; Apply-only commands returning Result; data-driven ordinances, coverage contributions, and desirability factors; EntitySpawner lifecycle; error handling via Result + NotificationBus; cross-cutting concerns (accessibility, analytics, tile elevation convention, deterministic iteration); performance budgets from Phase 1 with profiling at every phase close; replay capability called out; gameplay-rule mod support scoped out of V1; Phase 10 restructured into perf / polish / playtest weeks; Architecture Decision Record practice adopted. |
| **Changes in v0.2** | Layered architecture; WorldState; Command bus; shared GraphSolver and OverlaySystem; content category registries; Phase-0 facades (Log, I18n, FeatureFlags, EntityId); explicit tick pipeline; Grid phase moved before Terrain; road phase split into 4a/4b; Unity Splines package adopted; placeholder building checklist appendix. |

**2. Engineering Goals and Philosophy**

- **Ship-ready at every phase boundary.** Main is always green, always launches, always plays what the current phase promises.

- **One authoritative WorldState.** All mutable simulation data lives in a single state object. Services operate on it; nobody else mutates it. Save/load becomes serializing one object.

- **Commands in, Events out.** Every player action is a Command dispatched through a CommandBus. The simulation publishes Events for the presenter and UI. Undo, replay, and scripting fall out for free.

- **Simulation is Unity-free.** Sim code depends only on Unity.Mathematics, Unity.Collections, and Burst. No GameObject, MonoBehaviour, or Transform references. NUnit tests the sim without opening the editor.

- **Data-oriented hot paths.** Tile data is struct-of-arrays NativeArrays. Jobs + Burst where profiling says so. No premature ECS.

- **Data-driven content via category registries.** Buildings, zones, ordinances, ploppables each have a dedicated registry ScriptableObject. A thin ContentRegistry composes them. Category split avoids merge-conflict hell and maps to Addressables groups.

- **Typed entity IDs.** EntityId\<T\> via phantom types — EntityId\<Building\>, EntityId\<RoadSegment\>, EntityId\<Ploppable\>. Compile-time protection against passing a road ID to a building lookup. Guid-backed, zero runtime cost. ToString produces a debug-friendly "bldg_a1b2c3d4" form.

- **Sim time is authoritative.** The TickScheduler runs N sim ticks per real frame based on speed. Wall-clock time never enters sim code. Future-proofs determinism.

- **Feature flags over long-lived branches.** A FeatureFlags ScriptableObject gates WIP systems. Merge to main daily; flip the flag when the system is ready. Flags are runtime-mutable via the debug console — a broken system can be toggled off mid-session without restarting.

- **Constants, not magic numbers.** A single GameConstants file holds tile size (8 m), heightmap vertex spacing (4 m), default map size, tick rates. Grep to change any one.

- **Lean on the ecosystem for non-differentiating systems.** Unity Splines for roads, Cinemachine for camera, Asset Store for pathfinding later. Spend engineering budget on what makes CityRise itself.

- **Explicit error surface.** Commands return Result\<Unit\>. Failures carry a reason code that the UI surfaces through a NotificationBus. No silent swallowing, no unchecked exceptions in sim code.

- **Performance budgets from Phase 1.** Each system declares a soft CPU budget (ms/tick at 500 pop). Profiled at every phase close, not just Phase 10. Regressions caught when they're still cheap to fix.

- **Accessibility is a Phase 0 facade.** Colorblind-safe palettes, scalable UI text, remappable keybinds. Stubbed from day one so we can't accidentally build a feature that defies accessibility later.

- **Architecture Decision Records.** Significant technical decisions are captured in /docs/adr/NNNN-title.md in a lightweight template (Context, Decision, Consequences). Future collaborators understand why without archaeology.

**3. Tech Stack — Locked**

|  |  |
|----|----|
| **Engine** | Unity 6 LTS |
| **Scripting runtime** | Mono during development, IL2CPP for release builds |
| **Render pipeline** | Universal Render Pipeline (URP) |
| **Language / C# level** | C# 10, nullable reference types enabled, file-scoped namespaces |
| **Input** | Unity Input System 1.x |
| **UI** | UI Toolkit (UIElements). UGUI only if a specific widget justifies it. |
| **Content loading** | Addressables |
| **Splines / roads** | Unity Splines package (com.unity.splines) with custom mesh extrusion |
| **Simulation threading** | Managed C# by default; Jobs + Burst for hotspots |
| **Math** | Unity.Mathematics for sim code; UnityEngine math for presentation code |
| **Save format** | Versioned binary (MemoryPack) + JSON debug dumper (dev-only) |
| **Determinism** | Best-effort v0.1; full deterministic pass scheduled post-MVP |
| **Version control** | Git + Git LFS on GitHub (private) |
| **CI** | GitHub Actions running headless Unity builds for Windows on every main merge |
| **Crash reporting** | Unity Cloud Diagnostics (opt-in) once external playtesting starts |

**3.1 Third-Party Packages and Assets**

Items marked "MVP" are needed before the MVP is shippable. Others can be added when the phase needing them starts.

|  |  |  |  |
|----|----|----|----|
| **Package / Tool** | **Purpose** | **Cost** | **MVP?** |
| com.unity.render-pipelines.universal | URP render pipeline | Free | Yes |
| com.unity.inputsystem | New input system | Free | Yes |
| com.unity.addressables | Asset loading, future modding | Free | Yes |
| com.unity.cinemachine | Camera polish, smooth pan/zoom/orbit | Free | Yes |
| com.unity.splines | Spline data + sampling for roads | Free | Yes |
| com.unity.mathematics | Burst-friendly math types | Free | Yes |
| com.unity.burst | Burst compiler for Jobs | Free | Yes |
| com.unity.collections | NativeArray, NativeHashMap, etc. | Free | Yes |
| com.unity.nuget.newtonsoft-json | JSON debug dumps for saves | Free | Yes |
| com.unity.probuilder | Grey-box placeholder models | Free | Yes |
| com.unity.test-framework | Unit and play-mode tests | Free | Yes |
| MemoryPack (NuGet) | Fast binary serialization for release saves | Free (MIT) | Yes |
| A\* Pathfinding Project Pro | Pathfinding for visual agents | ~\$100 (Asset Store) | No (post-MVP) |
| DOTween (optional) | UI and camera tweens | Free / ~\$15 Pro | No |
| Odin Inspector (optional) | Editor tooling for data-driven design | ~\$55 (Asset Store) | No |
| FMOD Studio | Audio middleware if mixing demands it | Free indie | No |

**3.2 Coding Standards**

- File-scoped namespaces; one public type per file.

- Nullable reference types enabled project-wide; warnings-as-errors for nullability.

- Immutable structs where possible for sim data; classes only when reference identity matters.

- No static singletons for gameplay state. A lightweight Bootstrap class wires services via constructor injection.

- Allocation budget in Update and tick loops: zero. Use pooled collections and struct enumerators.

- All user-facing strings go through I18n.Get(key). English today; localization tables later.

- All logging through the Log facade with category tags (Sim, UI, Render, Net, Debug).

- Unit-test the simulation (headless, no GameObjects). Play-mode test the presentation layer.

- Conventional commits (feat, fix, chore, refactor, test, docs). Enforced by a lightweight Git hook.

- Pull requests for shared systems; trunk-based commits allowed for early prototypes.

- Naming — Systems vs Services. \`\*System\` names anything that ticks and mutates state (UtilitySystem, CoverageSystem, GrowthSystem, BudgetSystem). \`\*Service\` names stateless utilities and read-only queries (GridService, SaveService, ToolController). Mixing them is a review-blocking issue.

- Deterministic iteration in sim code. NativeHashMap iteration order is unspecified. Sim code iterates either over sorted keys or indexed arrays — never over a hashmap directly. A hot-path \`IterateSorted\<T\>\` helper is in Core.

- No exceptions in the sim tick. Sim code returns Result\<T\>; unexpected invariant violations assert in debug and log-and-continue in release (with a NotificationBus event).

- All events are \`readonly struct\`. Reference-type events cause GC pressure at tick cadence × subscriber count. Structs stay on the stack.

- Presentation never reads WorldState directly. Presentation subscribes to events and maintains a mirror; UI, Tools, and overlays read the mirror.

- Tools never mutate WorldState. A Tool reads the mirror and emits Commands — nothing more.

**3.3 Project Structure (Layered)**

Modules are organized into layers. Dependencies point downward: a layer may depend on layers below it, never above. Each layer is a separate assembly (asmdef), enforcing the rule at compile time.

- Assets/\_CityRise/Code/

  - Core/ — GameConstants, Log, I18n, FeatureFlags, EntityId, RNG, math helpers, coordinate conventions. Depends on: nothing.

  - Content/ — ScriptableObject definitions (BuildingDef, ZoneDef, OrdinanceDef, PloppableDef) and category registries (BuildingsRegistry, ZonesRegistry, OrdinancesRegistry, PloppablesRegistry, UnlocksRegistry). Depends on: Core.

  - Simulation/ — WorldState, Commands, Events, CommandBus, EventBus, TickScheduler, and all sim services (GridService, TerrainService, RoadNetwork, ZoneService, GrowthEngine, PloppableService, PowerNetwork, WaterNetwork, BudgetService). Zero Unity scene references. Depends on: Core, Content.

  - Persistence/ — Save manifest, ISaveable, MemoryPack adapters, JSON debug dumper, schema migrations. Depends on: Core, Content, Simulation.

  - Presentation/ — Mesh generators, overlay system, building/road presenters, VFX. Subscribes to sim events. Depends on: Core, Content, Simulation.

  - UI/ — UI Toolkit documents, stylesheets, panel controllers. Depends on: Core, Content, Simulation, Presentation.

  - Tools/ — Placement tools, terraform brush tool, road tool, zoning brush, ploppable tool. Input → Command. Depends on: Core, Content, Simulation, Presentation, UI.

  - App/ — Bootstrap (composition root), scene management, game modes. Depends on: everything below.

  - Debug/ — Debug console, editor tooling, state dumpers. Conditionally compiled for release builds. Depends on: all.

  - Tests/ — NUnit (edit-mode) tests on Simulation; play-mode tests on Presentation. Depends on: what it tests.

**4. Architecture**

**4.1 Architectural Layers**

The layering above translates to a strict dataflow:

- Input events (mouse, keyboard) are captured by Tools.

- Tools translate input into Commands (e.g., PaintZoneCommand, PlaceRoadSegmentCommand).

- Commands are dispatched to the CommandBus. The Simulation validates and applies each Command atomically, mutating WorldState.

- The Simulation publishes Events (e.g., BuildingSpawned, RoadAdded, BudgetUpdated) to the EventBus.

- The Presentation layer subscribes to Events and updates the scene (spawns GameObjects, regenerates meshes, updates overlays).

- The UI layer subscribes to Events for display and dispatches Commands in response to user interaction.

This single dataflow rule — Commands in, Events out — makes every system testable, replayable, and inherently undo-aware.

**Presentation mirror — never reach into WorldState**

Presentation, UI, and Tools never read WorldState directly. Instead, the Presentation layer maintains a mirror: a set of plain data structures updated by Event handlers. Overlays, panels, tooltips, and Tools all read the mirror.

- Decouples presentation from WorldState's internal layout — WorldState can be chunked, swapped (replay), or network-synced later without rewriting presentation code.

- Prevents dangling references when NativeArrays are disposed and reallocated on map resize.

- Keeps the dependency direction clean: Simulation → Event → Presentation mirror; never Presentation → Simulation.

**Tools-as-reducers**

A Tool is an input reducer: it reads the presentation mirror, interprets input (clicks, drags, modifier keys), and produces Commands. A Tool does not touch WorldState, Simulation internals, or Presentation internals. This keeps input handling the narrowest possible layer and makes every Tool trivially testable by feeding it synthetic input events.

**4.2 WorldState — the single source of truth**

WorldState is a C# class that owns every mutable simulation field. It is the only object whose fields change during the sim tick. Services are (mostly) stateless operators that read from and write to WorldState.

Shape:

- GridState: tile NativeArrays (elevation, terrainType, zoneType, desirability, pollution, powerCoverage, waterCoverage, serviceCoverage\[\]).

- RoadState: nodes, segments, tile→segment adjacency maps.

- BuildingState: spawned buildings indexed by EntityId; per-building data (footprint, zone, population, wealth).

- PloppableState: placed ploppables indexed by EntityId.

- UtilityState: power graph, water graph, per-component supply/demand snapshots.

- BudgetState: balance, tax rates, last month's income/expense breakdown.

- GameState: seed, sim-time, unlock tier, active ordinances.

Benefits: save/load is serializing WorldState. Unit tests construct a WorldState, apply a Command, and assert on fields. The debug console dumps WorldState as JSON on command. Rollback is memcpy.

**Read and Mutate views — compile-time enforcement**

WorldState exposes two interfaces. Everything in the codebase that needs sim data receives one of them, not WorldState itself:

- IWorldRead — pure queries. Handed to UI, Presentation, Tools, overlays, debug inspector. No mutation API exists on this type.

- IWorldMutate — strictly for Systems during their tick method. Passed in via the ITickStep.Run(IWorldMutate, …) signature and goes out of scope after the tick step returns.

"Only Systems mutate WorldState" becomes a compile-time invariant, not a convention. A UI bug cannot corrupt sim state because the UI literally does not have a reference that can mutate.

**Sim invariants**

Each System declares its invariants — rules that must always hold about the subset of WorldState it owns. Example invariants:

- ZoneService: a tile with zoneType ≠ None has a valid road adjacency.

- RoadNetwork: every segment has exactly two endpoint nodes, both non-null, both present in the nodes table.

- BuildingState: building.entityId is unique; building.zoneTile has zoneType consistent with building's zone category.

In debug builds, an IntegrityChecker runs after every tick and asserts each System's invariants. Violations assert in editor, log as errors in debug player, and are silent (but logged) in release. Turning the checker on costs ~1 ms/tick at 500 pop and catches mutation-ordering bugs the moment they appear.

**4.3 Commands and Events**

A Command is a struct describing an intent to change WorldState. Every player action is a Command.

Examples:

- TerraformCommand { brush, center, radius, strength }

- PlaceRoadSegmentCommand { start, end, curveControlPoints }

- PaintZoneCommand { zoneType, tileRect }

- PlacePloppableCommand { ploppableDefId, position, rotation }

- SetTaxRateCommand { zoneType, rate }

- PassOrdinanceCommand { ordinanceDefId, enabled }

Each Command implements a single Apply(WorldState, out CommandRecord) → Result\<Unit\> method. Validation happens inside Apply, against the live WorldState, at the moment of application. There is no separate Validate phase — eliminating the time-of-check-to-time-of-use race where a command is validated against a world that mutates before Apply runs.

The CommandBus serializes command application on the main thread, so intervening mutations are impossible between check and apply. On failure, Apply returns a Result carrying a typed reason (InvalidPlacement, InsufficientFunds, BlockedByTerrain, etc.); the NotificationBus surfaces user-facing reasons to the UI. On success, Apply emits a CommandRecord containing the inverse command for undo and any events that should be published.

Events are past-tense facts broadcast after state changed. Examples: BuildingSpawned, BuildingAbandoned, RoadAdded, RoadRemoved, CoverageChanged, BudgetUpdated. Events are pushed to a queue during the sim tick and flushed at tick end so the presenter never observes torn state. Replay is the natural corollary: record (initialSeed, initialState, commandLog) and you can rerun any session deterministically — valuable for bug reproduction and for post-MVP marketing captures.

**4.4 Simulation Systems and Services**

A System ticks and mutates WorldState. A Service is stateless — it exposes queries or wraps I/O. The naming is strict.

**Systems (tick-driven, mutate WorldState)**

- **UtilitySystem.** Runs the GraphSolver for power and water each sim tick; updates per-tile coverage flags.

- **CoverageSystem.** Recomputes service-coverage maps from placed ploppables; reads PloppableDef.CoverageContributions.

- **DesirabilitySystem.** Runs the desirability factor pipeline (Section 4.8) per tile.

- **GrowthSystem.** Statistical growth on the 0.1 Hz tick; evaluates, spawns, abandons.

- **BudgetSystem.** Monthly aggregation; bankruptcy enforcement.

- **OrdinanceSystem.** Applies the active ordinance modifier stack to Desirability and Budget inputs.

**Services (stateless, query- or I/O-oriented)**

- **GridService.** World-to-tile mapping, neighbor iteration, rectangle queries, tile elevation convention.

- **TerrainService.** Chunked heightmap storage and brush application. Queries + I/O; state lives in WorldState.

- **RoadNetwork.** Graph of nodes and Unity-spline-backed segments; computes per-tile adjacency on change. More data structure than service, but queried by many systems.

- **EntitySpawner.** Shared spawn/despawn lifecycle for buildings and ploppables: reserve footprint, assign EntityId, add to WorldState, emit SpawnedEvent, presenter instantiates.

- **GraphSolver.** Generic connected-components + supply/demand + reachability. Used by UtilitySystem via PowerNetwork and WaterNetwork adapters.

- **SaveService.** Save manifest orchestration; ISaveable implementations per system.

- **ContentRegistry.** Composes the category registries (BuildingsRegistry, ZonesRegistry, OrdinancesRegistry, PloppablesRegistry, UnlocksRegistry). Addressable-loaded; supports editor hot-reload in dev builds.

**Infrastructure**

- **TickScheduler.** Three clocks: sim (1 Hz), growth (0.1 Hz), budget (monthly). Speed multiplier scales all three. Sim time is authoritative.

- **CommandBus.** Serial Apply-only dispatch; Result return values; undo/redo stacks.

- **EventBus.** Publish-subscribe, no reflection. Queued during ticks; flushed at tick boundary.

- **NotificationBus.** User-facing messages derived from command rejections and sim events ("Insufficient funds", "No road adjacency"). UI subscribes.

- **TickMetrics.** Per-system ms/tick accumulator; Profiler markers. Compared against declared budgets at phase close.

**4.5 Simulation Tick Pipeline**

The tick pipeline is a composable list of ITickStep objects registered with the TickScheduler at Bootstrap. Reordering steps, inserting a debug probe, or A/B-testing a System is a Bootstrap change, not a scheduler change. Each ITickStep has: Name, Budget (ms), Run(IWorldMutate, TickContext) → Result. TickMetrics times every step.

Intra-tick communication rule: within a single tick, Systems communicate through WorldState only. Events are cross-tick. A step can rely on prior steps' mutations via WorldState but must not publish-and-subscribe within the same tick — that reintroduces ordering ambiguity the explicit step list exists to eliminate.

The order of operations per tick is explicit. Changing it is a code review event.

**Sim tick (1 Hz at 1x speed)**

- 1\. CommandBus.DrainQueue — apply queued commands to WorldState (serial, Result-returning).

- 2\. UtilitySystem.Solve — power and water graph solve; update per-tile coverage flags.

- 3\. CoverageSystem.Recompute — service coverage maps from placed ploppables.

- 4\. OrdinanceSystem.ApplyModifiers — layer active ordinance modifiers onto desirability/budget inputs.

- 5\. DesirabilitySystem.Update — per-tile desirability via the factor pipeline (Section 4.8).

- 6\. TickMetrics.Record — capture per-system timings; compare to budget.

- 7\. EventBus.Flush — publish queued events to subscribers.

**Growth tick (0.1 Hz — every 10 sim ticks)**

- 1\. GrowthSystem.Evaluate — per zoned tile, compute target population/jobs.

- 2\. GrowthSystem.ApplySpawns — via EntitySpawner; emit BuildingSpawned.

- 3\. GrowthSystem.ApplyAbandons — via EntitySpawner; emit BuildingAbandoned.

**Budget tick (monthly)**

- 1\. BudgetSystem.CollectTaxes — per zone type × effective rate × population.

- 2\. BudgetSystem.PayUpkeep — ploppable and plant upkeep, ordinance costs.

- 3\. BudgetSystem.ApplyNet — update balance, broadcast BudgetUpdated, check bankruptcy.

**4.6 Save/Load Model**

- Save manifest: ordered list of (subsystemId, schemaVersion). Saves and loads in manifest order.

- Each subsystem implements ISaveable with Serialize(IWriter) and Deserialize(IReader, int fromVersion).

- Migration functions keyed to (subsystemId, fromVersion, toVersion). Missing migration = refuse to load.

- Binary via MemoryPack; JSON dump produced in dev builds for diffable saves.

- Autosave every 5 in-game minutes to a rotating 3-slot ring; quicksave and manual saves separate.

- Atomic save writes: components save to temp file, then atomic rename on success.

**Worked migration example**

RoadNetwork v1 stores segments as { start, end, controls\[4\] }. For v2 we add a \`lanes\` field (default 2). The migration function registered for (RoadNetwork, 1 → 2) reads each segment, writes the three existing fields plus lanes = 2, and returns. Migrations are pure functions on byte-stream readers/writers, unit-tested in isolation, and composed transitively by the load path (v1 → v2 → v3 → current) if a save skips multiple versions.

**4.7 Shared Abstractions**

- **GraphSolver.** One generic graph algorithm module. Computes connected components, supply-vs-demand per component, and reachability. PowerNetwork and WaterNetwork are ~50-line adapters that feed different graph sources into the same solver.

- **OverlaySystem.** Renders a single compositing overlay texture from pluggable per-tile samplers. Each overlay (zoning, pollution, desirability, power, water, coverage) is one sampler function plus a color gradient. Toggle overlays in one place.

- **Category Registries.** Per-content-type ScriptableObjects (BuildingsRegistry, ZonesRegistry, OrdinancesRegistry, PloppablesRegistry, UnlocksRegistry) each holding a list of definitions. A thin ContentRegistry composes them. Splits by content type so team members don't step on each other's edits and each category aligns to an Addressables group.

- **Tool plug-ins.** Each Tool is a small class registered with the ToolController. Implements Activate, Update(input), Deactivate, and produces Commands. New tool = new class, no input wiring changes.

- **EntitySpawner.** One module unifies every spawn/despawn lifecycle. Reserve footprint → assign EntityId → insert into WorldState → emit SpawnedEvent → presenter instantiates. Buildings (from growth), ploppables (from player action), and future mass-transit entities all route through it.

- **SpatialIndex.** Uniform-grid index over the map (cell size 32 m). Ploppables, road nodes, and any positioned entity register; queries like "within R of point P" and "intersecting rect" are O(1) per cell rather than O(n) over all entities. Used by CoverageSystem, placement validation, click-picking, and overlay sampling.

- **IContentRegistry (interface).** ContentRegistry is an interface, not a concrete class. Production uses AddressablesContentRegistry; tests use InMemoryContentRegistry constructed with hand-built definitions. Keeps unit tests fast and Addressables-free.

**4.8 Data-Driven Content Semantics**

Content is data, not code. Three systems historically tempt hardcoding — ordinances, coverage, and desirability — and they are explicitly modelled as data here so the "data-driven" pillar is actually honored.

**Ordinances as modifier lists**

An OrdinanceDef holds a list of StatModifiers — tuples of { target, scope, op, amount } such as { Desirability, AllResidential, Add, +5 } or { BudgetMultiplier, Global, Mul, 0.98 }. Passing an ordinance pushes modifiers onto OrdinanceSystem's active stack; repealing pops them. Adding a new ordinance is a new asset; no C# required unless a genuinely new \*kind\* of effect is introduced.

**Coverage contributions on PloppableDef**

A PloppableDef holds CoverageContributions: a list of { serviceId, radius, falloff, strength }. A fire station has a FireCoverage contribution; a hospital would have a HealthCoverage contribution; a combined clinic could have both. CoverageSystem iterates contributions without knowing which services exist, so adding healthcare or education later is pure data.

**Desirability factor pipeline**

DesirabilitySystem runs a registered ordered list of DesirabilityFactor entries. Each factor is a small named function with a tuner-visible weight: RoadProximityFactor, CoverageFactor, PollutionFactor, OrdinanceFactor, DensityCapFactor. Balance passes live in a DesirabilityConfig ScriptableObject, not in code. Factors can be added or re-ordered via asset edits.

**Mod support scope**

Content mods (new buildings, zones, ordinances, ploppables) are supported via Addressables + category registries. Gameplay-rule mods (custom growth math, custom pathfinding, custom systems) require a scripting host (Lua, C# mod assemblies) and are explicitly out of scope for V1. Stated so the design doesn't accidentally drift toward a general scripting API.

**4.9 Cross-Cutting Concerns**

**Error handling**

- Sim code returns Result\<T\> — never throws on expected failure.

- Commands return Result\<Unit\>; rejection reasons are typed (enum CommandRejectionReason + optional detail).

- Unexpected invariant violations assert in debug builds, log-and-continue in release; NotificationBus surfaces to UI and crash reporter.

- Async I/O (addressables, saves) returns Task\<Result\<T\>\> with structured timeout + retry where relevant.

**Accessibility (Phase 0 stub, grown over time)**

- AccessibilityService exposes: ColorblindPalette (deuteranopia, protanopia, tritanopia, none), UiScale (0.8×–1.5×), ReducedMotion toggle, RemappableInput.

- Overlays consume ColorblindPalette automatically; hardcoded overlay colors forbidden.

- All UI text sizes derive from UiScale × a base size token.

**Analytics (facade only for MVP)**

- Analytics.Track(eventName, payload) facade with a NullSink backend for MVP.

- Swappable backend for the commercial phase (Unity Analytics, self-hosted, or other). No changes to call sites.

- Event taxonomy documented in /docs/analytics-events.md as events are added.

**Tile elevation convention**

The heightmap is 4 m vertex spacing; the sim grid is 8 m tiles. Each tile covers a 2×2 patch of heightmap vertices (4 corner samples plus an implicit center). Convention: Tile.elevation = arithmetic mean of the 4 corner heightmap samples. Zoning, growth, and ploppable validation reference Tile.elevation; Tile.slope (max corner Δ) is a separate field.

**Deterministic iteration in sim code**

- Sim code never iterates a NativeHashMap directly.

- IterateSorted\<TKey, TValue\>(NativeHashMap) helper returns deterministic order via a sorted key array.

- Indexed arrays are preferred where possible; hashmaps only when lookup is dominant.

- RNG draws are always through the seeded RNG service; System.Random and UnityEngine.Random are forbidden in sim code.

**Content hot-reload (dev-only)**

- In editor, changes to ScriptableObject content definitions trigger a ContentRegistry.Rebuild() via an AssetPostprocessor hook; systems subscribe and re-cache.

- Stripped from release builds.

- During reload, ContentRegistry.IsReloading = true and the TickScheduler skips ticks until reload completes to prevent mid-swap desync.

**Standard debugging workflow**

The same workflow applies to every bug, and the tooling exists from Phase 1 to support it:

- 1\. Obtain the failing save (from playtester report, crash dump, or reproduction steps).

- 2\. Load in editor with \`debug_load \<save\>\` command.

- 3\. Enable integrity checker, event log, and command log via debug console.

- 4\. Step ticks manually with \`tick_once\` until the bug manifests.

- 5\. \`dump_state\` writes WorldState as JSON; diff against previous tick's dump to identify the offending system.

- 6\. Add a unit test against the minimal WorldState that reproduces the bug; fix in the System; unit test now passes; regression prevented.

- 7\. Capture and commit a golden replay that would catch a regression of this bug.

**4.10 Replay and Debug Affordances**

Because sim state is one WorldState object and every player intent is a serializable Command, a session recording is just (initialSeed, initialWorldSnapshot, orderedCommandLog). Replay is effectively free.

- Replay recorder: writes commands to a ring buffer with timestamps (sim-ticks); dumped on debug command or on crash.

- Replay player: loads an initial snapshot, re-applies commands in order, asserts equality on final WorldState hash.

- Uses: reproducing bugs from playtester reports, regression testing (a golden replay must produce the golden final hash), marketing captures (record player session, replay, screen-record at 4K).

- Cost: a few hundred lines in Phase 1 to stand up the recorder; player is one debug command. Budget one day.

**5. Key Technical Decisions**

**5.1 Terrain — Custom Chunked Heightmap**

Three options were evaluated:

**Option A — Unity Built-in Terrain**

|  |  |
|----|----|
| **Pros** | **Cons** |
| Painting, LOD, colliders, trees, grass all built-in. | Single-terrain assumption; large maps require multiple terrains with seam artifacts. |
| Editor sculpting tools out of the box. | Runtime modification is slow — modifying heights rebuilds large internal structures. |
| Well-known among Unity devs. | Fixed heightmap resolution is awkward to align with a custom 8 m sim grid. |
| Hides mesh details behind a black box. | URP custom shader integration (for stylized flat-shaded look) is painful. |
|  | Brings overhead for features CityRise doesn't use (trees, grass, detail objects). |

**Option B — Custom Chunked Heightmap (Chosen)**

|  |  |
|----|----|
| **Pros** | **Cons** |
| Vertex spacing aligns exactly with the sim grid (4 m vertex, 8 m tile). | 3–4 weeks of upfront engineering. |
| Only dirty chunks regenerate on edit — fast terraforming. | We implement LOD, normals, and collider mesh ourselves. |
| Burst + Jobs compilable — very fast on large edits. | Edge cases around chunk seams (normals, gaps). |
| Tight shader control for stylized flat-shaded terrain. | Less tutorials / community help than Unity Terrain. |
| Predictable memory footprint; trivial save/load (one float array). |  |
| Simplified because low-poly needs no texture splatting — colors come from per-vertex tint or flat per-face material keyed to terrain type. |  |

**Option C — Unity Terrain + adapter layer**

|  |  |
|----|----|
| **Pros** | **Cons** |
| Faster to get moving initially. | Adapter layer is rework piled on Unity's opaque internals. |
| Retains some built-in features. | Still hits the runtime-edit slowness problem. |
|  | Harder URP custom shader integration. |

Decision: Option B (custom chunked heightmap). Low-poly stylized + grid-aligned + edit-heavy tips the balance clearly. The plan is further simplified by skipping texture splatting — a meaningful shortcut unique to the stylized direction.

**5.2 Roads — Unity Splines (Chosen) vs Polyarc**

Three approaches were evaluated:

**Option A — Strict grid tile roads**

Roads snap to 8 m cells with fixed orientations; meshes are stamped prefabs. Very simple but no curves; reads as a dated budget title. Rejected.

**Option B — Polyarc (straight + circular arc segments)**

Every segment is a straight line or a circular arc. Closed-form math for length, tangents, and point-at-distance. Mesh extrusion is trivial. Matches how real roads are actually built. The most mathematically elegant option specifically for roads. Custom code, smaller Unity ecosystem.

**Option C — Unity Splines package (Chosen)**

Engine-supplied Bezier splines with runtime mutation, tangent handles, sampling APIs, and mesh extrusion utilities. Saves weeks of work; leverages a well-tested, well-maintained package; industry-standard for Unity road tools. Costs: Bezier is slightly less elegant than polyarc, and curvature continuity at node junctions requires explicit tangent alignment.

Decision: Option C (Unity Splines). The best code is the code you don't write. Polyarc remains a fallback if Unity Splines proves constraining during Phase 4b; migration would be contained to RoadNetwork and RoadMeshGenerator.

**6. Phased Implementation Roadmap**

Phases are sequential. Each phase has a clear playable deliverable and leaves main green. Each phase ends with a phase-close day: integration test, regression run, tag commit. Effort estimates assume a solo programmer working consistently.

**6.1 Phase Summary**

|  |  |  |  |
|----|----|----|----|
| **\#** | **Phase** | **Effort** | **What's playable at the end** |
| **0** | **Setup, tech stack, Phase-0 facades** | 1–2 wk | Empty project runs, CI green, Log/I18n/FeatureFlags/EntityId/Bootstrap in, constants populated. |
| **1** | **Core framework** | 2–3 wk | TickScheduler, CommandBus, EventBus, save skeleton, camera, time controls, debug console, UI shell — all working on an empty world. |
| **2** | **Grid and WorldState** | 1 wk | WorldState struct with tile NativeArrays; inspect any tile; grid gizmo overlay. |
| **3** | **Terrain and terraforming** | 3–4 wk | Chunked heightmap; raise/lower/flatten/smooth brushes; water plane; saves and loads. |
| **4a** | **Road graph + primitive mesh** | 1–2 wk | Road graph data with Unity Splines; straight placement + ugly strip mesh; road-tile adjacency working. |
| **4b** | **Road curves, intersections, proper mesh** | 2 wk | Curved roads via Bezier handles; auto intersections; extruded road mesh with sidewalks. |
| **5** | **Zoning** | 1–2 wk | R/C/I paint tool with road-adjacency validation; zone overlay via OverlaySystem. |
| **6** | **Zone growth + placeholder buildings** | 3–4 wk | Growth engine spawns buildings on zoned tiles; population and demand visible. |
| **7** | **Ploppables, service coverage, budget** | 2–3 wk | Fire/police/park affect desirability; budget UI reflects income/upkeep. |
| **8** | **Power utility + GraphSolver** | 2 wk | Coal plant + cables; powered tiles grow, unpowered abandon. |
| **9** | **Water utility** | 1 wk | Pump + pipe-under-roads; water-less tiles stop growing. Short phase because GraphSolver already exists. |
| **10** | **MVP polish + playtest (perf / polish / playtest weeks)** | 3 wk | 20-minute playthrough without crashes; 60 FPS at 500 pop; golden replay passes; first external playtester feedback. |

**6.2 Phase 0 — Setup, Tech Stack, Phase-0 Facades**

Effort: 1–2 weeks. Prerequisites: none.

**Goals**

- Unity 6 LTS project from URP 3D template; all Phase-0 packages installed and pinned.

- Git + LFS configured; .gitignore for Unity; .gitattributes LFS rules for models/textures/audio.

- GitHub Actions CI building Windows on every main merge; build artifact uploaded.

- Layered asmdef structure as in 3.3 (empty layers with placeholder stubs).

- GameConstants populated (tile 8 m, heightmap vertex 4 m, map 2048×2048, tick rates).

- Log facade with category tags.

- I18n.Get(key) facade returning English strings from a single LocalizationTable SO.

- FeatureFlags ScriptableObject with per-phase flags (PowerEnabled, WaterEnabled, AgentsEnabled, …) all off by default.

- EntityId struct (Guid wrapper) with factory and equality.

- Bootstrap class in the Boot scene: instantiates Simulation, wires services, installs debug console, loads Main scene.

- Coordinate conventions documented (Unity defaults: Y-up, left-handed, meters; tile (0,0) at world origin; forward +Z).

- AccessibilityService stub (ColorblindPalette, UiScale, ReducedMotion, remappable input).

- Analytics.Track facade with NullSink.

- Result\<T\> type and NotificationBus in Core.

- IterateSorted helper in Core.

- ADR folder (/docs/adr/) with 0001-engine-choice.md, 0002-terrain-custom-chunked.md, 0003-roads-unity-splines.md seeded from Section 5 of this document.

- Initial performance budget doc (/docs/perf-budget.md) listing placeholder ms/tick targets per system.

- README, CLAUDE.md (for AI assistants), CONTRIBUTING.md (for humans).

- Phase close: tag v0.0.1-setup.

**6.3 Phase 1 — Core Framework**

Effort: 2–3 weeks. Prerequisites: Phase 0.

**Goals**

- TickScheduler: sim (1 Hz), growth (0.1 Hz), budget (monthly). Speed multiplier; sim time as authoritative clock.

- CommandBus: Command base type with Validate/Apply; undo/redo stacks; rejection events.

- EventBus: queued pub/sub; flush at tick boundary; no reflection.

- Persistence skeleton: ISaveable, save manifest, schema-version header, MemoryPack binary container, JSON debug dumper (dev-only).

- Cinemachine RTS camera: pan, zoom, orbit, edge-pan; pleasant feel.

- Time-control UI in UI Toolkit: pause, 1x, 2x, 3x; hotkeys wired.

- Debug console: tilde to open, attribute-registered commands, autocomplete; starters (dump_state, set_tick_rate, teleport_camera, run_commands_from_file).

- UI Toolkit shell: HUD root, top bar, bottom toolbar placeholder, right panel placeholder.

- TickMetrics service with Profiler.BeginSample/EndSample markers around every system's tick method.

- Performance budget baseline: each system is assigned a soft ms/tick target in /docs/perf-budget.md; TickMetrics prints a warning when exceeded.

- Replay recorder scaffolding: write commands to a ring buffer; dump command log on debug command or crash. Replay player left for Phase 2+.

- Phase close: save/load round-trips camera + time speed; perf budget green on an empty world.

**6.4 Phase 2 — Grid and WorldState**

Effort: 1 week. Prerequisites: Phase 1.

Why early: Grid is pure math and defines the substrate the terrain sits on top of. Building grid first means terrain chunks and tile data never have to be reconciled.

**Goals**

- WorldState class scaffolded with GridState; other state containers stubbed.

- GridState holds tile SoA NativeArrays: elevation, terrainType, zoneType, densityCap, desirability, pollution, powerCoverage, waterCoverage, serviceCoverage\[\].

- GridService: world-to-tile and tile-to-world, neighbor iteration, rectangle queries.

- TileInspector debug panel: click any tile, see all fields, values update live.

- Tile highlight system (hover + selection).

- Grid gizmo overlay toggle in the debug console.

**6.5 Phase 3 — Terrain and Terraforming**

Effort: 3–4 weeks. Prerequisites: Phase 2.

**Goals**

- Chunked heightmap terrain: chunks of 32×32 vertices at 4 m spacing (128 m per chunk); 16×16 chunks = 2048 m map.

- Terrain mesh generated per chunk; Burst-compiled; dirty-flag regen (no full-map rebuild).

- Flat-shaded material; per-face color keyed to terrain type (grass, dirt, sand, rock, snow).

- Brushes as Tools: RaiseBrush, LowerBrush, FlattenBrush, SmoothBrush, LevelToWaterBrush. Each emits TerraformCommand.

- Command-based undo (50-command ring).

- Water plane at y=0; simple animated shader.

- Mesh colliders regenerate on dirty chunks.

- Debug overlays: chunk boundaries, heightmap visualization.

- Terrain save/load via ISaveable.

- Map generator: a small procedural generator (perlin noise + water carving) produces a starting terrain with varied elevation and a river. ~200 lines. Provides realistic testing substrate for terraforming, road-over-slope, and water-edge placement — flat-plane testing hides these cases.

**6.6 Phase 4a — Road Graph + Primitive Mesh**

Effort: 1–2 weeks. Prerequisites: Phase 3.

Why split: the road system is the riskiest single feature in the plan. 4a de-risks by shipping the data model and adjacency working, with an intentionally ugly mesh that Phase 4b replaces.

**Goals**

- Road data model using Unity Splines: RoadNode (position, tangent, connected segments) + RoadSegment wrapping a SplineContainer.

- Placement tool: straight and cheap-curve modes. Straight = click start, click end. Curve = click start, click end, automatic midpoint offset perpendicular to the straight line (magnitude = 10% of length). Functional, not pretty.

- Endpoint snapping: 8 m grid; snap-to-existing-node creates a junction.

- Primitive mesh: a simple extruded strip from the spline with a flat asphalt color. Temporary by design; Phase 4b replaces.

- Road-tile adjacency: tiles within 4 cells of any spline sample record their nearest segment.

- PlaceRoadSegmentCommand, RemoveRoadSegmentCommand implemented.

- Road save/load via ISaveable.

- Rationale for cheap curves: shipping 4a with only straight roads would force Phase 5 (zoning) to test on a grid-only city, hiding real bugs. Cheap curves make Phase 5 realistic without the complexity of curve authoring UX.

**6.7 Phase 4b — Road Curves, Intersections, Final Mesh**

Effort: 2 weeks. Prerequisites: Phase 4a.

**Goals**

- Curved placement mode: click endpoints, drag to set Bezier tangent handles.

- Tangent continuity at shared nodes (both adjacent segments agree on the tangent to avoid visual kinks).

- Intersection mesh generation: flat caps at nodes with more than two connections; simple road-road merge at two-way nodes.

- Extruded road mesh with sidewalks and lane markings.

- Roads conform to terrain: sample heightmap under the spline and vertically offset.

- Phase close: roads look good enough to screenshot.

**6.8 Phase 5 — Zoning**

Effort: 1–2 weeks. Prerequisites: Phase 4b.

**Goals**

- Zone paint tool (Tools layer) emitting PaintZoneCommand. Brush with variable size.

- Validation: a tile is zonable only if within 4 tiles (32 m) of a road segment.

- OverlaySystem zoning overlay (green R, blue C, yellow I). First real consumer of OverlaySystem.

- Zone removal tool.

- Zone state in GridState; save/load via ISaveable.

**6.9 Phase 6 — Zone Growth and Placeholder Buildings**

Effort: 3–4 weeks. Prerequisites: Phase 5.

**Goals**

- Global demand model per R/C/I (simple: jobs vs population vs tax).

- Per-tile desirability function combining coverage (stubbed), pollution (stubbed), road proximity, zone density cap.

- GrowthEngine runs on the growth tick: evaluate, spawn, abandon (three-stage pipeline per Section 4.5).

- Footprint picker: table keyed by (density, wealth) → list of footprint defs; seeded RNG picks one.

- BuildingDef ScriptableObjects referencing placeholder models provided by the modeler (see Appendix B).

- BuildingPresenter subscribes to BuildingSpawned / BuildingAbandoned events; fade-in, despawn, pool.

- Population and demand visible in HUD top bar (via I18n keys).

- Save/load: paint zones, save, reload, continue growing — identical growth trajectory (RNG seed stable).

**6.10 Phase 7 — Ploppables, Service Coverage, Budget**

Effort: 2–3 weeks. Prerequisites: Phase 6.

**Goals**

- Ploppable tool: footprint rectangle, validation (flat ground or auto-flatten at cost), cost preview.

- MVP ploppable set: fire station, police station, small park (via PloppableDef SOs).

- CoverageSystem: per-service per-tile coverage computed on placement; cached in GridState.

- BudgetService: income (population × effective tax) minus expenses (upkeep + plants + ordinance costs) on the monthly tick.

- Budget UI panel: balance, income/expense breakdown, last month delta.

- OverlaySystem: coverage overlays per service type.

**6.11 Phase 8 — Power Utility + GraphSolver**

Effort: 2 weeks. Prerequisites: Phase 7.

**Goals**

- GraphSolver module: connected components + supply/demand per component + reachability. Generic over TSupply.

- PowerPlant ploppable (coal): supply, pollution, upkeep.

- Power cable tool (freestanding cables); power also auto-conducts along roads within a short range.

- PowerNetwork: thin adapter around GraphSolver using the power graph.

- Per-tile powerCoverage flag driven by network state.

- GrowthEngine respects powerCoverage: unpowered tiles don't grow and eventually abandon.

- OverlaySystem: power coverage overlay (reuses existing overlay pattern).

**6.12 Phase 9 — Water Utility**

Effort: 1 week. Prerequisites: Phase 8.

Short phase because GraphSolver already exists. Adding water is mostly configuration and a new ploppable.

**Goals**

- WaterPump ploppable; placement validation (adjacent to water body).

- Pipes travel automatically beneath roads (no separate pipe tool in MVP).

- WaterNetwork: second adapter around GraphSolver.

- Per-tile waterCoverage flag; GrowthEngine requires water for density 2+.

- OverlaySystem: water coverage overlay.

- Sewage stub deferred to post-MVP (pollution-only for now).

**6.13 Phase 10 — MVP Polish and Playtest**

Effort: 3 weeks (structured). Prerequisites: Phase 9.

Structured into three distinct weeks rather than a "polish bag" to absorb the late-stage bug surge predictably.

**Week 1 — Performance**

- Run TickMetrics against the full MVP on a 500-pop city; compare to budgets.

- Unity Profiler + Deep Profile; fix the top three CPU hotspots.

- GPU instancing audit for building meshes; shared material audit.

- Memory profile: no leaks over an hour of play; stable working set.

- Target 60 FPS on mid-range PC at 500 pop; 30 FPS floor on minimum spec.

**Week 2 — Polish**

- UI pass: consistent toolbar, tooltips, real icons in place of placeholders.

- Audio pass: placeholder SFX for every tool action; ambient loop; UI clicks.

- Camera feel: smoothing, zoom curves, edge-pan dead zone.

- Notification routing: every CommandRejection has a user-readable message.

- Empty-world bootstrap: a short scripted intro ("welcome mayor") rather than an abrupt blank terrain.

**Week 3 — Playtest and buffer**

- First external playtester: 30-minute blind playthrough; record screen + audio; collect bugs and confusion.

- Save/load stress: every minute for an hour, load each, verify no drift.

- Regression replay: run a golden replay recorded earlier, assert equal final-state hash.

- Buffer days for late-stage bug fixes discovered in playtest.

- Tag MVP build in Git; upload to itch.io as the project's first public trace.

**7. Technical Risks and Open Decisions**

- **Unity Splines runtime mutation.** The package is solid but more commonly used for baked splines. Mitigation: Phase 4a prototype validates runtime mutation early; polyarc fallback noted in Section 5.2.

- **Terrain + road interaction.** Roads should conform to terrain. MVP policy: roads lock terrain underneath; terraforming is blocked inside the road footprint. Revisit if painful.

- **Save schema drift.** Every subsystem owns a schema version; migration functions required before any PR merging a save-format change. The save manifest structure exists from Phase 1.

- **Command-based undo storage cost.** Terraforming commands can carry large diffs. Mitigation: bound undo to 50; commands that would exceed a size budget snapshot the chunk diff once and discard further redo beyond the budget.

- **Determinism drift.** Full determinism is deferred. Until then, all sim iteration uses sorted or indexed containers (enforced via IterateSorted helper); RNG draws always go through the seeded RNG service; System.Random and UnityEngine.Random are forbidden in sim code.

- **Hashmap iteration bugs.** NativeHashMap iteration order is explicitly unspecified in Unity.Collections. A lint rule (Roslyn analyzer or grep in CI) flags direct iteration of NativeHashMap in Simulation assembly.

- **Content hot-reload desync.** Reloading a ScriptableObject mid-play can leave WorldState holding a reference to a half-reloaded def. Mitigation: ContentRegistry sets IsReloading = true before swapping and the TickScheduler skips ticks while the flag is set; systems read from a frozen snapshot taken at tick start.

- **Performance at 500+ pop.** Statistical sim scales; presentation (building meshes) won't without care. GPU instancing for buildings, shared materials, LOD from day one in the kit.

- **Decisions deferred past MVP.** Visual agents (A\* vs NavMesh), audio middleware (FMOD), full determinism, Steam Workshop plumbing.

**8. Appendix A — First Actionable Engineering Tasks**

The first week of Phase 0, in order:

- Create GitHub repo CityRise with README, LICENSE, .gitignore (Unity), .gitattributes (LFS for psd/png/jpg/tga/fbx/blend/wav/mp3/ogg).

- Create Unity 6 LTS project from URP 3D template. Commit.

- Install Phase-0 packages from Section 3.1. Pin in Packages/manifest.json. Commit.

- Create layered asmdef structure under Assets/\_CityRise/Code/. Stubs only. Commit.

- Write GameConstants.cs with all locked constants. Commit.

- Write Core facades: Log, I18n, FeatureFlags SO, EntityId, RNG service, Result\<T\>, NotificationBus, IterateSorted, AccessibilityService stub, Analytics.Track facade. Commit.

- Create Boot and Main scenes; Bootstrap class wiring service instances. Commit.

- Add GitHub Actions workflow (Unity Windows headless build). Get CI green. Commit.

- Create /docs/adr/ with 0001-engine-choice.md, 0002-terrain-custom-chunked.md, 0003-roads-unity-splines.md seeded from Section 5. Commit.

- Create /docs/perf-budget.md with placeholder per-system ms/tick targets. Commit.

- Create /docs/analytics-events.md (empty table with columns: eventName, payload, added-in-phase). Commit.

- Write README, CLAUDE.md, CONTRIBUTING.md. Commit.

- Tag v0.0.1-setup. Merge to main.

**9. Appendix B — Placeholder Building Checklist (MVP)**

The list below is what the modeler must have delivered before Phase 6 begins. Minimum column is the absolute floor; Target column is the number needed for decent visual variety. Beyond the list is not required for MVP.

**9.1 Zoned (grown) buildings — low density only in MVP**

|  |  |  |  |  |
|----|----|----|----|----|
| **Model** | **Min** | **Target** | **Footprint** | **Notes** |
| **Residential Low** | 3 | 5 | 2×2 (16×16 m) | Small houses / duplexes; vary rooflines and colors. |
| **Commercial Low** | 3 | 5 | 2×2 (16×16 m) | Corner shops, small cafés; front-facing door. |
| **Industrial Low** | 3 | 5 | 2×2 (16×16 m) | Small workshops, warehouses; simple roll-up doors OK. |

**9.2 Ploppables — MVP set**

|  |  |  |  |  |
|----|----|----|----|----|
| **Model** | **Min** | **Target** | **Footprint** | **Notes** |
| **Fire Station** | 1 | 1 | 3×2 (24×16 m) | Bay door on front face; red tint. |
| **Police Station** | 1 | 1 | 3×2 (24×16 m) | Blue tint; small yard. |
| **Small Park** | 2 | 3 | 1×1 or 2×2 | Variants: grass-only, trees, plaza with bench. |

**9.3 Utilities — MVP set**

|  |  |  |  |  |
|----|----|----|----|----|
| **Model** | **Min** | **Target** | **Footprint** | **Notes** |
| **Coal Power Plant** | 1 | 1 | 6×4 (48×32 m) | Smokestack + main building; large footprint. |
| **Water Pump** | 1 | 1 | 2×2 (16×16 m) | Must look at home next to water; small pump-house silhouette. |
| **Power Pole / Pylon** | 1 | 1 | 1×1 sub-tile | Spawned at runtime along cable paths; slim silhouette. |

**9.4 Technical requirements — all placeholder models**

- Format: FBX (preferred) or glTF 2.0.

- Pivot: centered horizontally on the footprint, Y=0 at the ground plane.

- Orientation: forward face = +Z (Unity default).

- Footprint: multiples of the 8 m cell size (2×2, 3×2, etc.).

- LODs: LOD0 + LOD1 minimum (LOD1 may be extremely simplified for placeholder stage).

- Materials: flat-shaded or soft cel-shaded; minimal or zero textures; colors via vertex tint or simple albedo.

- Shared materials across buildings where possible (draw-call reduction).

- Polygon budget: \<= 500 tris for small zoned, \<= 2000 for ploppables, \<= 5000 for the coal plant. LOD1 roughly half of LOD0.

- Naming: \<Category\>\_\<Subtype\>\_\<Variant\>\_LOD\<n\>.fbx — e.g., R_L_01_LOD0.fbx, FireStation_01_LOD0.fbx, CoalPlant_01_LOD0.fbx.

- Scale: 1 Unity unit = 1 meter. No post-import scaling required.

- Clean geometry: no overlapping faces, no non-manifold edges, normals consistent (outward-facing).

**9.5 Not required for MVP (post-MVP list)**

- Medium and high density R / C / I.

- Hospital, school (elementary/high/university), landmark, recycling plant, garbage dump.

- Wind, solar, nuclear power plants.

- Mass transit (bus stop, metro station, depot).

- District-specific cosmetic variants.

- Vehicles, pedestrians, props — these come with the Alpha phase once visual agents are in.

**9.6 Minimum acceptance check**

For each delivered model, verify:

- Imports into Unity with correct scale and pivot (drop into scene; no rotation, no scale fix needed).

- Footprint matches the declared cell count.

- Materials render correctly under URP (no missing-material magenta).

- LODs switch at reasonable distance (manually test in scene).

- Naming matches the convention above (so the import pipeline can parse).

- No references to engine-internal assets (e.g., Standard materials, missing textures).

**10. Appendix C — Performance Budget (Initial Targets)**

Each system is assigned a soft CPU budget per tick at 500-population target. TickMetrics records actual ms/tick; exceeding the budget at phase close is a review-blocking issue. These numbers are starting points; revise as real profiling data comes in.

|                                                  |                      |
|--------------------------------------------------|----------------------|
| **System**                                       | Budget at 500 pop    |
| **UtilitySystem.Solve (power + water combined)** | ≤ 2.0 ms/sim tick    |
| **CoverageSystem.Recompute**                     | ≤ 1.0 ms/sim tick    |
| **DesirabilitySystem.Update**                    | ≤ 1.5 ms/sim tick    |
| **OrdinanceSystem.ApplyModifiers**               | ≤ 0.2 ms/sim tick    |
| **CommandBus.DrainQueue**                        | ≤ 0.5 ms/sim tick    |
| **EventBus.Flush**                               | ≤ 0.3 ms/sim tick    |
| **GrowthSystem (entire growth tick)**            | ≤ 4.0 ms/growth tick |
| **BudgetSystem (monthly tick)**                  | ≤ 2.0 ms/budget tick |
| **Main-thread frame time (presentation)**        | ≤ 16.6 ms (60 FPS)   |
| **Memory — WorldState (500 pop city)**           | ≤ 50 MB              |
| **Memory — Presentation (buildings + meshes)**   | ≤ 200 MB             |
| **Save file size (500 pop city)**                | ≤ 5 MB binary        |
| **Save duration (500 pop city)**                 | ≤ 1 second           |
| **Load duration (500 pop city)**                 | ≤ 3 seconds          |

Totals: sim tick targets sum to ~5.5 ms, leaving ~11 ms of the 16.6 ms frame for presentation and input — a comfortable margin. Revisit after Phase 6 and Phase 10 profiling.

**11. Appendix D — Architecture Decision Record Template**

Every ADR lives at /docs/adr/NNNN-kebab-case-title.md and follows this structure:

\# NNNN. Title

Date: YYYY-MM-DD

Status: Proposed \| Accepted \| Superseded by ADR-XXXX

\## Context

What problem are we solving? What constraints are in play?

\## Decision

What we're doing and why.

\## Alternatives considered

Options we evaluated and why we didn't pick them.

\## Consequences

Positive, negative, and neutral outcomes of this decision.

Initial ADRs seeded in Phase 0:

- 0001-engine-choice.md — Unity 6 LTS

- 0002-terrain-custom-chunked.md — Custom heightmap over Unity Terrain

- 0003-roads-unity-splines.md — Unity Splines over polyarc

- 0004-hybrid-simulation.md — Statistical core + visual agent layer

- 0005-apply-only-commands.md — Apply-only dispatch with Result (v0.3 change)

- 0006-data-driven-content.md — Ordinances/coverage/desirability as modifier lists (v0.3 change)

- 0007-worldstate-view-split.md — IWorldRead vs IWorldMutate interfaces (v0.4 change)

- 0008-presentation-mirror.md — Presentation never reads WorldState directly (v0.4 change)

- 0009-typed-entity-ids.md — EntityId\<T\> phantom-typed IDs (v0.4 change)

**12. Appendix E — Per-Phase Acceptance Tests**

Each phase ends with concrete, verifiable criteria. A phase is not considered complete until every criterion below passes. CI automates what it can; the rest are manual checks logged in the phase-close commit message.

**Phase 0 — Setup**

- A fresh clone builds and runs in Unity 6 LTS without manual setup steps.

- GitHub Actions CI completes a Windows build on main green.

- README, CLAUDE.md, CONTRIBUTING.md exist and are non-trivial.

- ADR folder has the seed ADRs (0001–0004 at minimum).

- Core facades compile and expose the documented APIs (Log, I18n, FeatureFlags, EntityId\<T\>, Result\<T\>, NotificationBus, IterateSorted, Accessibility, Analytics).

- Boot scene loads Main; Bootstrap wires the minimal service list without errors.

- Tagged v0.0.1-setup.

**Phase 1 — Core framework**

- Time controls (pause, 1×, 2×, 3×) respond to keyboard and UI.

- TickScheduler runs sim, growth, and budget clocks at correct scaled rates; verifiable in the debug tick counter.

- Save camera position + time speed; relaunch; identical state loaded.

- TickMetrics reports ms/tick per step in the debug overlay; no step exceeds its budget on an empty world.

- Debug console opens with \`~\`; at least five commands registered; autocomplete works.

- Replay recorder captures a session; exporting produces a valid command log file.

- IntegrityChecker runs without assertion failures in the empty-world scenario.

**Phase 2 — Grid and WorldState**

- Hovering a tile shows its coordinates live.

- Clicking a tile shows all fields in the TileInspector panel.

- WorldState constructor allocates tile NativeArrays; disposal is clean (no leaks in editor tests).

- Grid gizmo overlay toggles via debug console.

- NUnit tests pass for world-to-tile, neighbor iteration, rectangle queries.

**Phase 3 — Terrain and terraforming**

- Launch to a generated terrain with visible elevation and a river.

- Raise/lower/flatten brushes visibly edit terrain; undo reverts in one step.

- Terraform, save, relaunch, reload: terrain exactly matches pre-save state (byte-compare the saved heightmap).

- Mesh collider updates on dirty chunks (test by raycasting onto freshly raised terrain).

- TickMetrics: terrain brush stroke completes inside the per-step budget on a 2048×2048 map.

**Phase 4a — Road graph + primitive mesh**

- Place a straight road of 100 m; mesh renders; tiles within 4 cells of it report adjacency.

- Place a curved road via cheap-curve mode; mesh follows the curve; adjacency works.

- Connect two segments at a shared node; junction rendered as flat cap.

- Remove a segment; adjacency updates; no stale references.

- Save, reload, roads identical.

**Phase 4b — Road curves and final mesh**

- Curve authoring with tangent handles works smoothly.

- Adjacent segments at a shared node show tangent continuity (no visible kinks).

- Roads conform to terrain (sampled vertical offset under the spline).

- Sidewalks and lane markings visible.

- A screenshot of a built road network looks like a road network.

**Phase 5 — Zoning**

- Paint R/C/I zones next to roads; overlay visible.

- Attempting to zone beyond 4 cells from any road is rejected; NotificationBus surfaces the reason.

- Remove zone; overlay clears; WorldState zoneType resets.

- Save, reload, zones identical.

**Phase 6 — Zone growth + placeholder buildings**

- Paint a residential zone with 20 tiles; unpause; within one in-game month, population \> 0 and buildings visibly spawn.

- Commercial demand rises as residential population rises; visible in the demand meter.

- Save mid-growth; reload; growth continues along the identical trajectory (deterministic RNG).

- Building presenter pools GameObjects — no per-spawn allocations visible in Profiler.

- Golden replay: recorded growth session replays to identical final state hash.

**Phase 7 — Ploppables + budget**

- Place a fire station; cost deducted; monthly upkeep appears on the budget panel.

- Coverage overlay shows the station's radius; tiles inside the radius report fire-coverage \> 0.

- Place on uneven ground; auto-flatten runs; cost reflects extra flattening.

- Bankrupt the city by overspending; BudgetSystem emits bankruptcy state; game continues but warnings fire.

- Save + reload preserves ploppable positions and budget.

**Phase 8 — Power**

- Place coal plant; connect cables to a zoned area; tiles report powerCoverage = true.

- Remove a cable; cut-off tiles lose coverage; after N ticks, abandonment begins.

- GraphSolver: a network with mixed supply/demand resolves correctly (unit test with 10 nodes + verified output).

- Power overlay visible and toggleable.

**Phase 9 — Water**

- Place pump adjacent to water; water reaches road-connected tiles; growth to density 2+ allowed.

- Pump placed on dry land rejects with a clear reason.

- Water overlay visible and toggleable.

- Same GraphSolver handles water network (code reuse visible in diff).

**Phase 10 — Polish and playtest**

- End-to-end: new game → terraform → plant + pump → road → zone → plop → grow to ~500 pop with no crash.

- 60 FPS sustained on a mid-range PC at 500 pop.

- 30 FPS floor on minimum spec.

- Save/load stress: 60 save-loads in an hour, no drift in WorldState hash compared to a golden baseline.

- Regression replay passes on final build.

- External playtester completes a 30-minute session and can articulate what they built.

- No P0 or P1 bugs open.

- Uploaded to itch.io; download link works; build runs on a fresh PC without Unity installed.

**13. Appendix F — Commercialization Checklist**

Switches to flip when a commercial release is greenlit (end of Alpha or later). Each item has a specific owner and a blocking check at the release candidate milestone.

- Steamworks SDK integrated; Steam App ID reserved; achievements stub.

- Steam store page created: description, screenshots, trailer, system requirements.

- Steam demo build (if doing a Steam Next Fest pass).

- Steam Input mapping for Steam Deck verification.

- Analytics: swap NullSink for a real backend; event taxonomy reviewed; opt-in UI.

- Crash reporting: swap Log facade backend to Unity Cloud Diagnostics or Sentry; opt-in UI.

- Privacy policy page; data-collection disclosures.

- EULA / Terms of Service drafted.

- LICENSE updated from hobby license (MIT) to commercial source license.

- Asset audit: every third-party asset has a license record in /docs/asset-register.md; no CC-NC or research-only licenses.

- Audio audit: commissioned or licensed music with paperwork.

- Credits screen built from a YAML file; every contributor reviewed.

- Localization pipeline wired (I18n tables for each target language).

- Accessibility pass: colorblind presets verified; UI scale verified; keybinds remappable; subtitles if voice exists.

- Save-compatibility commitment: document which save versions 1.0 will load; migration plan for each.

- Trademark and domain availability checked for the final title.

- Company / publishing entity registered (if applicable for tax/payout reasons).

- Payout accounts set up (Steam Direct, itch.io).

- Press kit: about page, fact sheet, screenshots, trailer, logos, contact info.

- Launch plan: wishlist drive timeline, streamer outreach list, launch-day patch readiness.
