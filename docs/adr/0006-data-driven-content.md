# 0006. Data-driven content — ordinances, coverage, desirability as modifier lists

Date: 2026-04-20
Status: Accepted

## Context

A pillar of CityRise is data-driven content: new buildings, ordinances, ploppables, and zone densities should be addable without code changes. The first version of the architecture had ordinance effects, coverage radii, and desirability factors implied to be hardcoded C# per type. That contradicts the pillar.

## Decision

Three systems are explicitly modeled as data:

- **Ordinances** are lists of `StatModifier` entries (`{ target, scope, op, amount }`). Passing an ordinance pushes modifiers; repealing pops them.
- **Coverage** is a list of `CoverageContribution` (`{ serviceId, radius, falloff, strength }`) on each PloppableDef. CoverageSystem iterates contributions without knowing which services exist.
- **Desirability** is a registered ordered list of `DesirabilityFactor` contributors (RoadProximity, Coverage, Pollution, OrdinanceModifier, DensityCap). Factors and weights live in a `DesirabilityConfig` ScriptableObject.

## Alternatives considered

- **Hardcoded C# per ordinance / coverage type / desirability factor**: rejected because it directly contradicts the data-driven content pillar and pushes balance work into code reviews.

## Consequences

**Positive**
- Adding a new ordinance, service type, or balance pass becomes an asset edit, not code.
- Tuning lives in ScriptableObjects, viewable and adjustable in the editor.
- The data-driven pillar becomes real, not aspirational.
- Future modder ergonomics: content mods can ship new ordinances with no code.

**Negative**
- Slightly more abstraction at the type level.
- Some genuinely-novel effect categories will still require new code (a new modifier op, a new coverage falloff curve type).

**Neutral**
- Gameplay-rule mods (custom growth math, custom systems) are still out of V1 scope. This ADR covers content data, not behavior code.
