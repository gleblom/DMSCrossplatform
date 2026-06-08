using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public class NotificationService: ObservableObject, INotificationService
{
    public ObservableCollection<MvNotificationsDto> Notifications { get; set; } = new();
    
    private readonly IDocumentService _doc;
    private readonly IPushService _push;

    public NotificationService(IDocumentService doc, IPushService push)
    {
        _doc = doc;
        _push = push;
        
        _ = LoadNotificationsAsync();
    }

    private int _unreadCount;

    public int UnreadCount
    {
        get => Notifications.Count(x => !x.IsRead);
        set => SetProperty(ref _unreadCount, value);
    }

    public event EventHandler? NotificationReceived;
    public event EventHandler? Initialized;

    public async Task AddNotification(int notificationId)
    {
        var notification = await _push.GetNotification(notificationId);
        
        Notifications.Add(notification);
        
        _unreadCount++;
        
        NotificationReceived?.Invoke(this, EventArgs.Empty);
        Notifications = new ObservableCollection<MvNotificationsDto>(Notifications.OrderByDescending(x => x.CreatedAt));
    }

    public async Task MarkRead()
    {
        await _push.MarkRead();
        _ = LoadNotificationsAsync();
    }

   
    private async Task LoadNotificationsAsync()
    {
        var not = await _push.GetNotifications();

        Notifications = new ObservableCollection<MvNotificationsDto>(not.OrderByDescending(x => x.CreatedAt));
        Initialized?.Invoke(this, EventArgs.Empty);
    }
}