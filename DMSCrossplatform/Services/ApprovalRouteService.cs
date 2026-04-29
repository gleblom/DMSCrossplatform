using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public class ApprovalRouteService: IApprovalRouteService
{
        private readonly IApiClient _api;
    public ApprovalRouteService(IApiClient api) => _api = api;

    public async Task<IReadOnlyList<ApprovalRouteReadDto>> ListAsync(CancellationToken ct = default)
        => await _api.GetAsync<List<ApprovalRouteReadDto>>("/api/approval-routes", ct);

    public Task<ApprovalRouteReadDto> GetAsync(int id, CancellationToken ct = default)
        => _api.GetAsync<ApprovalRouteReadDto>($"/api/approval-routes/{id}", ct);

    public Task<ApprovalRouteReadDto> CreateAsync(ApprovalRouteCreateDto dto, CancellationToken ct = default)
        => _api.PostJsonAsync<ApprovalRouteCreateDto, ApprovalRouteReadDto>("/api/approval-routes", dto, ct);

    public Task<ApprovalRouteReadDto> UpdateAsync(int id, ApprovalRouteUpdateDto dto, CancellationToken ct = default)
        => _api.PutJsonAsync<ApprovalRouteUpdateDto, ApprovalRouteReadDto>($"/api/approval-routes/{id}", dto, ct);

    public Task DeleteAsync(int id, CancellationToken ct = default)
        => _api.DeleteAsync($"/api/approval-routes/{id}", ct);

    public Task<RouteNodeReadDto> CreateNodeAsync(int routeId, RouteNodeCreateDto dto, CancellationToken ct = default)
        => _api.PostJsonAsync<RouteNodeCreateDto, RouteNodeReadDto>($"/api/approval-routes/{routeId}/nodes", dto, ct);

    public Task<RouteNodeReadDto> UpdateNodeAsync(int routeId, int nodeId, RouteNodeUpdateDto dto, CancellationToken ct = default)
        => _api.PutJsonAsync<RouteNodeUpdateDto, RouteNodeReadDto>(
            $"/api/approval-routes/{routeId}/nodes/{nodeId}", dto, ct);

    public Task DeleteNodeAsync(int routeId, int nodeId, CancellationToken ct = default)
        => _api.DeleteAsync($"/api/approval-routes/{routeId}/nodes/{nodeId}", ct);

    public Task<RouteEdgeReadDto> CreateEdgeAsync(int routeId, RouteEdgeCreateDto dto, CancellationToken ct = default)
        => _api.PostJsonAsync<RouteEdgeCreateDto, RouteEdgeReadDto>($"/api/approval-routes/{routeId}/edges", dto, ct);

    public Task DeleteEdgeAsync(int routeId, int edgeId, CancellationToken ct = default)
        => _api.DeleteAsync($"/api/approval-routes/{routeId}/edges/{edgeId}", ct);

    public Task<RouteGraphDto> GetGraphAsync(int routeId, CancellationToken ct = default)
        => _api.GetAsync<RouteGraphDto>($"/api/approval-routes/{routeId}/graph", ct);
}