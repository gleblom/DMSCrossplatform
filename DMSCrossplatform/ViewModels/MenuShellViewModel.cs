using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure;
using DMSCrossplatform.Infrastructure.Android;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Infrastructure.Policy;
using DMSCrossplatform.Services;
using DMSCrossplatform.Views;
using Microsoft.Extensions.DependencyInjection;
using Plugin.LocalNotification.EventArgs;

namespace DMSCrossplatform.ViewModels;
public sealed class MenuRegionState : ViewState { }

public partial class MenuShellViewModel : ViewModelBase
{
    private readonly INavigationService<MenuRegionState> _navigation;
    private readonly ShellHost _host;
    private readonly ISessionService _sessionService;
    private readonly IPolicy _policy;
    private readonly IWindowsGetChannelUri _getChannelUri;
    private readonly IAndroidGetFcmToken _getFcmToken;
    private readonly INotificationService _notificationService;
    
    [ObservableProperty] private int _notificationsCount;
    
    [ObservableProperty] private bool _isPaneOpen;
    [ObservableProperty] private bool _isNotificationPaneOpen;

    [ObservableProperty] private bool _isNotificationsVisible;
    [ObservableProperty] private bool _canSeeDocs;
    [ObservableProperty] private bool _canSeeEmployees;
    [ObservableProperty] private bool _canSeeApprovalRoutes;
    [ObservableProperty] private string _fullName;
    [ObservableProperty] private string _role;
    // [ObservableProperty] private SettingsViewModel _settings;
    [ObservableProperty] private NotificationViewModel _notification;
    [ObservableProperty] private bool _isOpen;

    public MenuRegionState Region { get; }

    public MenuShellViewModel(
        ISessionService sessionService,
        MenuRegionState region,
        INavigationService<MenuRegionState> navigation,
        NotificationViewModel notification,
        IPolicyFactory policyFactory, ShellHost host, 
        IWindowsGetChannelUri getChannelUri, INotificationService notificationService)
    {
        _sessionService = sessionService;
        Region = region;
        _navigation = navigation;
        _host = host;
        _getChannelUri = getChannelUri;
        _notificationService = notificationService;


        _policy = policyFactory.CreatePolicy();

        Initialize(sessionService, notificationService, notification);
    }
    public MenuShellViewModel(
        ISessionService sessionService,
        MenuRegionState region,
        INavigationService<MenuRegionState> navigation,
        NotificationViewModel notification,
        IPolicyFactory policyFactory, ShellHost host, 
        IAndroidGetFcmToken getFcmToken, INotificationService notificationService)
    {
        _sessionService = sessionService;
        Region = region;
        _navigation = navigation;
        _host = host;
        _getFcmToken = getFcmToken;
        _notificationService = notificationService;


        _policy = policyFactory.CreatePolicy();
        
        Initialize(sessionService, notificationService, notification);
    }
    


    private void Initialize(ISessionService sessionService,
        INotificationService notificationService,
        NotificationViewModel notification)
    {

        CanSeeEmployees = _policy.CanSeeEmployees;
        CanSeeDocs = _policy.CanSeeDocs;
        CanSeeApprovalRoutes = _policy.CanSeeApprovalRoutes;
        
        IsNotificationsVisible = OperatingSystem.IsAndroid() && CanSeeDocs;

        notificationService.NotificationReceived += NotificationReceived;
        notificationService.Initialized += (_, _) =>
        {
            NotificationsCount = _notificationService.UnreadCount;;
        };
        
        FullName = sessionService.CurrentUser.FullName;
        Role = sessionService.CurrentUser.RoleName;
        
        Notification = notification;

        switch (_sessionService.CurrentUser.RoleId)
        {
            case 2:
                _navigation.NavigateTo<UserListViewModel>();
                break;
            case 3:
                _navigation.NavigateTo<ApprovalRoutesListViewModel>();
                break;
            default:
                DocumentsListViewModel.Mode = "all";
                _navigation.NavigateTo<DocumentsListViewModel>();
                break;
        }
    }

    private void NotificationReceived(object? sender, EventArgs e)
    {
        var count = _notificationService.UnreadCount;
        NotificationsCount = count;
    }
    
    [RelayCommand]
    private void TogglePane() => IsPaneOpen = !IsPaneOpen;

    [RelayCommand]
    private void Open() => IsOpen = !IsOpen;

    [RelayCommand]
    private void NavigateDirector()
    {
        _navigation.NavigateTo<UserListViewModel>();
    }

    [RelayCommand]
    private void NavigatePublicDocs()
    {
        DocumentsListViewModel.Mode = "all";
        _navigation.NavigateTo<DocumentsListViewModel>();
    }
    [RelayCommand]
    private void NavigateIncomingDocs()
    {
        DocumentsListViewModel.Mode = "incoming";
        _navigation.NavigateTo<DocumentsListViewModel>();
    }

    [RelayCommand]
    private void NavigateNotifications()
    {
        _navigation.NavigateTo<NotificationViewModel>();
    }
    [RelayCommand]
    private void ToggleNotificationPane()
    {
        IsNotificationPaneOpen = !IsNotificationPaneOpen;
        _notificationService.MarkRead();
    }
    [RelayCommand]
    private void NavigateNewDoc()
    {
        _navigation.NavigateTo<UploadDocumentViewModel>();
    }

    [RelayCommand]
    private void NavigateMyDocs()
    {
        MyDocumentsListViewModel.Mode = "my";
        _navigation.NavigateTo<MyDocumentsListViewModel>();
    }

    [RelayCommand]
    private void NavigateSettings()
    {
        _navigation.NavigateTo<SettingsViewModel>();
    }

    [RelayCommand]
    private void NavigateRoutes()
    {
        _navigation.NavigateTo<ApprovalRoutesListViewModel>();
    }

    [RelayCommand]
    private void NavigateScanner()
    {
        _navigation.NavigateTo<CameraViewModel>();
    }


    [RelayCommand]
    private void SignOut()
    {
        _sessionService.SignOutAsync();
        _host.ShowStartup();
    }
}