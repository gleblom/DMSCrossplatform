using System;
using System.Collections.Generic;
using System.Linq;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.Services;

namespace DMSCrossplatform.Infrastructure.Policy;

public sealed class PolicyFactory: IPolicyFactory
{
    private readonly IEnumerable<IPolicy> _policies;
    
    private readonly ISessionService _session;

    private readonly UserFullDto _currentUser;

    public PolicyFactory(
        IEnumerable<IPolicy> policies,
        ISessionService sessionService)
    {
        _policies = policies;
        _session = sessionService;
        _currentUser = _session.CurrentUser;
    }

    public IPolicy CreatePolicy()
    {
        var policy = _policies.FirstOrDefault(p => p.Matches(_currentUser.RoleId));

        return policy ?? throw new InvalidOperationException("No policy found");
    }
    
}