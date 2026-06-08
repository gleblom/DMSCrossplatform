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
    private readonly IPushService _doc;
    
    [ObservableProperty] private ObservableCollection<MvNotificationsDto>  _notifications;

    public NotificationViewModel(INavigationService<MenuRegionState> nav, IPushService doc)
    {
        _nav = nav;
        _doc = doc;
       _ = LoadDocumentsAsync();
    }

    private async Task LoadDocumentsAsync()
    {
        var docs = await _doc.GetNotifications();

        Notifications = new ObservableCollection<MvNotificationsDto>(docs.OrderByDescending(x => x.CreatedAt));
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
    private void DeleteNotification()
    {
        
    }
    
    
    
}