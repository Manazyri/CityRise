# 0009. Typed entity IDs — EntityId<T> via phantom types

Date: 2026-04-20
Status: Accepted

## Context

Many things in WorldState need stable identity across saves and lookups: buildings, ploppables, road nodes, road segments, future agents. The naive options are integer indices (fragile across saves and reorderings) or untyped Guids (no compile-time protection against mixing ID types).

## Decision

Use **`EntityId<T>` as a phantom-typed Guid wrapper**:

```csharp
public readonly struct EntityId<T> : IEquatable<EntityId<T>> {
    public readonly Guid Value;
    public EntityId(Guid g) { Value = g; }
    public static EntityId<T> New() => new(Guid.NewGuid());
    public override string ToString() => $"{TypePrefix<T>.Get()}_{Value:N}".Substring(0, 14);
    // equality, hash, etc.
}
```

`EntityId<Building>`, `EntityId<RoadSegment>`, `EntityId<Ploppable>` are distinct types at compile time. Passing a road-segment ID to a building lookup is a compile error.

`ToString()` returns a debug-friendly form — `"bldg_a1b2c3d4"`, `"road_e5f6g7h8"` — that's readable in logs and JSON dumps.

## Alternatives considered

- **Integer indices into arrays**: fast but fragile across saves, reorderings, and additions.
- **Untyped Guid**: no compile-time protection against ID-type confusion.
- **Per-entity-type wrapper struct (`BuildingId`, `RoadSegmentId`, …) with no shared generic**: equivalent type safety but more boilerplate, no shared code reuse.

## Consequences

**Positive**
- Compile-time prevention of ID-type mix-ups.
- Free runtime cost (struct, no boxing).
- Stable across saves (Guid is canonical).
- Debug-friendly stringification.

**Negative**
- Slightly verbose call sites where the type parameter must be explicit.
- Generic constraint in some collection types (`Dictionary<EntityId<Building>, BuildingState>`).

**Neutral**
- The phantom-type pattern is uncommon in C# but well-understood in functional languages and game-engine codebases.
