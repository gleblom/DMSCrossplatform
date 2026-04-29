using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMSCrossplatform.Infrastructure.Storage;

public interface ITokenStorage
{
    Task<IReadOnlyList<StoredSession>> GetAllAsync();
    Task<StoredSession?> GetActiveAsync();
    Task SetActiveAsync(string email);
    Task SaveAsync(StoredSession session);
    Task RemoveAsync(string email);
    Task ClearActiveAsync();
}
public sealed record  StoredSession(
    string Email,
    string AccessToken,
    string RefreshToken,
    string TokenType
);