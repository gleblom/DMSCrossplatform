using System;
using System.Net;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.Services;
using Microsoft.Extensions.Logging;

namespace DMSCrossplatform.ViewModels;

public partial class ValidateOtpViewModel: ViewModelBase
{
    private readonly IAuthService _auth;
    private readonly ISessionService _session;
    private readonly INavigationService<StartupRegionState>  _navigation;
    private readonly AppSettings _settings;
    private readonly ILogger<ValidateOtpViewModel> _log;
    private readonly ShellHost _shellHost;
    
    
    [ObservableProperty] private string? _token = string.Empty;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private DateTime? _lockedUntil;
    
    private int _failedAttempts;
    
    public ValidateOtpViewModel(
        IAuthService auth, ISessionService session, 
        INavigationService<StartupRegionState> navigation, 
        AppSettings settings, ILogger<ValidateOtpViewModel> log, 
        ShellHost shellHost)
    {
        _auth = auth;
        _session = session;
        _navigation = navigation;
        _settings = settings;
        _log = log;
        _shellHost = shellHost;
    }

    [RelayCommand]
    public async Task SignIn()
    {
        ErrorMessage = null;
        if (LockedUntil.HasValue && DateTime.UtcNow < LockedUntil.Value)
        {
            var seconds = (int)(LockedUntil.Value - DateTime.UtcNow).TotalSeconds;
            ErrorMessage = $"Слишком много неудачных попыток. Попробуйте снова через {seconds} с.";
            return;
        }

        try
        {
            IsBusy = true;
            var token = await _auth.ValidateOtpAsync(new OtpVerifyDto
            {
                Token = Token,
                OtpBase32 = ""
            });
            await _session.SignInAsync(_session.CurrentUser.Email, token);
            
            _session.CurrentUser = await _auth.GetMeAsync();
            
            _failedAttempts = 0;
            
            _shellHost.ShowMenu();
        }
        catch (ApiException ex)
        {
            _failedAttempts++;
            _log.LogWarning(ex, "OTP auth failed ");

            if (_failedAttempts >= _settings.MaxLoginAttempts)
            {
                LockedUntil = DateTime.UtcNow.AddSeconds(_settings.LoginLockoutSeconds);
                ErrorMessage = $"Превышено число попыток входа. Блокировка на {_settings.LoginLockoutSeconds} с.";
                _failedAttempts = 0;
            }
            else
            {
                ErrorMessage = ex.StatusCode switch
                {
                    HttpStatusCode.NotFound or HttpStatusCode.Unauthorized => "Неверный код",
                    HttpStatusCode.Locked => "Превышено число попыток входа. Попробуйте позже",
                    _ => ErrorMessage
                };
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
    public void Cancel()
    {
        _navigation.NavigateTo<LoginViewModel>();
    }
    
}