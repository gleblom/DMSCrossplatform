using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure;
using DMSCrossplatform.Infrastructure.Android;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Infrastructure.Storage;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.Services;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly IPushService  _pushService;
    private readonly IAndroidActivityHost _androidActivityHost;
    private readonly IAndroidGetFcmToken _androidGetFcmToken;
    private readonly IAndroidPermissionRequester  _androidPermissionRequester;
    private readonly IAndroidPasskeySignalSync? _androidPasskeySignalSync;
    private readonly IWindowsGetChannelUri  _windowsGetChannelUri;
    private readonly IDeviceIdentityStore _deviceIdentityStore;
    private readonly AppSettings _settings;
    private string? _deviceId;
    
    
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private UserFullDto _user;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private Bitmap _qrCodeBitmap;
    [ObservableProperty] private bool _isVisible;
    [ObservableProperty] private string _secretCode;
    [ObservableProperty] private string _authCode;
    [ObservableProperty] private bool? _passkeyEnabled;
    [ObservableProperty] private bool? _otpEnabled;
    [ObservableProperty] private bool? _isPushEnabled;
    [ObservableProperty] private bool? _isDevicePasskeyEnabled;
    [ObservableProperty] private ObservableCollection<PasskeyCredentialDto> _credentials;

    public SettingsViewModel(
        ILogger<SettingsViewModel> log,
        IAuthService authService,
        IWebAuthnClient webAuthnClient, 
        ISessionService sessionService, 
        IPushService pushService,
        IWindowsGetChannelUri windowsGetChannelUri,
        IDeviceIdentityStore deviceIdentityStore,
        AppSettings settings
        )
    {
       
        _log = log;
        _authService = authService;
        _webAuthnClient = webAuthnClient;
        _sessionService = sessionService;
        _pushService = pushService;
        _deviceIdentityStore = deviceIdentityStore;
        _settings = settings;
        _ = LoadUserAsync();

        _windowsGetChannelUri = windowsGetChannelUri;

    }

    public SettingsViewModel(ILogger<SettingsViewModel> log,
        IAuthService authService,
        IWebAuthnClient webAuthnClient,
        ISessionService sessionService,
        IPushService pushService,
        IAndroidActivityHost host,
        IAndroidGetFcmToken  fcmToken,
        IAndroidPermissionRequester permissionRequester,
        IAndroidPasskeySignalSync androidPasskeySignalSync,
        IDeviceIdentityStore deviceIdentityStore,
        AppSettings settings
    )
    {
        _log = log;
        _authService = authService;
        _webAuthnClient = webAuthnClient;
        _sessionService = sessionService;
        _pushService = pushService;
        _deviceIdentityStore = deviceIdentityStore;
        _settings = settings;
        _ = LoadUserAsync();
        
        _androidActivityHost = host;
        _androidGetFcmToken = fcmToken;
        _androidPermissionRequester = permissionRequester;
        _androidPasskeySignalSync = androidPasskeySignalSync;
    }

    private async Task LoadUserAsync()
    {
        User = await _authService.GetMeAsync();
        OtpEnabled = User.OtpEnabled;
        _deviceId = await _deviceIdentityStore.GetOrCreateAsync();
        _sessionService.CurrentUser = User;
        await LoadDeviceSettingsAsync();
    }

    private async Task LoadDeviceSettingsAsync()
    {
        if (string.IsNullOrWhiteSpace(_deviceId))
            return;

        try
        {
            var pushStatus = await _pushService.GetDeviceStatus(_deviceId);
            IsPushEnabled = pushStatus.UserPushEnabled && pushStatus.DevicePushEnabled;
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            IsPushEnabled = false;
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Ошибка получения статуса push-уведомлений");
        }

        try
        {
            var credentials = await _authService.GetPasskeysAsync();
            Credentials = new ObservableCollection<PasskeyCredentialDto>(credentials);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Ошибка получения статуса ключа безопасности");
        }
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
    private async Task TogglePasskey(string credentialId)
    {
        ErrorMessage = null;
        try
        {
            var deviceId = await GetDeviceIdAsync();
            var webautn = await _authService.WebauthnRegisterOptionsAsync();

            JsonSerializerOptions options = null;

            if (OperatingSystem.IsAndroid())
            {
                options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };
            }

            var jsonOptions = JsonSerializer.Serialize(webautn.Options, options);

            var osResponse = await _webAuthnClient.RegisterAsync(jsonOptions);

            await _authService.WebauthnRegisterFinishAsync(new WebAuthnFinishRequestDto
            {
                ChallengeId = webautn.ChallengeId,
                Credential = JToken.Parse(osResponse),
                DeviceId = deviceId,
                DeviceName = GetDeviceName()
            });
            User = await _authService.GetMeAsync();
            
            await LoadUserAsync();
        }
        catch (ApiException ex)
        {
            _log.LogWarning(ex, "Ошибка регистрации ключа безопасности");
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Объект уже существует");
        }
        finally
        {
            _ = LoadDeviceSettingsAsync();
        }
    }

    [RelayCommand]
    private async Task TogglePush()
    {
        ErrorMessage = null;
        var targetState = IsPushEnabled == true;
        try
        {
            if (!targetState)
            {
             
                await DisablePushAsync();
                IsPushEnabled = false;
                return;
            }
            
            var deviceId = await GetDeviceIdAsync();
#if  Android

            if (OperatingSystem.IsAndroid())
            {
                Console.WriteLine($"Device target state: {deviceId}");
                var activity = _androidActivityHost.Current
                               ?? throw new InvalidOperationException("Android activity is not available.");
                var granted = await _androidPermissionRequester.RequestNotificationAsync(activity);
                if (!granted)
                {
                    ErrorMessage = "Уведомления не разрешены";
                    return;
                }
            
                var token = await _androidGetFcmToken.GetToken(activity);
                if (token == "empty")
                {
                    return;
                }
                Console.WriteLine($"FirebaseToken: {token}");
                await _pushService.RegisterDevice(new DeviceRegisterDto()
                {
                    DeviceId = deviceId,
                    Platform = "android",
                    PushToken = token,
                });
            }
#endif
            if (OperatingSystem.IsWindows())
            {
                var pushChannel = await _windowsGetChannelUri.GetPushChannel();
                if (pushChannel != null)
                {
                     await _pushService.RegisterDevice(new DeviceRegisterDto()
                    {
                        DeviceId = deviceId,
                        Platform = "windows",
                        PushToken = pushChannel
                    });
                }
            }

            await _pushService.UpdateDeviceStatus(deviceId, new DeviceUpdateDto { PushEnabled = true });
            IsPushEnabled = true;
        }
        catch(Exception ex)
        {
            IsPushEnabled = !targetState;
        }
    }

    private async Task DisablePushAsync()
    {
        var deviceId = await GetDeviceIdAsync();
        await _pushService.UpdatePushSettings(new PushSettingsUpdateDto { PushEnabled = false });

        try
        {
            await _pushService.UpdateDeviceStatus(deviceId, new DeviceUpdateDto { PushEnabled = false });
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
        }

#if  Android
        if (OperatingSystem.IsAndroid())
        {
            var activity = _androidActivityHost.Current
                           ?? throw new InvalidOperationException("Android activity is not available.");
            await _androidPermissionRequester.RevokeNotificationAsync(activity);
        }
#endif
    }

    [RelayCommand]
    private async Task DisablePasskey(string credentialId)
    {
        try
        {
            await _authService.RevokePasskeyAsync(credentialId);

            IReadOnlyList<string> activeCredentialIds = Credentials
                .Where(c => c.CredentialId == credentialId)
                .Select(c => c.CredentialId)
                .ToArray();
            if (OperatingSystem.IsAndroid() && _androidPasskeySignalSync is not null)
            {
                var rpId = GetRpId();
                await _androidPasskeySignalSync.SignalAcceptedIdsAsync(
                    rpId,
                    _sessionService.CurrentUser?.UserId,
                    activeCredentialIds,
                    CancellationToken.None);
            }

            await LoadDeviceSettingsAsync();
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex.Message);
        }
    }

    private async Task<string> GetDeviceIdAsync()
        => _deviceId ??= await _deviceIdentityStore.GetOrCreateAsync();

    private string GetDeviceName()
    {
        if (OperatingSystem.IsAndroid())
            return $"Android {Android.OS.Build.Model}";


        return Environment.MachineName;
    }

    private string GetRpId()
    {
        return "linuxserver.tailea0f78.ts.net";
    }
}
