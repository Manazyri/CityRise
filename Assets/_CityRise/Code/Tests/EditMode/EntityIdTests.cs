#nullable enable

using System;
using CityRise.Core;
using NUnit.Framework;

namespace CityRise.Tests.EditMode;

[EntityIdPrefix("bldg")]
internal sealed class TestBuildingMarker { }

[EntityIdPrefix("road")]
internal sealed class TestRoadMarker { }

internal sealed class UnprefixedMarker { }

public sealed class EntityIdTests
{
    [Test]
    public void New_ReturnsDistinctIds()
    {
        var a = EntityId<TestBuildingMarker>.New();
        var b = EntityId<TestBuildingMarker>.New();
        Assert.That(a, Is.Not.EqualTo(b));
        Assert.That(a.IsNone, Is.False);
        Assert.That(b.IsNone, Is.False);
    }

    [Test]
    public void None_IsEmptyGuid()
    {
        var none = EntityId<TestBuildingMarker>.None;
        Assert.That(none.IsNone, Is.True);
        Assert.That(none.Value, Is.EqualTo(Guid.Empty));
    }

    [Test]
    public void Equality_IsValueBased()
    {
        var g = Guid.NewGuid();
        var a = new EntityId<TestBuildingMarker>(g);
        var b = new EntityId<TestBuildingMarker>(g);
        Assert.That(a == b, Is.True);
        Assert.That(a.Equals(b), Is.True);
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void ToString_UsesAttributePrefix_AndFirst8HexChars()
    {
        var g = new Guid("a1b2c3d4-1111-2222-3333-444455556666");
        var id = new EntityId<TestBuildingMarker>(g);
        Assert.That(id.ToString(), Is.EqualTo("bldg_a1b2c3d4"));
    }

    [Test]
    public void ToString_DifferentMarker_DifferentPrefix()
    {
        var g = new Guid("e5f6a7b8-1111-2222-3333-444455556666");
        var id = new EntityId<TestRoadMarker>(g);
        Assert.That(id.ToString(), Is.EqualTo("road_e5f6a7b8"));
    }

    [Test]
    public void ToString_NoAttribute_FallsBackToTypeNamePrefix()
    {
        var g = new Guid("12345678-1111-2222-3333-444455556666");
        var id = new EntityId<UnprefixedMarker>(g);
        // Falls back to first 4 lowercased chars of "UnprefixedMarker" → "unpr"
        Assert.That(id.ToString(), Is.EqualTo("unpr_12345678"));
    }
}
