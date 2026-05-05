#nullable enable

using System;
using Unity.Collections;

namespace CityRise.Core;

/// <summary>
/// Deterministic iteration over <see cref="NativeHashMap{TKey,TValue}"/>. NativeHashMap iteration
/// order is unspecified — using one directly in sim code is a determinism bug. Always go through
/// here (Tech Roadmap section 4.9).
/// </summary>
/// <remarks>
/// Hot-path callers should use <see cref="GetSortedKeys{TKey,TValue}"/> and loop the returned
/// array manually, since <c>Action</c> dispatch isn't Burst-compatible. <see cref="Run{TKey,TValue}"/>
/// is the convenience wrapper for non-hot paths.
/// </remarks>
public static class IterateSorted
{
    /// <summary>
    /// Returns a freshly allocated, sorted array of the map's keys. Caller owns the array and
    /// must dispose it (use <c>using var keys = ...</c>).
    /// </summary>
    public static NativeArray<TKey> GetSortedKeys<TKey, TValue>(
        in NativeHashMap<TKey, TValue> map,
        Allocator allocator)
        where TKey : unmanaged, IEquatable<TKey>, IComparable<TKey>
        where TValue : unmanaged
    {
        var keys = map.GetKeyArray(allocator);
        keys.Sort();
        return keys;
    }

    /// <summary>
    /// Iterate <paramref name="map"/> in sorted-key order, invoking <paramref name="body"/> for each pair.
    /// Allocates a temp key array; not Burst-compatible because of the delegate. Prefer
    /// <see cref="GetSortedKeys{TKey,TValue}"/> in sim hot paths.
    /// </summary>
    public static void Run<TKey, TValue>(
        in NativeHashMap<TKey, TValue> map,
        Action<TKey, TValue> body)
        where TKey : unmanaged, IEquatable<TKey>, IComparable<TKey>
        where TValue : unmanaged
    {
        if (body is null) throw new ArgumentNullException(nameof(body));

        using var keys = GetSortedKeys(map, Allocator.Temp);
        for (int i = 0; i < keys.Length; i++)
        {
            var key = keys[i];
            body(key, map[key]);
        }
    }
}
