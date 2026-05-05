#nullable enable

using System;
using System.Reflection;

namespace CityRise.Core;

/// <summary>
/// Phantom-typed Guid wrapper. <see cref="EntityId{Building}"/> and <see cref="EntityId{RoadSegment}"/>
/// are distinct types at compile time; passing one where the other is expected is a compile error.
/// </summary>
/// <remarks>
/// Per ADR-0009. Marker type <typeparamref name="T"/> can be any type — typically an empty marker
/// struct or the entity definition class itself. Apply <see cref="EntityIdPrefixAttribute"/> to the
/// marker for a custom debug prefix; otherwise the prefix is the lower-cased first 4 characters
/// of the type name.
/// </remarks>
public readonly struct EntityId<T> : IEquatable<EntityId<T>>
{
    public readonly Guid Value;

    public EntityId(Guid value)
    {
        Value = value;
    }

    /// <summary>Generates a fresh, globally unique id. Not deterministic — do not call from sim code; use a seeded factory if you need determinism.</summary>
    public static EntityId<T> New() => new(Guid.NewGuid());

    /// <summary>An empty id (Guid.Empty). Treat as "unset"; never persist as a real entity reference.</summary>
    public static EntityId<T> None => default;

    public bool IsNone => Value == Guid.Empty;

    public bool Equals(EntityId<T> other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is EntityId<T> other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(EntityId<T> a, EntityId<T> b) => a.Equals(b);
    public static bool operator !=(EntityId<T> a, EntityId<T> b) => !a.Equals(b);

    /// <summary>Debug-friendly form: "&lt;prefix&gt;_&lt;first 8 hex chars of the Guid&gt;". E.g. "bldg_a1b2c3d4".</summary>
    public override string ToString()
    {
        var prefix = TypePrefix.Get();
        var hex = Value.ToString("N").Substring(0, 8);
        return $"{prefix}_{hex}";
    }

    /// <summary>Cached prefix lookup per marker type. Resolved once at first access.</summary>
    private static class TypePrefix
    {
        private static readonly string s_prefix = Resolve();

        public static string Get() => s_prefix;

        private static string Resolve()
        {
            var attr = typeof(T).GetCustomAttribute<EntityIdPrefixAttribute>(inherit: false);
            if (attr is not null) return attr.Prefix;

            var name = typeof(T).Name.ToLowerInvariant();
            return name.Length >= 4 ? name.Substring(0, 4) : name.PadRight(4, 'x');
        }
    }
}
