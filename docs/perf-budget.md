# Performance Budget

Per-system soft CPU budgets at the **500-population** target. TickMetrics records actual ms/tick; exceeding the budget at phase close is a review-blocking issue.

These numbers are starting points; revise as real profiling data comes in (typically after Phase 6 and Phase 10).

## Sim tick (1 Hz at 1× speed)

| System | Budget |
|---|---|
| `CommandBus.DrainQueue` | ≤ 0.5 ms |
| `UtilitySystem.Solve` (power + water combined) | ≤ 2.0 ms |
| `CoverageSystem.Recompute` | ≤ 1.0 ms |
| `OrdinanceSystem.ApplyModifiers` | ≤ 0.2 ms |
| `DesirabilitySystem.Update` | ≤ 1.5 ms |
| `EventBus.Flush` | ≤ 0.3 ms |
| **Total sim tick** | **≤ 5.5 ms** |

## Growth tick (0.1 Hz)

| System | Budget |
|---|---|
| `GrowthSystem` (full growth tick) | ≤ 4.0 ms |

## Budget tick (monthly)

| System | Budget |
|---|---|
| `BudgetSystem` (full budget tick) | ≤ 2.0 ms |

## Frame budget

| Item | Budget |
|---|---|
| Main-thread frame time | ≤ 16.6 ms (60 FPS) |
| Sim contribution to a tick frame | ≤ 5.5 ms (see above) |
| Presentation, UI, input | ≤ 11 ms |

## Memory

| Item | Budget |
|---|---|
| WorldState (500 pop city) | ≤ 50 MB |
| Presentation (buildings, meshes, textures) | ≤ 200 MB |

## I/O

| Item | Budget |
|---|---|
| Save file size (500 pop city) | ≤ 5 MB binary |
| Save duration | ≤ 1 second |
| Load duration | ≤ 3 seconds |

## Discipline

- TickMetrics prints a warning when any system exceeds its budget for 3 consecutive ticks.
- At every phase-close commit, attach the ms/tick numbers to the commit message.
- Crossing budget by >2× requires either an optimization PR or an ADR raising the budget with justification.
