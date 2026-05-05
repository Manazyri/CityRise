# 0002. Terrain — custom chunked heightmap

Date: 2026-04-20
Status: Accepted

## Context

CityRise has a fixed simulation/zoning grid (8 m tile, 4 m heightmap vertex spacing). Terraforming is a primary player verb, so heightmap edits happen frequently at runtime. The art direction is stylized low-poly — flat-shaded terrain colored by terrain type, no texture splatting, no grass/tree systems in MVP.

## Decision

Implement a **custom chunked heightmap terrain** instead of using Unity's built-in Terrain system. Chunks of 32×32 vertices at 4 m spacing (128 m per chunk), 16×16 chunks for a 2048 m map. Burst-compiled mesh regeneration with dirty-flag chunk regen. Flat-shaded material with per-face color keyed to terrain type.

## Alternatives considered

- **Unity built-in Terrain**: rich features (painting, LOD, colliders, trees, grass, splatting) but slow runtime edits, fixed heightmap resolution awkwardly aligned with our 8 m sim grid, painful URP custom shader integration for a stylized look, and significant overhead for features we don't use.
- **Unity Terrain + adapter layer**: keeps some built-in features but doubles the complexity and still suffers the runtime-edit speed problem.

## Consequences

**Positive**
- Vertex spacing aligned exactly with the sim grid.
- Only dirty chunks regenerate on edit — terraforming is fast.
- Burst + Jobs compilable.
- Tight shader control for the stylized flat-shaded look.
- Simplified by skipping texture splatting (low-poly aesthetic doesn't need it).
- Trivial save/load (one float array).

**Negative**
- 3–4 weeks of upfront engineering in Phase 3.
- We implement LOD, normals, and collider mesh ourselves.
- Edge cases at chunk seams (normals, gaps) need care.
- Less community help than Unity Terrain.

**Neutral**
- If we ever pivot to realistic graphics with grass/trees/detail objects, this decision should be revisited (Unity Terrain is the better fit for that art direction).
