using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public class CompanyService : ICompanyService
{
    private readonly IApiClient _api;
    public CompanyService(IApiClient api) => _api = api;

    public Task<CompanyReadDto> CreateAsync(CompanyCreateDto dto, CancellationToken ct = default)
        => _api.PostJsonAsync<CompanyCreateDto, CompanyReadDto>("/api/company", dto, ct);

    public Task<CompanyReadDto> UpdateAsync(CompanyUpdateDto dto, CancellationToken ct = default)
        => _api.PutJsonAsync<CompanyUpdateDto, CompanyReadDto>("/api/company", dto, ct);
}