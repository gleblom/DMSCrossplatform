using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public interface IAuthService
{
    Task<UserReadDto> RegisterAsync(UserCreateDto dto, CancellationToken ct = default);
    Task<UserTokenDto> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<UserTokenDto> RefreshAsync(string refreshToken, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task<UserFullDto> GetMeAsync(CancellationToken ct = default);
    Task ForgotPasswordAsync(string email, CancellationToken ct = default);
    Task ResetPasswordAsync(string token, string password, CancellationToken ct = default);
    Task ConfirmEmailAsync(string token, CancellationToken ct = default);
    Task<OtpDto> GenerateOtpAsync(CancellationToken ct = default);

    Task ConfirmOtpAsync(OtpVerifyDto otp, CancellationToken ct = default);

    Task<UserTokenDto> ValidateOtpAsync(OtpVerifyDto otp, CancellationToken ct = default);
    Task DisableOtpAsync(CancellationToken ct = default);
    
    Task<WebAuthnOptionsResponseDto> WebauthnRegisterOptionsAsync(CancellationToken ct = default);
    
    Task WebauthnRegisterFinishAsync(WebAuthnFinishRequestDto finishDto,  CancellationToken ct = default);

    Task<WebAuthnLoginOptionsResponseDto> WebauthnLoginOptionsAsync(CancellationToken ct = default);
    
    Task<UserTokenDto> WebauthnLoginFinishAsync(WebAuthnFinishRequestDto finishDto, CancellationToken ct = default);
    
    Task<PasskeyStatusDto> GetPasskeyStatusAsync(string deviceId, CancellationToken ct = default);
    Task<IReadOnlyList<PasskeyCredentialDto>> GetPasskeysAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> RevokePasskeyAsync(string credentialId, CancellationToken ct = default);

    Task<IReadOnlyList<string>> EnablePasskeyAsync(string credentialId, CancellationToken ct = default);
}