using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public interface ICompanyService
{
    Task<CompanyReadDto> CreateAsync(CompanyCreateDto dto, CancellationToken ct = default);
    Task<CompanyReadDto> UpdateAsync(CompanyUpdateDto dto, CancellationToken ct = default);
}