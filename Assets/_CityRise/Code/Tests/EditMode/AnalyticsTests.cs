#nullable enable

using System;
using System.Collections.Generic;
using CityRise.Core;
using NUnit.Framework;

namespace CityRise.Tests.EditMode;

public sealed class AnalyticsTests
{
    private sealed class CapturingSink : IAnalyticsSink
    {
        public readonly List<(string Name, IReadOnlyDictionary<string, object>? Payload)> Events = new();
        public void Track(string eventName, IReadOnlyDictionary<string, object>? payload)
            => Events.Add((eventName, payload));
    }

    [TearDown]
    public void TearDown() => Analytics.ResetSink();

    [Test]
    public void Default_IsNullSink_NoOp()
    {
        Assert.That(Analytics.ActiveSink, Is.SameAs(NullAnalyticsSink.Instance));
        Assert.DoesNotThrow(() => Analytics.Track("any.event"));
    }

    [Test]
    public void SetSink_RoutesTrack()
    {
        var sink = new CapturingSink();
        Analytics.SetSink(sink);

        Analytics.Track("session.start");
        Analytics.Track("session.end", new Dictionary<string, object> { ["duration"] = 42 });

        Assert.That(sink.Events.Count, Is.EqualTo(2));
        Assert.That(sink.Events[0].Name, Is.EqualTo("session.start"));
        Assert.That(sink.Events[0].Payload, Is.Null);
        Assert.That(sink.Events[1].Name, Is.EqualTo("session.end"));
        Assert.That(sink.Events[1].Payload!["duration"], Is.EqualTo(42));
    }

    [Test]
    public void EmptyEventName_Throws()
    {
        Assert.That(() => Analytics.Track(""), Throws.ArgumentException);
        Assert.That(() => Analytics.Track(null!), Throws.ArgumentException);
    }

    [Test]
    public void NullPayload_Throws_OnPayloadOverload()
    {
        Assert.That(() => Analytics.Track("e", null!), Throws.ArgumentNullException);
    }

    [Test]
    public void SetSink_NullThrows()
    {
        Assert.That(() => Analytics.SetSink(null!), Throws.ArgumentNullException);
    }
}
