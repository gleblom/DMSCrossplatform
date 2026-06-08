using System.Collections.Generic;
using System.Linq;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Infrastructure.Policy;

public class UserPolicy: IPolicy
{
    public bool CanSeeUnits => false;
    public bool CanSeeRoles => false;
    public bool CanSeeEmployees => false;
    public bool CanSeeDocs => true;
    public bool CanSeeApprovalRoutes => false;
    public bool Matches(int? roleId) => roleId != 1 && roleId != 2 && roleId != 3 ;

    public List<UserFullDto> Users(List<UserFullDto> users)
        => users.ToList();
    
    
    public List<RoleReadDto> Roles(List<RoleReadDto> roles)
        => roles.ToList();
    
    public List<UnitReadDto> Units(List<UnitReadDto> units)
        => units.ToList();
}