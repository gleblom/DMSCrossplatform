using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Infrastructure.Validation;
using DMSCrossplatform.Services;
using Microsoft.Extensions.Logging;

namespace DMSCrossplatform.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _auth;
    private readonly ISessionService _session;
    private readonly INavigationService _navigation;
    private readonly AppSettings _settings;
    private readonly ILogger<LoginViewModel> _log;

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private DateTime? _lockedUntil;

    private int _failedAttempts;

    public LoginViewModel(IAuthService auth, ISessionService session, INavigationService nav,
        AppSettings settings, ILogger<LoginViewModel> log)
    {
        _auth = auth;
        _session = session;
        _navigation = nav;
        _settings = settings;
        _log = log;
    }

    [RelayCommand]
    private async Task SignInAsync()
    {
        ErrorMessage = null;

        if (LockedUntil.HasValue && DateTime.UtcNow < LockedUntil.Value)
        {
            var seconds = (int)(LockedUntil.Value - DateTime.UtcNow).TotalSeconds;
            ErrorMessage = $"Слишком много неудачных попыток. Попробуйте снова через {seconds} с.";
            return;
        }

        var emailErr = Validators.ValidateEmail(Email);
        var passErr = Validators.ValidatePassword(Password);
        if (emailErr is not null) { ErrorMessage = emailErr; return; }
        if (passErr is not null) { ErrorMessage = passErr; return; }

        try
        {
            IsBusy = true;
            var token = await _auth.LoginAsync(Email, Password);
            await _session.SignInAsync(Email, token);
            _failedAttempts = 0;
            _session.CurrentUser = await _auth.GetMeAsync();
            if(_session.CurrentUser.RoleId == 1 && _session.CurrentUser.CompanyId == null)
                _navigation.NavigateTo<CompanyCreateViewModel>();
        
            if(_session.CurrentUser.FirstName == null || 
               _session.CurrentUser.SecondName == null  ||
               _session.CurrentUser.ThirdName == null )
                _navigation.NavigateTo<ProfileCreateViewModel>();
            _navigation.NavigateTo<MenuShellViewModel>();
        }
        catch (ApiException ex)
        {
            _failedAttempts++;
            _log.LogWarning(ex, "Login failed for {Email}", Email);

            if (_failedAttempts >= _settings.MaxLoginAttempts)
            {
                LockedUntil = DateTime.UtcNow.AddSeconds(_settings.LoginLockoutSeconds);
                ErrorMessage = $"Превышено число попыток входа. Блокировка на {_settings.LoginLockoutSeconds} с.";
                _failedAttempts = 0;
            }
            else
            {
                ErrorMessage = ex.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    ? "Неверный email или пароль."
                    : ex.Message;
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Login unexpected error");
            ErrorMessage = "Не удалось подключиться к серверу.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void GoToRegister() => _navigation.NavigateTo<RegisterViewModel>();

    [RelayCommand]
    private void GoToForgotPassword() => _navigation.NavigateTo<ForgotPasswordViewModel>();
}