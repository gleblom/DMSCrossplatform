using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserFullDto>> GetAllAsync(CancellationToken ct = default);
    Task<ProfileDto> CreateProfileAsync(ProfileDto dto, CancellationToken ct = default);
    Task<ProfileDto> UpdateProfileAsync(ProfileDto dto, CancellationToken ct = default);
    Task UpdateUserCompanyAsync(UserUpdateDto dto, CancellationToken ct = default);
}