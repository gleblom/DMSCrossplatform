using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Infrastructure.Policy;
using DMSCrossplatform.Services;
using DMSCrossplatform.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DMSCrossplatform.ViewModels;
public sealed class MenuRegionState : ViewState { }

public partial class MenuShellViewModel : ViewModelBase
{
    private readonly INavigationService<MenuRegionState> _navigation;
    private readonly ShellHost _host;
    private readonly IAuthService _authService;
    private readonly ISessionService _sessionService;
    private readonly IPolicy _policy;
    
    [ObservableProperty] private bool _isPaneOpen;
    
    [ObservableProperty] private bool _canSeeDocs;
    [ObservableProperty] private bool _canSeeEmployees;
    [ObservableProperty] private bool _canSeeApprovalRoutes;
    [ObservableProperty] private string _fullName;
    [ObservableProperty] private string _role;
    [ObservableProperty] private SettingsViewModel _settings;
    [ObservableProperty] private bool _isOpen;

    public MenuRegionState Region { get; }

    public MenuShellViewModel(
        ISessionService sessionService,
        MenuRegionState region,
        INavigationService<MenuRegionState> navigation,
        IPolicyFactory policyFactory, ShellHost host, IAuthService authService)
    {
        _sessionService = sessionService;
        Region = region;
        _navigation = navigation;
        _host = host;
        _authService = authService;

        _policy = policyFactory.CreatePolicy();

        CanSeeEmployees = _policy.CanSeeEmployees;
        CanSeeDocs = _policy.CanSeeDocs;
        CanSeeApprovalRoutes = _policy.CanSeeApprovalRoutes;
        
        FullName = sessionService.CurrentUser.FullName;
        Role = sessionService.CurrentUser.RoleName;
        
        
        Settings = App.Services.GetRequiredService<SettingsViewModel>();

        switch (_sessionService.CurrentUser.RoleId)
        {
            case 2:
                _navigation.NavigateTo<UserListViewModel>();
                break;
            case 3:
                _navigation.NavigateTo<ApprovalRoutesListViewModel>();
                break;
            default:
                _navigation.NavigateTo<DocumentsListViewModel>();
                break;
        }
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
        _navigation.NavigateTo<DocumentsListViewModel>();
    }
    [RelayCommand]
    private void NavigateIncomingDocs()
    {
        DocumentsListViewModel.SwitchMode();
    }
    [RelayCommand]
    private void NavigateNewDoc()
    {
        _navigation.NavigateTo<UploadDocumentViewModel>();
    }

    [RelayCommand]
    private void NavigateMyDocs()
    {
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
    private void SignOut()
    {
        _sessionService.SignOutAsync();
        _host.ShowStartup();
    }
}