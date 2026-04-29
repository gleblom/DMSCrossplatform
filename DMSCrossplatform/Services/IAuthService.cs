using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public interface IAuthService
{
    Task<UserReadDto> RegisterDirectorAsync(UserCreateDto dto, CancellationToken ct = default);
    Task<UserTokenDto> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<UserTokenDto> RefreshAsync(string refreshToken, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task<UserFullDto
    > GetMeAsync(CancellationToken ct = default);
    Task ForgotPasswordAsync(string email, CancellationToken ct = default);
    Task ResetPasswordAsync(string token, string password, CancellationToken ct = default);
    Task ConfirmEmailAsync(string token, CancellationToken ct = default);
}