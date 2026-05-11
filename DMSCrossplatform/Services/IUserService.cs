using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public interface IUserService
{
    ObservableCollection<UserFullDto> Users { get; set; }
    Task<IReadOnlyList<UserFullDto>> GetAllAsync(string? userName = null, List<int>? units = null, List<int>? roles = null, CancellationToken ct = default);
    Task<ProfileDto> CreateProfileAsync(ProfileDto dto, CancellationToken ct = default);
    Task<ProfileDto> UpdateProfileAsync(ProfileDto dto, CancellationToken ct = default);
    Task UpdateUserCompanyAsync(UserUpdateDto dto, CancellationToken ct = default);
}