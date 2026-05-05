#nullable enable

using System;
using CityRise.Core;
using NUnit.Framework;

namespace CityRise.Tests.EditMode;

public sealed class ResultTests
{
    [Test]
    public void Ok_HasValue_NoError()
    {
        var r = Result<int>.Ok(42);
        Assert.That(r.IsOk, Is.True);
        Assert.That(r.IsErr, Is.False);
        Assert.That(r.Value, Is.EqualTo(42));
    }

    [Test]
    public void Err_HasError_NoValue()
    {
        var r = Result<int>.Err("nope");
        Assert.That(r.IsErr, Is.True);
        Assert.That(r.IsOk, Is.False);
        Assert.That(r.Error, Is.EqualTo("nope"));
    }

    [Test]
    public void Ok_AccessingError_Throws()
    {
        var r = Result<int>.Ok(1);
        Assert.That(() => _ = r.Error, Throws.InvalidOperationException);
    }

    [Test]
    public void Err_AccessingValue_Throws()
    {
        var r = Result<int>.Err("nope");
        Assert.That(() => _ = r.Value, Throws.InvalidOperationException);
    }

    [Test]
    public void Err_EmptyMessage_Rejected()
    {
        Assert.That(() => Result<int>.Err(""), Throws.ArgumentException);
        Assert.That(() => Result<int>.Err(null!), Throws.ArgumentException);
    }

    [Test]
    public void TryGet_Ok_YieldsValue()
    {
        var r = Result<string>.Ok("hi");
        var ok = r.TryGet(out var value, out var error);
        Assert.That(ok, Is.True);
        Assert.That(value, Is.EqualTo("hi"));
        Assert.That(error, Is.EqualTo(string.Empty));
    }

    [Test]
    public void TryGet_Err_YieldsError()
    {
        var r = Result<string>.Err("bad");
        var ok = r.TryGet(out _, out var error);
        Assert.That(ok, Is.False);
        Assert.That(error, Is.EqualTo("bad"));
    }

    [Test]
    public void Map_Ok_AppliesFunction()
    {
        var r = Result<int>.Ok(3).Map(x => x * 2);
        Assert.That(r.IsOk, Is.True);
        Assert.That(r.Value, Is.EqualTo(6));
    }

    [Test]
    public void Map_Err_PassesThrough()
    {
        var r = Result<int>.Err("upstream").Map(x => x * 2);
        Assert.That(r.IsErr, Is.True);
        Assert.That(r.Error, Is.EqualTo("upstream"));
    }

    [Test]
    public void Bind_Ok_ChainsFallibleStep()
    {
        var r = Result<int>.Ok(3).Bind(x => x > 0 ? Result<string>.Ok($"got {x}") : Result<string>.Err("neg"));
        Assert.That(r.IsOk, Is.True);
        Assert.That(r.Value, Is.EqualTo("got 3"));
    }

    [Test]
    public void Bind_Err_ShortCircuits()
    {
        var calls = 0;
        var r = Result<int>.Err("bad").Bind(_ => { calls++; return Result<int>.Ok(0); });
        Assert.That(r.IsErr, Is.True);
        Assert.That(calls, Is.EqualTo(0));
    }

    [Test]
    public void Map_NullFunction_Throws()
    {
        var r = Result<int>.Ok(1);
        Assert.That(() => r.Map<int>(null!), Throws.ArgumentNullException);
    }

    [Test]
    public void UnitResult_Ok_RoundTrips()
    {
        var r = Result<Unit>.Ok(Unit.Value);
        Assert.That(r.IsOk, Is.True);
        Assert.That(r.Value, Is.EqualTo(Unit.Value));
    }
}
