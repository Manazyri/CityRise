# 0004. Simulation model — hybrid statistical core + visual agent layer

Date: 2026-04-20
Status: Accepted

## Context

Modern city-builders typically use agent-based simulation — every citizen and vehicle is an entity with home, job, and commute. Cities: Skylines popularized this. The cost is significant engineering investment and a hard performance ceiling around tens of thousands of agents.

CityRise leans toward SimCity 4's depth and feel rather than Cities: Skylines's agent simulation. The team is two people. The art direction is stylized low-poly with the camera pulled back.

## Decision

Use a **hybrid simulation**: a **statistical core** drives all gameplay-relevant numbers (zone growth, jobs, desirability, utilities, budget) at the tile level. A **visual agent layer** provides the perception of a living city via capped pedestrians (~200) and vehicles (~500) that path on the road network. Agents reflect statistics; they do not drive them.

## Alternatives considered

- **Pure agent-based**: matches modern genre expectations but is the highest single engineering risk for a solo coder.
- **Pure statistical**: easiest to build, scales well, but feels less alive — modern reviewers may ding it.

## Consequences

**Positive**
- Statistical core keeps engineering budget for ordinances, utilities, zoning depth, ploppables, unlock trees — the systems we actually want.
- Visual agent layer satisfies modern players' expectation of a living city.
- Specific systems (e.g., traffic) can be upgraded to true agent simulation post-MVP if desired.
- Performance budget is comfortably under control.

**Negative**
- Two layers to maintain; design discipline required to keep them conceptually aligned.
- Some emergent gameplay (organic traffic patterns) is reduced.

**Neutral**
- The visual agent layer is a Phase 1+ addition; MVP statistical core ships without it.
