using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public class AuthService : IAuthService
{
    private readonly IApiClient _api;

    public AuthService(IApiClient api) => _api = api;

    public Task<UserReadDto> RegisterAsync(UserCreateDto dto, CancellationToken ct = default)
        => _api.PostJsonAsync<UserCreateDto, UserReadDto>("/api/auth/register", dto, ct);

    public Task<UserTokenDto> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", email),
            new KeyValuePair<string, string>("password", password)
        });
        return _api.PostUrlEncodedAsync<UserTokenDto>("/api/auth/login", content, ct);
    }

    public Task<UserTokenDto> RefreshAsync(string refreshToken, CancellationToken ct = default)
        => _api.PostJsonAsync<object, UserTokenDto>("/api/auth/refresh",
            new { refresh_token_value = refreshToken }, ct);

    public Task LogoutAsync(string refreshToken, CancellationToken ct = default)
        => _api.PostJsonAsync<object>("/api/auth/logout", new { refresh_token_value = refreshToken }, ct);

    public Task<UserFullDto> GetMeAsync(CancellationToken ct = default)
        => _api.GetAsync<UserFullDto>("/api/auth/me", ct);

    public Task ForgotPasswordAsync(string email, CancellationToken ct = default)
        => _api.PostJsonAsync<object>($"/api/auth/forgot-password?email={System.Uri.EscapeDataString(email)}", new { }, ct);

    public Task ResetPasswordAsync(string token, string password, CancellationToken ct = default)
        => _api.PostJsonAsync<object>(
            $"/api/auth/reset-password?token={System.Uri.EscapeDataString(token)}&password={System.Uri.EscapeDataString(password)}",
            new { }, ct);

    public Task ConfirmEmailAsync(string token, CancellationToken ct = default)
        => _api.GetAsync<object>($"/api/auth/confirm-email?token={System.Uri.EscapeDataString(token)}", ct);
    
    public Task<OtpDto> GenerateOtpAsync(CancellationToken ct = default)
        => _api.PostJsonAsync<object, OtpDto>("/api/auth/otp/generate", new { }, ct);
    
    public Task ConfirmOtpAsync(OtpVerifyDto otp, CancellationToken ct = default)
        => _api.PostJsonAsync<object>("/api/auth/otp/confirm", new {token = otp.Token, otp_base32 = otp.OtpBase32}, ct);
    
    public Task<UserTokenDto> ValidateOtpAsync(OtpVerifyDto otp, CancellationToken ct = default)
        => _api.PostJsonAsync<object, UserTokenDto>("/api/auth/otp/validate", new { token = otp.Token, otp_base32 = otp.OtpBase32}, ct);

    public Task DisableOtpAsync(CancellationToken ct = default)
        => _api.PostJsonAsync<object>("/api/auth/opt/validate", new {}, ct);

    public Task<WebAuthnOptionsResponseDto> WebauthnRegisterOptionsAsync(CancellationToken ct = default)
        => _api.PostJsonAsync<object, WebAuthnOptionsResponseDto>("/api/auth/webauthn/register/options", new {}, ct);

    public Task WebauthnRegisterFinishAsync(WebAuthnFinishRequestDto finishDto, CancellationToken ct = default)
        => _api.PostJsonAsync<WebAuthnFinishRequestDto, object>("/api/auth/webauthn/register/finish", finishDto, ct);

    public Task<WebAuthnLoginOptionsResponseDto> WebauthnLoginOptionsAsync(CancellationToken ct = default)
        => _api.PostJsonAsync<object, WebAuthnLoginOptionsResponseDto>("/api/auth/webauthn/login/options", new { }, ct);

    public Task<UserTokenDto> WebauthnLoginFinishAsync(WebAuthnFinishRequestDto finishDto, CancellationToken ct = default)
        => _api.PostJsonAsync<WebAuthnFinishRequestDto, UserTokenDto>("/api/auth/webauthn/login/finish", finishDto, ct);

    public Task<PasskeyStatusDto> GetPasskeyStatusAsync(string deviceId, CancellationToken ct = default)
        => _api.GetAsync<PasskeyStatusDto>($"/api/auth/webauthn/status/", ct);

    public Task<IReadOnlyList<PasskeyCredentialDto>> GetPasskeysAsync(CancellationToken ct = default)
        => _api.GetAsync<IReadOnlyList<PasskeyCredentialDto>>("/api/auth/webauthn/credentials", ct);

    public Task<IReadOnlyList<string>> RevokePasskeyAsync(string credentialId, CancellationToken ct = default)
        => _api.PostJsonAsync<object, IReadOnlyList<string>>($"/api/auth/webauthn/credentials/{credentialId}/revoke", new {}, ct);

    public Task<IReadOnlyList<string>> EnablePasskeyAsync(string credentialId, CancellationToken ct = default)
        => _api.PostJsonAsync<object, IReadOnlyList<string>>($"/api/auth/webauthn/credentials/{credentialId}/enable", new {}, ct);
}