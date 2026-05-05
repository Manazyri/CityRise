#nullable enable

namespace CityRise.Core;

/// <summary>Severity tier for user-facing notifications surfaced via <see cref="NotificationBus"/>.</summary>
public enum NotificationSeverity
{
    Info,
    Warning,
    Error,
}
