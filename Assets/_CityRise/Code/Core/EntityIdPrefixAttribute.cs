#nullable enable

using System;

namespace CityRise.Core;

/// <summary>
/// Marks the short prefix used in <see cref="EntityId{T}.ToString"/> for a given marker type.
/// Without this attribute the prefix falls back to the lower-cased first 4 characters of the type name.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class EntityIdPrefixAttribute : Attribute
{
    public string Prefix { get; }

    public EntityIdPrefixAttribute(string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
            throw new ArgumentException("Prefix must be non-empty.", nameof(prefix));
        if (prefix.Length > 8)
            throw new ArgumentException("Prefix must be 8 characters or fewer.", nameof(prefix));
        Prefix = prefix;
    }
}
