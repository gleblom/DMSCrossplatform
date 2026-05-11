using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public interface IDictionariesService
{
    ObservableCollection<RoleReadDto> Roles { get; set; }
    ObservableCollection<UnitReadDto> Units { get; set; }
    
    
    
    Task<IReadOnlyList<RoleReadDto>> GetRolesAsync(CancellationToken ct = default);
    Task<RoleReadDto> CreateRoleAsync(RoleCreateDto dto, CancellationToken ct = default);
    Task<RoleReadDto> UpdateRoleAsync(int id, RoleUpdateDto dto, CancellationToken ct = default);
    Task DeleteRoleAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<UnitReadDto>> GetUnitsAsync(CancellationToken ct = default);
    Task<UnitReadDto> CreateUnitAsync(UnitCreateDto dto, CancellationToken ct = default);
    Task<UnitReadDto> UpdateUnitAsync(int id, UnitUpdateDto dto, CancellationToken ct = default);
    Task DeleteUnitAsync(int id, CancellationToken ct = default);
    Task<UnitReadDto> AttachUnitToCompanyAsync(int unitId, Guid companyId, CancellationToken ct = default);
    Task DetachUnitFromCompanyAsync(int unitId, Guid companyId, CancellationToken ct = default);

    Task<IReadOnlyList<RoleCategoryReadDto>> AddRoleCategoryAsync(RoleCategoryDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<RoleCategoryReadDto>> UpdateRoleCategoryAsync(RoleCategoryDto dto, CancellationToken ct = default);
    
    public Task<IReadOnlyList<RoleCategoryReadDto>> GetRoleCategoriesAsync(int? id, CancellationToken ct = default);
    public Task<IReadOnlyCollection<SimpleDto>> GetCategoriesAsync(CancellationToken ct = default);
    public Task<IReadOnlyCollection<SimpleDto>> GetStatusesAsync(CancellationToken ct = default);

    
}