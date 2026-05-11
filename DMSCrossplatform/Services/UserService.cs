using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public class UserService : IUserService
{
    private readonly IApiClient _api;
    public UserService(IApiClient api) => _api = api;

    public ObservableCollection<UserFullDto> Users { get;  set; }

    public async Task<IReadOnlyList<UserFullDto>> GetAllAsync(
        string? userName = null, List<int>? units = null,
        List<int>? roles = null,
        CancellationToken ct = default)
    {
        var query = new List<string>();
        if(!string.IsNullOrEmpty(userName)) query.Add($"user_name={Uri.EscapeDataString(userName)}");
        if(units != null && units.Count != 0) query.Add(string.Join("&" , units.Select(u => $"units={u}")));
        if(roles != null && roles.Count !=0) query.Add(string.Join("&" , roles.Select(u => $"roles={u}")));
        var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";
        
        var users = await _api.GetAsync<List<UserFullDto>>($"/api/users/all/{qs}", ct);
        
        Users = new ObservableCollection<UserFullDto>(users);
        
        return Users;
    }

    public Task<ProfileDto> CreateProfileAsync(ProfileDto dto, CancellationToken ct = default)
        => _api.PutJsonAsync<ProfileDto, ProfileDto>("/api/users/user/profile", dto, ct);

    public Task<ProfileDto> UpdateProfileAsync(ProfileDto dto, CancellationToken ct = default)
        => _api.PutJsonAsync<ProfileDto, ProfileDto>("/api/users/profile", dto, ct);

    public Task UpdateUserCompanyAsync(UserUpdateDto dto, CancellationToken ct = default)
        => _api.PutJsonAsync<UserUpdateDto, object>("/api/users/company", dto, ct);
}