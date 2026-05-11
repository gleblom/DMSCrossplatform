using System;
using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public interface ISessionService
{
    string? RefreshToken { get; set; }
    string? AccessToken { get; set; }
    UserFullDto? CurrentUser { get; set; }
    bool IsAuthenticated { get; }

    event EventHandler? AuthStateChanged;
    void ClearAccessToken();

    Task<string?> GetAccessTokenAsync();
    Task<bool> TryRefreshAsync(CancellationToken ct = default);
    Task SignInAsync(string email, UserTokenDto token);
    Task SignOutAsync();
    Task LoadStoredAsync();
}