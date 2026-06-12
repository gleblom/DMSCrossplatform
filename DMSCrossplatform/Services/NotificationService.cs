using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public partial class NotificationService: ObservableObject, INotificationService
{
    [ObservableProperty]
    private ObservableCollection<MvNotificationsDto> _notifications = new();

    [ObservableProperty] private int _unreadCount;

    private readonly IPushService _push;

    public NotificationService(IPushService push)
    {
        _push = push;
        _ = LoadNotificationsAsync();
    }

    public event EventHandler? NotificationReceived;
    public event EventHandler? Initialized;

    private void RecalculateUnreadCount()
    {
        UnreadCount = Notifications.Count(x => !x.IsRead);
    }

    public async Task AddNotification(int notificationId)
    {
        var notification = await _push.GetNotification(notificationId);

        Notifications.Insert(0, notification);
        RecalculateUnreadCount();

        NotificationReceived?.Invoke(this, EventArgs.Empty);
    }

    public async Task MarkRead()
    {
        await _push.MarkRead();
        await LoadNotificationsAsync();
        NotificationReceived?.Invoke(this, EventArgs.Empty);
    }

    public async Task LoadNotificationsAsync()
    {
        Notifications.Clear();
        var not = await _push.GetNotifications();
        Notifications = new ObservableCollection<MvNotificationsDto>(
            not.OrderByDescending(x => x.CreatedAt));

        RecalculateUnreadCount();
        Initialized?.Invoke(this, EventArgs.Empty);
    }
}
