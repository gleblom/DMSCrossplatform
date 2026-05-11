using System.Collections.Generic;
using System.Linq;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Infrastructure.Policy;

public interface IPolicy
{
    bool CanSeeUnits { get; }
    
    bool CanSeeRoles { get; }
    
    bool CanSeeEmployees { get; }
    
    bool CanSeeDocs { get; }
    
    bool CanSeeApprovalRoutes { get; }
    
    bool Matches(int? roleId);

    public List<UserFullDto> Users(List<UserFullDto> users);
    
    public List<RoleReadDto> Roles(List<RoleReadDto> roles);
    
    public List<UnitReadDto> Units(List<UnitReadDto> units);
}