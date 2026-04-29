using System;

namespace DMSCrossplatform.Services;

public enum AppNotificationType { NewApproval, Rejection, Completion, Expiration }
public enum AppNotificationStatus { Unread, Read }
public class AppNotification
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public AppNotificationType Type { get; init; }
    public AppNotificationStatus Status { get; set; } = AppNotificationStatus.Unread;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}