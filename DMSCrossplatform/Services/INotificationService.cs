using System;
using System.Collections.ObjectModel;

namespace DMSCrossplatform.Services;

public interface INotificationService
{
    ObservableCollection<AppNotification> Items { get; }
    void Add(AppNotification n);
    void MarkRead(Guid id);
}