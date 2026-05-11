using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using QRCoder;

namespace DMSCrossplatform.ViewModels;

public partial class SettingsViewModel: ViewModelBase
{
    private readonly ILogger<SettingsViewModel> _log;
    private readonly IWebAuthnClient _webAuthnClient;
    private readonly IAuthService _authService;
    private readonly ISessionService _sessionService;
    
    
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private UserFullDto _user;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private Bitmap _qrCodeBitmap;
    [ObservableProperty] private bool _isVisible;
    [ObservableProperty] private string _secretCode;
    [ObservableProperty] private string _authCode;
    [ObservableProperty] private bool? _passkeyEnabled;
    [ObservableProperty] private bool? _otpEnabled;

    public SettingsViewModel(
        ILogger<SettingsViewModel> log,
        IAuthService authService,
        IWebAuthnClient webAuthnClient, 
        ISessionService sessionService)
    {
        _log = log;
        _authService = authService;
        _webAuthnClient = webAuthnClient;
        _sessionService = sessionService;
        _ = LoadUserAsync();
    }

    private async Task LoadUserAsync()
    {
        User = await _authService.GetMeAsync();
        PasskeyEnabled = User.PasskeyEnabled;
        OtpEnabled = User.OtpEnabled;
        _sessionService.CurrentUser = User;
    }

    private Bitmap GenerateQrCode(string otpAuthUrl)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(otpAuthUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(20);
        return new Bitmap(new MemoryStream(qrCodeBytes));
    }

    [RelayCommand]
    public async Task ActivateCommand()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            await _authService.ConfirmOtpAsync(new OtpVerifyDto
            {
                OtpBase32 = SecretCode,
                Token = AuthCode
            });
            IsVisible = false;
            await LoadUserAsync();
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.StatusCode switch
            {
                HttpStatusCode.NotFound or HttpStatusCode.Unauthorized => "Неверный код",
                HttpStatusCode.Locked => "Превышено число попыток входа. Попробуйте позже",
                _ => ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Ошибка верификации");
            ErrorMessage = "Неизвестная ошибка верификации";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void Cancel()
    {
        IsVisible = false;
    }

    [RelayCommand]
    public async Task TwoFactorAuthCommand()
    {
        ErrorMessage = null;
        switch (User.OtpEnabled)
        {
            case false:
                IsVisible = true;
                try
                {
                    var otp = await _authService.GenerateOtpAsync();
                    QrCodeBitmap = GenerateQrCode(otp.OtpAuthUrl);
                    SecretCode = otp.OtpBase32;

                }
                catch (ApiException ex)
                {
                    _log.LogWarning(ex, "Ошибка при получении OTP");
                    ErrorMessage = "Ошибка при получении OTP";
                }
                catch (Exception e)
                {
                    _log.LogWarning(e, "Неизвестная ошибка при получении OTP");
                    ErrorMessage = e.Message;
                }
                break;
            case true:
                try
                {
                    await _authService.GenerateOtpAsync();
                }
                catch (ApiException ex)
                {
                    _log.LogWarning(ex, "Ошибка отвязки 2FA");
                    ErrorMessage = "Ошибка отвязки 2FA";
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Ошибка отвязки 2FA");
                    ErrorMessage = ex.Message;
                }
                break;
        }
    }

    [RelayCommand]
    public async Task EnablePasskey()
    {
        ErrorMessage = null;
        try
        {
            var webautn = await _authService.WebauthnRegisterOptionsAsync();
            var jsonOptions = JsonSerializer.Serialize(webautn.Options);
            var osResponse = await _webAuthnClient.RegisterAsync(jsonOptions);


            await _authService.WebauthnRegisterFinishAsync(new WebAuthnFinishRequestDto
            {
                ChallengeId = webautn.ChallengeId,
                Credential = JToken.Parse(osResponse)
            });
            User = await _authService.GetMeAsync();
        }
        catch (ApiException ex)
        {
            ErrorMessage = "Ошибка регистрации ключа безопасности";
            _log.LogWarning(ex, "Ошибка регистрации ключа безопасности");
        }
        catch (Exception ex)
        {

            _log.LogWarning(ex, "Операция отменена пользователем");
        }
    }
}