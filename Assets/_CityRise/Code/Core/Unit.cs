#nullable enable

using System;

namespace CityRise.Core;

/// <summary>
/// The unit type — a value type with a single inhabitant. Used as the success type of
/// <see cref="Result{Unit}"/> when a command succeeds with no payload.
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    public static readonly Unit Value = default;

    public bool Equals(Unit other) => true;
    public override bool Equals(object? obj) => obj is Unit;
    public override int GetHashCode() => 0;
    public override string ToString() => "()";

    public static bool operator ==(Unit _, Unit __) => true;
    public static bool operator !=(Unit _, Unit __) => false;
}
