#nullable enable

using System;

namespace CityRise.Core;

/// <summary>
/// One user-facing message emitted by sim or system code. UI subscribes to <see cref="NotificationBus"/>
/// and renders the localized text via <see cref="I18n.Get(string, object[])"/> using <see cref="MessageKey"/>.
/// </summary>
public readonly struct Notification : IEquatable<Notification>
{
    public readonly NotificationSeverity Severity;
    public readonly string MessageKey;
    public readonly object[] Args;

    private static readonly object[] s_empty = Array.Empty<object>();

    public Notification(NotificationSeverity severity, string messageKey, object[]? args = null)
    {
        if (string.IsNullOrEmpty(messageKey))
            throw new ArgumentException("messageKey must be non-empty.", nameof(messageKey));
        Severity = severity;
        MessageKey = messageKey;
        Args = args ?? s_empty;
    }

    /// <summary>Localizes the message via <see cref="I18n"/>.</summary>
    public string Format() => Args.Length == 0 ? I18n.Get(MessageKey) : I18n.Get(MessageKey, Args);

    public bool Equals(Notification other)
    {
        if (Severity != other.Severity || MessageKey != other.MessageKey) return false;
        if (Args.Length != other.Args.Length) return false;
        for (int i = 0; i < Args.Length; i++)
        {
            if (!Equals(Args[i], other.Args[i])) return false;
        }
        return true;
    }

    public override bool Equals(object? obj) => obj is Notification n && Equals(n);
    public override int GetHashCode() => HashCode.Combine((int)Severity, MessageKey, Args.Length);
    public override string ToString() => $"[{Severity}] {MessageKey} (args={Args.Length})";
}
