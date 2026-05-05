#nullable enable

using System;

namespace CityRise.Core;

/// <summary>
/// Distributes user-facing notifications from sim and system code to subscribers (UI is the
/// primary one). Push from any thread that owns the main loop; the bus does not marshal —
/// callers serialize naturally because pushes happen at tick boundaries.
/// </summary>
public sealed class NotificationBus
{
    /// <summary>Fires for every <see cref="Push"/>. UI subscribes here.</summary>
    public event Action<Notification>? OnNotification;

    /// <summary>Push a message. <paramref name="messageKey"/> is an I18n key.</summary>
    public void Push(NotificationSeverity severity, string messageKey, params object[] args)
    {
        var notification = new Notification(severity, messageKey, args);
        OnNotification?.Invoke(notification);
    }

    public void Push(in Notification notification) => OnNotification?.Invoke(notification);
}
