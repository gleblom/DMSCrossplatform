using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Android.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.Services;

namespace DMSCrossplatform.ViewModels;

public partial class NotificationViewModel: ViewModelBase
{
    private readonly INavigationService<MenuRegionState> _nav;
    private readonly INotificationService _notification;
    private readonly IPushService _pushService;
    
    [ObservableProperty] private ObservableCollection<MvNotificationsDto>  _notifications;

    public NotificationViewModel(
        INavigationService<MenuRegionState> nav, 
        INotificationService notification, IPushService pushService)
    {
        _nav = nav;
        _notification = notification;
        _pushService = pushService;
        _ = LoadNotificationsAsync();
    }

    private async Task LoadNotificationsAsync()
    {
        await _notification.LoadNotificationsAsync();

        Notifications = _notification.Notifications;
    }

    [RelayCommand]
    private void OpenDocument(Guid? documentId)
    {
        if (documentId == null)
        {
            return;
        }
        App.SelectedDocumentId = documentId;
        DocumentViewModel.Mode = "incoming";
        _nav.NavigateTo<DocumentViewModel>();
    }

    [RelayCommand]
    private async Task DeleteNotification(int? notificationId)
    {
        if (notificationId == null)
            return;
        await _pushService.DeleteNotification(notificationId.Value);
        await  LoadNotificationsAsync();
    }
    
    
    
}