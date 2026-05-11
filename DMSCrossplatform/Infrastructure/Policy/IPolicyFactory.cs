using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Infrastructure.Policy;

public interface IPolicyFactory
{
    IPolicy CreatePolicy();
}