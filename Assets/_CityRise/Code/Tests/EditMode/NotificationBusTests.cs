#nullable enable

using System.Collections.Generic;
using CityRise.Core;
using NUnit.Framework;

namespace CityRise.Tests.EditMode;

public sealed class NotificationBusTests
{
    [Test]
    public void Push_FiresEvent_WithCorrectFields()
    {
        var bus = new NotificationBus();
        var captured = new List<Notification>();
        bus.OnNotification += n => captured.Add(n);

        bus.Push(NotificationSeverity.Warning, "budget.bankrupt", "month-1");

        Assert.That(captured.Count, Is.EqualTo(1));
        Assert.That(captured[0].Severity, Is.EqualTo(NotificationSeverity.Warning));
        Assert.That(captured[0].MessageKey, Is.EqualTo("budget.bankrupt"));
        Assert.That(captured[0].Args.Length, Is.EqualTo(1));
        Assert.That(captured[0].Args[0], Is.EqualTo("month-1"));
    }

    [Test]
    public void Push_NoSubscribers_DoesNotThrow()
    {
        var bus = new NotificationBus();
        Assert.DoesNotThrow(() => bus.Push(NotificationSeverity.Info, "anything"));
    }

    [Test]
    public void EmptyKey_Throws()
    {
        var bus = new NotificationBus();
        Assert.That(() => bus.Push(NotificationSeverity.Info, ""), Throws.ArgumentException);
    }

    [Test]
    public void Notification_Format_UsesI18n()
    {
        // Build a tiny localizer and route through I18n.
        var n = new Notification(NotificationSeverity.Error, "missing.key");
        var formatted = n.Format();
        Assert.That(formatted, Is.EqualTo("[KEY:missing.key]"));
    }
}
