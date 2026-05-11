using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Infrastructure;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Infrastructure.Storage;
using DMSCrossplatform.Models.Dto;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DMSCrossplatform.Services;

public class SessionService : ISessionService
{
    private readonly ITokenStorage _storage;
    private readonly AppSettings _settings;
    private readonly ILogger<SessionService> _log;

    private string? _email;
    
    public string? RefreshToken { get; set; }

    public string? AccessToken { get; set; }

    public UserFullDto? CurrentUser { get; set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

    public event EventHandler? AuthStateChanged;

    public SessionService(ITokenStorage storage, AppSettings settings, ILogger<SessionService> log)
    {
        
        _storage = storage;
        _settings = settings;
        _log = log;
    }
    
    public Task<string?> GetAccessTokenAsync() => Task.FromResult(AccessToken);

    public void ClearAccessToken() => AccessToken = null;
    
    public async Task LoadStoredAsync()
    {
        var session = await _storage.GetActiveAsync();
        if (session is null) return;
        _email = session.Email;
        AccessToken = session.AccessToken;
        RefreshToken = session.RefreshToken;
        AuthStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task SignInAsync(string email, UserTokenDto token)
    {
        _email = email;
        AccessToken = token.AccessToken;
        RefreshToken = token.RefreshToken;
        await _storage.SaveAsync(new StoredSession(email, token.AccessToken, token.RefreshToken, token.TokenType));
        await _storage.SetActiveAsync(email);
        AuthStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task SignOutAsync()
    {
        if (_email is not null)
            await _storage.RemoveAsync(_email);
        _email = null;
        AccessToken = null;
        RefreshToken = null;
        CurrentUser = null;
        await _storage.ClearActiveAsync();
        // AuthStateChanged?.Invoke(this, EventArgs.Empty);
    }
    

    public async Task<bool> TryRefreshAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(RefreshToken) || _email is null) return false;
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(_settings.ApiBaseUrl) };
            var body = JsonConvert.SerializeObject(new { refresh_token_value = RefreshToken });
            using var req = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh")
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
            };
            using var resp = await http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode) return false;
            var json = await resp.Content.ReadAsStringAsync(ct);
            var token = JsonConvert.DeserializeObject<UserTokenDto>(json);
            if (token is null) return false;
            await SignInAsync(_email, token);
            return true;
        }
        catch (Exception e)
        {
            _log.LogWarning(e, "Refresh token failed");
            return false;
        }
    }
}