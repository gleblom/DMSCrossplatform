using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Services;

namespace DMSCrossplatform.ViewModels;

public partial class ShellViewModel: ViewModelBase
{
    private readonly INavigationService _navigation;
    private readonly ISessionService _session;
    private readonly IAuthService _auth;

    [ObservableProperty] private ViewModelBase? _currentView;

    public ShellViewModel(INavigationService navigation, ISessionService session, IAuthService auth)
    {
        _auth = auth;
        _navigation = navigation;
        _session = session;
        _navigation.CurrentChanged += (_, _) => CurrentView = _navigation.Current;
        _session.AuthStateChanged += OnAuthStateChanged;
        
        session.LoadStoredAsync();
        
        
        _navigation.NavigateTo<LoginViewModel>();
    }

    private async Task LoadUserInfo()
    {
        _session.CurrentUser = await _auth.GetMeAsync();
    }

    private async void OnAuthStateChanged(object? sender, EventArgs e)
    {
        if (!_session.IsAuthenticated)
            _navigation.NavigateTo<LoginViewModel>();


        await Task.WhenAll(LoadUserInfo());
        
        if(_session.CurrentUser.RoleId == 1 && _session.CurrentUser.CompanyId == null)
            _navigation.NavigateTo<CompanyCreateViewModel>();
        
        if(_session.CurrentUser.FirstName == null  || 
           _session.CurrentUser.SecondName == null  ||
           _session.CurrentUser.ThirdName == null )
            _navigation.NavigateTo<ProfileCreateViewModel>();

        _navigation.NavigateTo<MenuShellViewModel>();
        _navigation.CurrentChanged -= (_, _) => CurrentView = _navigation.Current;
        
    }
}