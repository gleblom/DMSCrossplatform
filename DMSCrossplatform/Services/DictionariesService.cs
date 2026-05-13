using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public class DictionariesService: IDictionariesService
{
    private readonly IApiClient _api;
    public DictionariesService(IApiClient api) => _api = api;

    public ObservableCollection<RoleReadDto> Roles { get; set; }
    public ObservableCollection<UnitReadDto> Units { get; set; }

    public async Task<IReadOnlyList<RoleReadDto>> GetRolesAsync(CancellationToken ct = default)
    {
      var roles = await _api.GetAsync<List<RoleReadDto>>("/api/dictionaries/roles", ct);
      
      Roles = new ObservableCollection<RoleReadDto>(roles);
      return Roles;
    }

    public Task<RoleReadDto> CreateRoleAsync(RoleCreateDto dto, CancellationToken ct = default)
        => _api.PostJsonAsync<RoleCreateDto, RoleReadDto>("/api/dictionaries/roles", dto, ct);

    public Task<RoleReadDto> UpdateRoleAsync(int id, RoleUpdateDto dto, CancellationToken ct = default)
        => _api.PutJsonAsync<RoleUpdateDto, RoleReadDto>($"/api/dictionaries/roles/{id}", dto, ct);

    public Task DeleteRoleAsync(int id, CancellationToken ct = default)
        => _api.DeleteAsync($"/api/dictionaries/roles/{id}", ct);

    public async Task<IReadOnlyList<UnitReadDto>> GetUnitsAsync(CancellationToken ct = default)
    {
        var units = await _api.GetAsync<List<UnitReadDto>>("/api/dictionaries/units", ct);
        Units = new ObservableCollection<UnitReadDto>(units);
        return Units;
    }

    public Task<UnitReadDto> CreateUnitAsync(UnitCreateDto dto, CancellationToken ct = default)
        => _api.PostJsonAsync<UnitCreateDto, UnitReadDto>("/api/dictionaries/units", dto, ct);

    public Task<UnitReadDto> UpdateUnitAsync(int id, UnitUpdateDto dto, CancellationToken ct = default)
        => _api.PutJsonAsync<UnitUpdateDto, UnitReadDto>($"/api/dictionaries/units/{id}", dto, ct);

    public Task DeleteUnitAsync(int id, CancellationToken ct = default)
        => _api.DeleteAsync($"/api/dictionaries/units/{id}", ct);

    public Task<UnitReadDto> AttachUnitToCompanyAsync(int unitId, Guid companyId, CancellationToken ct = default)
        => _api.PostJsonAsync<object, UnitReadDto>(
            $"/api/dictionaries/units/{unitId}/companies/{companyId}", new { }, ct);

    public Task DetachUnitFromCompanyAsync(int unitId, Guid companyId, CancellationToken ct = default)
        => _api.DeleteAsync($"/api/dictionaries/units/{unitId}/companies/{companyId}", ct);

    public async Task<IReadOnlyList<RoleCategoryReadDto>> AddRoleCategoryAsync(RoleCategoryDto dto, CancellationToken ct = default)
        => await _api.PostJsonAsync<RoleCategoryDto, List<RoleCategoryReadDto>>("/api/dictionaries/role_categories", dto, ct);

    public async Task<IReadOnlyList<RoleCategoryReadDto>> UpdateRoleCategoryAsync(RoleCategoryDto dto, CancellationToken ct = default)
        => await _api.PostJsonAsync<RoleCategoryDto, List<RoleCategoryReadDto>>(
            "/api/dictionaries/update/role_categories", dto, ct);
    
    public async Task<IReadOnlyList<RoleCategoryReadDto>> GetRoleCategoriesAsync(int? id, CancellationToken ct = default)
        => await _api.GetAsync<List<RoleCategoryReadDto>>($"/api/dictionaries/role_categories/{id}", ct);
    
    public async Task<IReadOnlyCollection<SimpleDto>> GetCategoriesAsync(CancellationToken ct = default)
        => await _api.GetAsync<List<SimpleDto>>("/api/dictionaries/categories", ct);

    public async Task<IReadOnlyCollection<SimpleDto>> GetStatusesAsync(CancellationToken ct = default)
        => await _api.GetAsync<List<SimpleDto>>("/api/dictionaries/statuses", ct);

    public async Task<IReadOnlyCollection<SimpleDto>> GetUnitsSimpleAsync(CancellationToken ct = default)
    {
        var units = await GetUnitsAsync(ct);
        return units.Select(u => new SimpleDto { Id = u.Id, Name = u.Name }).ToList();
    }
}