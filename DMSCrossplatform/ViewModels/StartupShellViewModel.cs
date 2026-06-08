using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Services;

namespace DMSCrossplatform.ViewModels;

public sealed class StartupRegionState : ViewState { }

public partial class StartupShellViewModel : ViewModelBase
{
    private readonly INavigationService<StartupRegionState> _navigation;
    private readonly ISessionService _session;
    private readonly IAuthService _auth;
    private readonly ShellHost _shellHost;

    public StartupRegionState Region { get; }

    public StartupShellViewModel(
        StartupRegionState region,
        ShellHost shellHost,
        INavigationService<StartupRegionState> navigation,
        ISessionService session,
        IAuthService auth)
    {
        Region = region;
        _shellHost = shellHost;
        _auth = auth;
        _navigation = navigation;
        _session = session;
        _session.AuthStateChanged += OnAuthStateChanged;

        _navigation.NavigateTo<LoginViewModel>();

        // Android debug startup is tight: encrypted storage is restored after the
        // first startup view is available, and UI changes are marshalled below.
        _ = Task.Run(session.LoadStoredAsync);
    }

    private async Task LoadUserInfo()
    {
        _session.CurrentUser = await _auth.GetMeAsync();
    }

    private async void OnAuthStateChanged(object? sender, EventArgs e)
    {
        if (!_session.IsAuthenticated)
        {
            await Dispatcher.UIThread.InvokeAsync(() => _navigation.NavigateTo<LoginViewModel>());
            return;
        }

        try
        {
            await LoadUserInfo();
        }
        catch
        {
            await Dispatcher.UIThread.InvokeAsync(() => _navigation.NavigateTo<LoginViewModel>());
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_session.CurrentUser?.RoleId == 1 && _session.CurrentUser.CompanyId == null)
            {
                _navigation.NavigateTo<CompanyCreateViewModel>();
                return;
            }

            if (_session.CurrentUser?.FirstName == null ||
                _session.CurrentUser.SecondName == null ||
                _session.CurrentUser.ThirdName == null)
            {
                _navigation.NavigateTo<ProfileCreateViewModel>();
                return;
            }

            _shellHost.ShowMenu();
        });
    }
}
