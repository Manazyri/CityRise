# Phase 0 — Setup, Tech Stack, and Phase-0 Facades

**Goal**: A fresh clone builds and runs in Unity 6 LTS without manual setup. CI is green. Core facades (Log, I18n, FeatureFlags, EntityId<T>, Result<T>, NotificationBus, IterateSorted, Accessibility stub, Analytics stub) compile and expose the documented APIs. Boot scene loads Main; Bootstrap wires the minimal service list. Tagged `v0.0.1-setup`.

**Effort**: 1–2 weeks of focused work.

**Acceptance criteria**: see `docs/CityRise_Tech_Roadmap.docx` Appendix E, "Phase 0".

## Sequenced task list

Each task lists `[USER]` for what only Mark can do, `[CC]` for Claude Code work, or `[BOTH]` for collaboration. Tasks are ordered by dependency.

### Day 1 — Project skeleton

| # | Owner | Task |
|---|-------|------|
| 1 | USER | Confirm Unity 6 LTS project exists at chosen path, opens cleanly, URP active |
| 2 | USER | Confirm CoplayDev Unity MCP is running and Claude Code can see it |
| 3 | CC | Initialize git in project root; create `.gitignore` (Unity standard + Rider/JetBrains); create `.gitattributes` with LFS rules for `*.psd *.png *.jpg *.tga *.fbx *.blend *.wav *.mp3 *.ogg *.mp4 *.mov` |
| 4 | CC | Create `/docs/` folder; copy GDD, Tech Roadmap, perf-budget.md, ADRs from handoff |
| 5 | CC | Create `CLAUDE.md` at project root from handoff |
| 6 | CC | Write `README.md`: project name, status, quickstart (clone, open in Unity 6 LTS, press play), pointers to /docs/ |
| 7 | CC | Write `CONTRIBUTING.md`: branching policy, commit conventions, PR checklist, ADR practice, link to coding rules in CLAUDE.md |
| 8 | USER | Create private GitHub repo `CityRise` (UI or `gh repo create CityRise --private`) |
| 9 | CC | Add remote, initial commit ("chore: project skeleton"), push to main |

### Day 2 — Asmdef structure and packages

| # | Owner | Task |
|---|-------|------|
| 10 | CC | Pin Unity packages in `Packages/manifest.json`: URP, Input System, Addressables, Cinemachine, Splines, Mathematics, Burst, Collections, Newtonsoft JSON, Probuilder, Test Framework |
| 11 | CC | Add MemoryPack via NuGet-for-Unity or as a precompiled DLL to `Assets/Plugins/` |
| 12 | CC | Create folder structure under `Assets/_CityRise/Code/`: Core, Content, Simulation, Persistence, Presentation, UI, Tools, App, Debug, Tests |
| 13 | CC | Create one `*.asmdef` per folder with the dependency rules in CLAUDE.md (downward only) |
| 14 | CC | Verify Unity recompiles cleanly with empty asmdefs |
| 15 | CC | Commit ("chore: layered asmdef structure and package pins") |

### Day 3 — Core facades part 1 (constants, IDs, math, RNG)

| # | Owner | Task |
|---|-------|------|
| 16 | CC | `Core/GameConstants.cs` — all locked constants from CLAUDE.md |
| 17 | CC | `Core/EntityId.cs` — `EntityId<T>` phantom-typed Guid wrapper with factory, equality, hash, debug-friendly ToString (`"bldg_a1b2c3d4"`) |
| 18 | CC | `Core/Result.cs` — `Result<T>` and `Result<Unit>` with Ok/Err constructors, Map, Bind, IsOk |
| 19 | CC | `Core/RandomService.cs` — seeded RNG service; sim-tick consumers receive an `IRandom`; `System.Random` and `UnityEngine.Random` banned in Simulation asmdef |
| 20 | CC | `Core/CoordinateConventions.cs` — documented constants, helpers (worldToTile, tileToWorld) |
| 21 | CC | NUnit tests for the above in `Tests/` |
| 22 | CC | Commit ("feat(core): constants, EntityId, Result, RNG, coordinates") |

### Day 4 — Core facades part 2 (logging, i18n, accessibility, analytics, feature flags)

| # | Owner | Task |
|---|-------|------|
| 23 | CC | `Core/Log.cs` — facade with `Log.Info/Warn/Error/Debug(category, msg)`; default sink to UnityEngine.Debug; pluggable for crash reporter later |
| 24 | CC | `Core/I18n.cs` — `I18n.Get(key, args)`; backed by a simple in-memory dictionary loaded from a `LocalizationTable` SO; English-only fallback; missing keys return `[KEY:foo.bar]` to make them visible |
| 25 | CC | `Core/AccessibilityService.cs` — stub interface with ColorblindPalette enum, UiScale float, ReducedMotion bool, RemappableInput list; backed by `AccessibilityConfig` SO |
| 26 | CC | `Core/Analytics.cs` — `Analytics.Track(eventName, payload)` facade with a NullSink backend |
| 27 | CC | `Core/FeatureFlags.cs` + `FeatureFlags.asset` SO — runtime-mutable; per-phase flags (PowerEnabled, WaterEnabled, AgentsEnabled, …) all off by default |
| 28 | CC | `Core/NotificationBus.cs` — `NotificationBus.Push(severity, key, args)`; UI subscribes |
| 29 | CC | `Core/IterateSorted.cs` — helper for deterministic iteration over NativeHashMap |
| 30 | CC | NUnit tests |
| 31 | CC | Commit ("feat(core): Log, I18n, Accessibility, Analytics, FeatureFlags, NotificationBus, IterateSorted") |

### Day 5 — Bootstrap and scenes

| # | Owner | Task |
|---|-------|------|
| 32 | CC | Create `App/Bootstrap.cs` — composition root; constructor-injects all Core facades into a `ServiceContainer` (manual wiring, no DI framework) |
| 33 | CC | Create Boot scene with a single GameObject holding `Bootstrap`; configure to load Main scene on start |
| 34 | CC | Create empty Main scene with a placeholder camera (Cinemachine RTS rig comes in Phase 1) |
| 35 | CC | Set Boot as the first scene in Build Settings; Main second |
| 36 | CC | Verify pressing Play in editor → Boot loads → Main loads → no errors |
| 37 | CC | Commit ("feat(app): Bootstrap composition root + Boot/Main scenes") |

### Day 6 — CI

| # | Owner | Task |
|---|-------|------|
| 38 | USER | Generate Unity license activation file for CI (per game-ci.com instructions) and add to GitHub repo secrets as `UNITY_LICENSE` |
| 39 | CC | Write `.github/workflows/build.yml` — game-ci/unity-builder action, Windows64 target, Unity 6 LTS, runs on push to main |
| 40 | CC | Write `.github/workflows/test.yml` — game-ci/unity-test-runner, runs on every PR |
| 41 | CC | Push, watch CI run; iterate until green |
| 42 | CC | Add CI status badge to README |
| 43 | CC | Commit ("ci: Windows build + edit-mode tests on Unity 6 LTS") |

### Day 7 — Documentation polish and tag

| # | Owner | Task |
|---|-------|------|
| 44 | CC | Verify all 9 ADRs are in `/docs/adr/` with correct status (Accepted) |
| 45 | CC | Verify `/docs/perf-budget.md` mirrors Tech Roadmap Appendix C |
| 46 | CC | Create empty `/docs/analytics-events.md` table (eventName, payload, added-in-phase) |
| 47 | CC | Run all Phase 0 acceptance criteria from Tech Roadmap Appendix E; check each box |
| 48 | CC | Open Phase 0 retrospective issue on GitHub: what went well, what to improve, hours actually spent vs estimate |
| 49 | CC | Tag `v0.0.1-setup`; push tag |
| 50 | BOTH | Mark Phase 0 done; transition to Phase 1 (Core Framework) |

## Things Claude Code should NOT do in Phase 0

- Don't write any sim systems (UtilitySystem, GrowthSystem, etc.). Phase 1.
- Don't write any commands beyond a placeholder `NoOpCommand` for testing CommandBus dispatch (Phase 1).
- Don't write the TickScheduler or CommandBus or EventBus (Phase 1).
- Don't write any UI panels (Phase 1 ships the UI shell only).
- Don't author content SOs (BuildingDef, OrdinanceDef). Phase 6 onward.
- Don't pull in pathfinding (A* Pathfinding Project) — visual agents are post-MVP.

## Things to flag back to the user

If during Phase 0 Claude Code hits any of the following, surface to the user before proceeding:

- A package version conflict in manifest.json
- A CI failure that requires Unity license assistance
- A naming/structural choice not covered in CLAUDE.md or the Tech Roadmap
- An estimate slipping more than 2x (Day-3 work taking 6 days)
- Anything that would require updating CLAUDE.md or creating a new ADR
