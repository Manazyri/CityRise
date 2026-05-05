# 0001. Engine choice — Unity 6 LTS

Date: 2026-04-20
Status: Accepted

## Context

CityRise is a 3D stylized city-builder for PC. Two-person team (one programmer, one 3D modeler), hobby project with potential commercial pivot. We need an engine that supports a heavy simulation workload, provides good editor tooling, has a healthy ecosystem of relevant assets, and stays productive for a solo coder.

## Decision

Use **Unity 6 LTS** as the engine, with C# 10 / .NET Standard 2.1.

## Alternatives considered

- **Unreal Engine 5**: superior visual fidelity out of the box, but C++/Blueprints is a heavier lift for one coder, and Unreal's simulation tooling is less mature than Unity's DOTS/Burst stack.
- **Godot 4**: open-source and lean, but unproven at city-builder scale; we'd build more infrastructure ourselves.
- **Custom engine**: out of scope for a two-person team.

## Consequences

**Positive**
- Genre-proven (Cities: Skylines 1+2, Workers & Resources, Tropico are all Unity).
- C# is productive for a solo coder.
- Asset Store ecosystem covers non-differentiating systems (pathfinding, camera, UI primitives).
- Burst + Jobs available for sim hotspots without committing to ECS as a blanket architecture.

**Negative**
- Unity's runtime size is heavier than Godot.
- Some Unity APIs (UI Toolkit, Splines) are still maturing.

**Neutral**
- Locked to Unity LTS upgrade cadence.
