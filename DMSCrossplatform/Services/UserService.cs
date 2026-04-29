using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public class UserService : IUserService
{
    private readonly IApiClient _api;
    public UserService(IApiClient api) => _api = api;

    public async Task<IReadOnlyList<UserFullDto>> GetAllAsync(CancellationToken ct = default)
        => await _api.GetAsync<List<UserFullDto>>("/api/users/all", ct);

    public Task<ProfileDto> CreateProfileAsync(ProfileDto dto, CancellationToken ct = default)
        => _api.PutJsonAsync<ProfileDto, ProfileDto>("/api/users/user/profile", dto, ct);

    public Task<ProfileDto> UpdateProfileAsync(ProfileDto dto, CancellationToken ct = default)
        => _api.PutJsonAsync<ProfileDto, ProfileDto>("/api/users/profile", dto, ct);

    public Task UpdateUserCompanyAsync(UserUpdateDto dto, CancellationToken ct = default)
        => _api.PutJsonAsync<UserUpdateDto, object>("/api/users/company", dto, ct);
}