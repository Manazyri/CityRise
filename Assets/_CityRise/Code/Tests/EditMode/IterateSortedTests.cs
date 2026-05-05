#nullable enable

using System.Collections.Generic;
using CityRise.Core;
using NUnit.Framework;
using Unity.Collections;

namespace CityRise.Tests.EditMode;

public sealed class IterateSortedTests
{
    [Test]
    public void GetSortedKeys_ReturnsKeysInAscendingOrder()
    {
        using var map = new NativeHashMap<int, int>(8, Allocator.Temp);
        map.Add(7, 70);
        map.Add(1, 10);
        map.Add(3, 30);
        map.Add(2, 20);

        using var keys = IterateSorted.GetSortedKeys(map, Allocator.Temp);
        Assert.That(keys.Length, Is.EqualTo(4));
        Assert.That(keys[0], Is.EqualTo(1));
        Assert.That(keys[1], Is.EqualTo(2));
        Assert.That(keys[2], Is.EqualTo(3));
        Assert.That(keys[3], Is.EqualTo(7));
    }

    [Test]
    public void Run_VisitsEveryEntry_InSortedOrder()
    {
        using var map = new NativeHashMap<int, int>(8, Allocator.Temp);
        map.Add(5, 500);
        map.Add(1, 100);
        map.Add(3, 300);

        var visited = new List<(int Key, int Value)>();
        IterateSorted.Run(map, (k, v) => visited.Add((k, v)));

        Assert.That(visited, Is.EqualTo(new List<(int, int)>
        {
            (1, 100), (3, 300), (5, 500),
        }));
    }

    [Test]
    public void Run_NullBody_Throws()
    {
        using var map = new NativeHashMap<int, int>(2, Allocator.Temp);
        map.Add(1, 1);
        Assert.That(() => IterateSorted.Run<int, int>(map, null!), Throws.ArgumentNullException);
    }

    [Test]
    public void Run_EmptyMap_DoesNotInvoke()
    {
        using var map = new NativeHashMap<int, int>(1, Allocator.Temp);
        var calls = 0;
        IterateSorted.Run(map, (_, _) => calls++);
        Assert.That(calls, Is.EqualTo(0));
    }
}
