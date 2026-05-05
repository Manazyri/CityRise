# CityRise

A stylized low-poly 3D city-builder for PC. Two-person team (programmer + 3D modeler), hobby project with potential commercial pivot deferred to end of Alpha.

**Status:** Phase 0 — Setup, tech stack, and Phase-0 facades. See `PHASE_0_TASKS.md` for the current task list.

## Quickstart

1. Clone the repo:
   ```
   git clone https://github.com/Manazyri/CityRise.git
   ```
2. Install **Unity 6 LTS** via Unity Hub.
3. Open the project folder in Unity Hub. Unity will resolve packages on first open (this can take a few minutes).
4. Open the `Boot` scene (`Assets/_CityRise/Scenes/Boot.unity`) and press Play.

> Note: until Phase 0 task #33 is complete, the only scene is the URP template's. From task #33 onward, `Boot` is the entry scene.

## Tech stack (locked)

- Engine: **Unity 6 LTS** (URP)
- Language: **C# 10**, .NET Standard 2.1, nullable reference types enabled
- Input: **Unity Input System**
- UI: **UI Toolkit** (UGUI only when justified)
- Splines (roads): **Unity Splines** (`com.unity.splines`)
- Sim threading: managed C# default; **Jobs + Burst** on hotspots
- Math: **Unity.Mathematics**
- Save: **MemoryPack** binary + Newtonsoft JSON debug dumps (dev-only)
- VCS: Git + Git LFS on GitHub (private)
- CI: GitHub Actions (Unity headless Windows build per main merge)

Full architecture details in [`CLAUDE.md`](CLAUDE.md) and [`docs/CityRise_Tech_Roadmap.docx`](docs/CityRise_Tech_Roadmap.docx).

## Project layout

```
Assets/
  _CityRise/
    Code/        Layered asmdefs (Core → Content → Simulation → Persistence → Presentation → UI → Tools → App; Debug, Tests isolated)
    Scenes/      Boot.unity, Main.unity
  Plugins/       Third-party DLLs (NuGet packages, MemoryPack, etc.)
  Settings/      URP renderer assets
docs/
  CityRise_GDD.docx                Game design document
  CityRise_Tech_Roadmap.docx       Architecture and phased plan (v0.4)
  adr/                             Architecture Decision Records (0001–0009)
  perf-budget.md                   Per-system CPU budgets at 500-pop target
CLAUDE.md                          AI-assistant context (locked decisions, coding rules)
CONTRIBUTING.md                    Branching, commits, PR checklist, ADR practice
PHASE_0_TASKS.md                   Phase 0 sequenced task list
HANDOFF.md                         Original handoff doc; preserved for history
```

## Documentation

- **Architecture & coding rules:** [`CLAUDE.md`](CLAUDE.md)
- **Game design:** [`docs/CityRise_GDD.docx`](docs/CityRise_GDD.docx)
- **Technical roadmap:** [`docs/CityRise_Tech_Roadmap.docx`](docs/CityRise_Tech_Roadmap.docx)
- **Architectural decisions:** [`docs/adr/`](docs/adr/)
- **Performance budgets:** [`docs/perf-budget.md`](docs/perf-budget.md)
- **Contributing:** [`CONTRIBUTING.md`](CONTRIBUTING.md)

## License

Hobby phase: not yet licensed. License decision deferred to commercial-intent decision at end of Alpha.
