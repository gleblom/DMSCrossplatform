using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DMSCrossplatform.Services;

public class NotificationService : INotificationService
{
    public ObservableCollection<AppNotification> Items { get; } = new();

    public void Add(AppNotification n) => Items.Insert(0, n);

    public void MarkRead(Guid id)
    {
        var item = Items.FirstOrDefault(x => x.Id == id);
        if (item is not null) item.Status = AppNotificationStatus.Read;
    }
}