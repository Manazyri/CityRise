#nullable enable

using System;
using CityRise.Core;
using NUnit.Framework;

namespace CityRise.Tests.EditMode;

public sealed class ServiceContainerTests
{
    private interface IGreeter { string Hello(); }
    private sealed class Greeter : IGreeter { public string Hello() => "hi"; }
    private sealed class FrenchGreeter : IGreeter { public string Hello() => "salut"; }

    [Test]
    public void Register_AndGet_RoundTrip()
    {
        var c = new ServiceContainer();
        var g = new Greeter();
        c.Register<IGreeter>(g);

        Assert.That(c.Get<IGreeter>(), Is.SameAs(g));
        Assert.That(c.Get<IGreeter>().Hello(), Is.EqualTo("hi"));
    }

    [Test]
    public void Get_Missing_Throws()
    {
        var c = new ServiceContainer();
        Assert.That(() => c.Get<IGreeter>(), Throws.InvalidOperationException);
    }

    [Test]
    public void TryGet_Missing_ReturnsFalse_AndNull()
    {
        var c = new ServiceContainer();
        var ok = c.TryGet<IGreeter>(out var svc);
        Assert.That(ok, Is.False);
        Assert.That(svc, Is.Null);
    }

    [Test]
    public void TryGet_Present_ReturnsTrue()
    {
        var c = new ServiceContainer();
        c.Register<IGreeter>(new Greeter());
        var ok = c.TryGet<IGreeter>(out var svc);
        Assert.That(ok, Is.True);
        Assert.That(svc, Is.Not.Null);
    }

    [Test]
    public void Register_Replaces_PriorRegistration()
    {
        var c = new ServiceContainer();
        c.Register<IGreeter>(new Greeter());
        c.Register<IGreeter>(new FrenchGreeter());

        Assert.That(c.Get<IGreeter>().Hello(), Is.EqualTo("salut"));
        Assert.That(c.Count, Is.EqualTo(1));
    }

    [Test]
    public void Register_Null_Throws()
    {
        var c = new ServiceContainer();
        Assert.That(() => c.Register<IGreeter>(null!), Throws.ArgumentNullException);
    }

    [Test]
    public void IsRegistered_TracksRegistrations()
    {
        var c = new ServiceContainer();
        Assert.That(c.IsRegistered<IGreeter>(), Is.False);
        c.Register<IGreeter>(new Greeter());
        Assert.That(c.IsRegistered<IGreeter>(), Is.True);
    }

    [Test]
    public void DistinctTypes_RegisterIndependently()
    {
        var c = new ServiceContainer();
        c.Register<IGreeter>(new Greeter());
        c.Register<NotificationBus>(new NotificationBus());
        Assert.That(c.Count, Is.EqualTo(2));
    }
}
