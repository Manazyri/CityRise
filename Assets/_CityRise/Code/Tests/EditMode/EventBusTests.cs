#nullable enable

using System.Collections.Generic;
using CityRise.Simulation.Infrastructure;
using NUnit.Framework;

namespace CityRise.Tests.EditMode;

public sealed class EventBusTests
{
    private readonly struct PingEvent : IEvent
    {
        public readonly int Value;
        public PingEvent(int v) { Value = v; }
    }

    private readonly struct PongEvent : IEvent
    {
        public readonly string Tag;
        public PongEvent(string t) { Tag = t; }
    }

    [Test]
    public void Publish_Without_Flush_DoesNotDispatch()
    {
        var bus = new EventBus();
        var received = new List<int>();
        bus.Subscribe<PingEvent>(e => received.Add(e.Value));

        bus.Publish(new PingEvent(1));
        bus.Publish(new PingEvent(2));

        Assert.That(received, Is.Empty, "Subscribers must not see events until Flush.");
        Assert.That(bus.PendingCount<PingEvent>(), Is.EqualTo(2));
    }

    [Test]
    public void Flush_DispatchesAllPending()
    {
        var bus = new EventBus();
        var received = new List<int>();
        bus.Subscribe<PingEvent>(e => received.Add(e.Value));

        bus.Publish(new PingEvent(1));
        bus.Publish(new PingEvent(2));
        var dispatched = bus.Flush();

        Assert.That(dispatched, Is.EqualTo(2));
        Assert.That(received, Is.EqualTo(new[] { 1, 2 }));
        Assert.That(bus.PendingCount<PingEvent>(), Is.EqualTo(0));
    }

    [Test]
    public void Flush_RoutesByEventType()
    {
        var bus = new EventBus();
        var pings = new List<int>();
        var pongs = new List<string>();
        bus.Subscribe<PingEvent>(e => pings.Add(e.Value));
        bus.Subscribe<PongEvent>(e => pongs.Add(e.Tag));

        bus.Publish(new PingEvent(7));
        bus.Publish(new PongEvent("hi"));
        bus.Publish(new PingEvent(8));
        bus.Flush();

        Assert.That(pings, Is.EqualTo(new[] { 7, 8 }));
        Assert.That(pongs, Is.EqualTo(new[] { "hi" }));
    }

    [Test]
    public void MultipleSubscribers_AllFire()
    {
        var bus = new EventBus();
        var a = 0;
        var b = 0;
        bus.Subscribe<PingEvent>(_ => a++);
        bus.Subscribe<PingEvent>(_ => b++);

        bus.Publish(new PingEvent(0));
        bus.Flush();

        Assert.That(a, Is.EqualTo(1));
        Assert.That(b, Is.EqualTo(1));
    }

    [Test]
    public void Unsubscribe_StopsDelivery()
    {
        var bus = new EventBus();
        var count = 0;
        void Handler(PingEvent e) => count++;
        bus.Subscribe<PingEvent>(Handler);

        bus.Publish(new PingEvent(0));
        bus.Flush();
        Assert.That(count, Is.EqualTo(1));

        bus.Unsubscribe<PingEvent>(Handler);
        bus.Publish(new PingEvent(1));
        bus.Flush();
        Assert.That(count, Is.EqualTo(1), "Unsubscribed handler must not fire.");
    }

    [Test]
    public void NoSubscriber_PendingEventsStillCleared()
    {
        var bus = new EventBus();
        bus.Publish(new PingEvent(1));
        Assert.That(bus.PendingCount<PingEvent>(), Is.EqualTo(1));

        bus.Flush();
        Assert.That(bus.PendingCount<PingEvent>(), Is.EqualTo(0));
    }

    [Test]
    public void RepublishingDuringDispatch_DefersToNextFlush()
    {
        var bus = new EventBus();
        var received = new List<int>();
        bus.Subscribe<PingEvent>(e =>
        {
            received.Add(e.Value);
            if (e.Value == 1) bus.Publish(new PingEvent(99));
        });

        bus.Publish(new PingEvent(1));
        bus.Flush();
        Assert.That(received, Is.EqualTo(new[] { 1 }), "Re-published event must not fire in the same flush.");
        Assert.That(bus.PendingCount<PingEvent>(), Is.EqualTo(1));

        bus.Flush();
        Assert.That(received, Is.EqualTo(new[] { 1, 99 }));
    }

    [Test]
    public void Subscribe_Null_Throws()
    {
        var bus = new EventBus();
        Assert.That(() => bus.Subscribe<PingEvent>(null!), Throws.ArgumentNullException);
        Assert.That(() => bus.Unsubscribe<PingEvent>(null!), Throws.ArgumentNullException);
    }

    [Test]
    public void ClearPending_DropsWithoutDispatch()
    {
        var bus = new EventBus();
        var fired = 0;
        bus.Subscribe<PingEvent>(_ => fired++);

        bus.Publish(new PingEvent(0));
        bus.Publish(new PingEvent(1));
        bus.ClearPending();
        bus.Flush();

        Assert.That(fired, Is.EqualTo(0));
    }
}
