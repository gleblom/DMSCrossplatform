using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Infrastructure.Validation;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DMSCrossplatform.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _auth;
    private readonly ISessionService _session;
    private readonly INavigationService<StartupRegionState>  _navigation;
    private readonly AppSettings _settings;
    private readonly ILogger<LoginViewModel> _log;
    private readonly ShellHost _shellHost;
    private readonly IWebAuthnClient _webAuthnClient;
    

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private DateTime? _lockedUntil;

    private int _failedAttempts;

    public LoginViewModel(IAuthService auth, ISessionService session, 
        ShellHost shellHost, INavigationService<StartupRegionState> nav,
        AppSettings settings, ILogger<LoginViewModel> log, IWebAuthnClient webAuthnClient)
    {
        _auth = auth;
        _session = session;
        _shellHost = shellHost;
        _navigation = nav;
        _settings = settings;
        _log = log;
        _webAuthnClient = webAuthnClient;
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
            var userToken = _auth.LoginAsync(Email, Password);

            await Task.WhenAll(userToken);


            if (string.IsNullOrEmpty(userToken.Result.RefreshToken))
            {
                _session.CurrentUser = new UserFullDto
                {
                    Email = Email
                };
                _session.AccessToken = userToken.Result.AccessToken;
                _navigation.NavigateTo<ValidateOtpViewModel>();
                return;
            }
            
            await _session.SignInAsync(Email, userToken.Result);
            _failedAttempts = 0;
            _session.CurrentUser = await _auth.GetMeAsync();

            if (_session.CurrentUser.RoleId == 1 && _session.CurrentUser.CompanyId == null)
            {
                _navigation.NavigateTo<CompanyCreateViewModel>();
                return;
            }

            if (_session.CurrentUser.FirstName == null ||
                _session.CurrentUser.SecondName == null ||
                _session.CurrentUser.ThirdName == null)
            {
                _navigation.NavigateTo<ProfileCreateViewModel>();
                 return;
            }
            _shellHost.ShowMenu();
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
                ErrorMessage = ex.StatusCode == HttpStatusCode.Unauthorized
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
    private async Task PasskeySignIn()
    {
        ErrorMessage = null;
        try
        {
            IsBusy = true;
            var webautn = await _auth.WebauthnLoginOptionsAsync();

            var jsonOptions = JsonSerializer.Serialize(webautn.Options);
            var osResponse = await _webAuthnClient.AuthenticateAsync(jsonOptions);

            var userToken = await _auth.WebauthnLoginFinishAsync(new WebAuthnFinishRequestDto
            {
                ChallengeId = webautn.ChallengeId,
                Credential = JToken.Parse(osResponse)
            });
            await _session.SignInAsync(Email, userToken);
            _session.CurrentUser = await _auth.GetMeAsync();
            _shellHost.ShowMenu();
        }
        catch (ApiException ex)
        {
            ErrorMessage = "Ошибка при входе с помощью ключа безопасности";
            _log.LogWarning(ex, "Ошибка регистрации ключа безопасности");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Login unexpected error");
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