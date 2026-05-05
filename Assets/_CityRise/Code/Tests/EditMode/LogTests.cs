#nullable enable

using System;
using System.Collections.Generic;
using CityRise.Core;
using NUnit.Framework;

namespace CityRise.Tests.EditMode;

public sealed class LogTests
{
    private sealed class CapturingSink : ILogSink
    {
        public readonly List<(LogLevel Level, LogCategory Category, string Message)> Entries = new();
        public void Write(LogLevel level, LogCategory category, string message)
        {
            Entries.Add((level, category, message));
        }
    }

    [TearDown]
    public void TearDown() => Log.ResetSink();

    [Test]
    public void SetSink_RoutesAllLevels()
    {
        var sink = new CapturingSink();
        Log.SetSink(sink);

        Log.Debug(LogCategory.Sim, "d");
        Log.Info(LogCategory.UI, "i");
        Log.Warn(LogCategory.Render, "w");
        Log.Error(LogCategory.Net, "e");

        Assert.That(sink.Entries.Count, Is.EqualTo(4));
        Assert.That(sink.Entries[0], Is.EqualTo((LogLevel.Debug, LogCategory.Sim, "d")));
        Assert.That(sink.Entries[1], Is.EqualTo((LogLevel.Info, LogCategory.UI, "i")));
        Assert.That(sink.Entries[2], Is.EqualTo((LogLevel.Warn, LogCategory.Render, "w")));
        Assert.That(sink.Entries[3], Is.EqualTo((LogLevel.Error, LogCategory.Net, "e")));
    }

    [Test]
    public void SetSink_NullThrows()
    {
        Assert.That(() => Log.SetSink(null!), Throws.ArgumentNullException);
    }

    [Test]
    public void ResetSink_RestoresDefault()
    {
        Log.SetSink(new CapturingSink());
        var customSink = Log.ActiveSink;
        Log.ResetSink();
        Assert.That(Log.ActiveSink, Is.Not.SameAs(customSink));
    }
}
