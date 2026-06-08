using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public interface INotificationService
{
    ObservableCollection<MvNotificationsDto> Notifications { get; set; }
    event EventHandler? NotificationReceived;
    event EventHandler? Initialized;

    Task AddNotification(int notificationId);

    Task MarkRead();
    
    int UnreadCount { get; set; }
}