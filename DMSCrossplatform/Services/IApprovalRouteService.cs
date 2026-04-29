using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public interface IApprovalRouteService
{
    Task<IReadOnlyList<ApprovalRouteReadDto>> ListAsync(CancellationToken ct = default);
    Task<ApprovalRouteReadDto> GetAsync(int id, CancellationToken ct = default);
    Task<ApprovalRouteReadDto> CreateAsync(ApprovalRouteCreateDto dto, CancellationToken ct = default);
    Task<ApprovalRouteReadDto> UpdateAsync(int id, ApprovalRouteUpdateDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    Task<RouteNodeReadDto> CreateNodeAsync(int routeId, RouteNodeCreateDto dto, CancellationToken ct = default);
    Task<RouteNodeReadDto> UpdateNodeAsync(int routeId, int nodeId, RouteNodeUpdateDto dto, CancellationToken ct = default);
    Task DeleteNodeAsync(int routeId, int nodeId, CancellationToken ct = default);

    Task<RouteEdgeReadDto> CreateEdgeAsync(int routeId, RouteEdgeCreateDto dto, CancellationToken ct = default);
    Task DeleteEdgeAsync(int routeId, int edgeId, CancellationToken ct = default);

    Task<RouteGraphDto> GetGraphAsync(int routeId, CancellationToken ct = default);
}