using System.Collections.Generic;
using System.Linq;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Infrastructure.Policy;

public class AdminPolicy : IPolicy
{
    public bool CanSeeUnits => true;
    
    public bool CanSeeRoles => true;
    
    public bool CanSeeEmployees => true;
    
    public bool CanSeeDocs => false;

    public bool CanSeeApprovalRoutes => false;

    public bool Matches(int? roleId) => roleId == 2;

    public List<UserFullDto> Users(List<UserFullDto> users)
        => users.Where(u => u.RoleId != 1 && u.RoleId != 2 && u.RoleId != 3).ToList();
    
    public List<RoleReadDto> Roles(List<RoleReadDto> roles)
        => roles.Where(r => r.Id != 1 && r.Id != 2 && r.Id != 3).ToList();

    public List<UnitReadDto> Units(List<UnitReadDto> units)
        => units;
}