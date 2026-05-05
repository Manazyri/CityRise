# 0003. Roads — Unity Splines package

Date: 2026-04-20
Status: Accepted

## Context

Roads are the spine of any city-builder. The data model needs to support straight and curved segments, intersections, runtime placement and removal, and serialization. The mesh needs to look modern — no strict grid-tile road blockiness. We have a two-person team and want to spend engineering budget on what's differentiating.

## Decision

Use the **Unity Splines package** (`com.unity.splines`) as the substrate for road segments. Cubic Bezier knots with tangent handles, runtime mutation supported, sampling APIs for road-tile adjacency, and mesh extrusion utilities for the road mesh.

## Alternatives considered

- **Strict grid tile roads**: very simple but no curves; reads as a dated budget title.
- **Polyarc (straight + circular arc segments)**: most mathematically elegant for roads specifically — closed-form math for length/tangent/point-at-distance, trivial mesh extrusion, matches how real roads are built. Custom code, smaller Unity ecosystem.

## Consequences

**Positive**
- Engine-supplied — saves weeks of work.
- Well-tested, well-maintained package.
- Industry-standard for Unity road tools.
- Editor authoring of test layouts is free.

**Negative**
- Bezier is slightly less elegant than polyarc.
- Tangent continuity at shared nodes requires explicit handling.
- Runtime mutation of splines is supported but less commonly trodden than baked spline use.

**Neutral**
- Polyarc remains a fallback if Unity Splines proves constraining during Phase 4b. Migration would be contained to RoadNetwork and RoadMeshGenerator.
