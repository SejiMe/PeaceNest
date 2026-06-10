namespace PeaceNest.Api.Features.Notifications.ListNotifications;

public sealed record Response(
    IReadOnlyCollection<NotificationResponse> Notifications,
    int UnreadCount);
