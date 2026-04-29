using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Infrastructure.Validation;
using DMSCrossplatform.Services;
using Microsoft.Extensions.Logging;

namespace DMSCrossplatform.ViewModels;


public partial class ForgotPasswordViewModel : ViewModelBase
{
    private readonly IAuthService _auth;
    private readonly INavigationService _nav;
    private readonly ILogger<ForgotPasswordViewModel> _log;
    private readonly AppSettings _settings;

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _resetToken = string.Empty;
    [ObservableProperty] private string _newPassword = string.Empty;
    [ObservableProperty] private bool _tokenSent;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _successMessage;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private DateTime? _lockedUntil;
    
    private int _attempts;

    public ForgotPasswordViewModel(IAuthService auth, 
        INavigationService nav, 
        AppSettings settings,
        ILogger<ForgotPasswordViewModel> log)
    {
        _auth = auth;
        _nav = nav;
        _settings = settings;
        _log = log;
    }

    [RelayCommand]
    private async Task RequestTokenAsync()
    {
        _attempts++;
        ErrorMessage = null;
        SuccessMessage = null;

        if (LockedUntil.HasValue && DateTime.UtcNow < LockedUntil.Value)
        {
            var seconds = (int)(LockedUntil.Value - DateTime.UtcNow).TotalSeconds;
            ErrorMessage = $"Слишком много попыток. Попробуйте снова через {seconds} с.";
            return;
        }
        if (_attempts >= _settings.MaxLoginAttempts)
        {
            LockedUntil = DateTime.UtcNow.AddSeconds(_settings.LoginLockoutSeconds);
            ErrorMessage = $"Превышено число попыток получения токена восстановления. Блокировка на {_settings.LoginLockoutSeconds} с.";
            _attempts = 0;
        }
        
        var err = Validators.ValidateEmail(Email);
        if (err is not null) { ErrorMessage = err; return; }

        try
        {
            IsBusy = true;
            await _auth.ForgotPasswordAsync(Email);
            TokenSent = true;
            SuccessMessage = "Если аккаунт с такой почтой существует, то на него будет отправлен токен восстановления";
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Forgot password failed");
            ErrorMessage = "Не удалось отправить запрос восстановления.";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ResetAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(ResetToken)) { ErrorMessage = "Введите токен."; return; }
        var err = Validators.ValidatePassword(NewPassword);
        if (err is not null) { ErrorMessage = err; return; }

        try
        {
            IsBusy = true;
            await _auth.ResetPasswordAsync(ResetToken, NewPassword);
            SuccessMessage = "Пароль успешно обновлён.";
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Reset password failed");
            ErrorMessage = "Не удалось сбросить пароль. Проверьте токен.";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void Cancel() => _nav.GoBack();
}