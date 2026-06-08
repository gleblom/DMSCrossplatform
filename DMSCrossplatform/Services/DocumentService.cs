using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public class DocumentService : IDocumentService
{
    private readonly IApiClient _api;
        
    public DocumentService(IApiClient api) => _api = api;

    public async Task<IReadOnlyList<DocumentFullReadDto>> ListAsync(
        string? startDate = null, string? endDate = null,
        List<Guid>? authors = null, List<int>? statusId = null, List<int>? categoryId = null,
        string? search = null, string? mode = null, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (statusId?.Count != 0 && statusId != null) query.Add(string.Join("&", statusId.Select(id => $"status_id={id}")));
        if (categoryId?.Count != 0 && categoryId != null) query.Add(string.Join("&", categoryId.Select(id => $"category_id={id}")));
        if(authors?.Count != 0 && authors != null) query.Add(string.Join("&", authors.Select(id => $"authors={id}")));
        if (startDate != null) query.Add($"from_date={startDate}");
        if (endDate != null) query.Add($"to_date={endDate}");
        if (!string.IsNullOrEmpty(search)) query.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrEmpty(mode)) query.Add($"mode={Uri.EscapeDataString(mode)}");
        var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";
        return await _api.GetAsync<List<DocumentFullReadDto>>($"/api/documents/{qs}", ct);
    }

    public Task<DocumentReadDto> GetAsync(Guid id, CancellationToken ct = default)
        => _api.GetAsync<DocumentReadDto>($"/api/documents/{id}", ct);

    public Task<DocumentReadDto> CreateAsync(DocumentCreateDto dto, CancellationToken ct = default)
        => _api.PostJsonAsync<DocumentCreateDto, DocumentReadDto>("/api/documents", dto, ct);

    public Task<DocumentReadDto> SubmitAsync(Guid id, int routeId, CancellationToken ct = default)
        => _api.PostJsonAsync<DocumentSubmitDto, DocumentReadDto>(
            $"/api/documents/{id}/submit", new DocumentSubmitDto { RouteId = routeId }, ct);

    public Task<DocumentReadDto> ApproveAsync(Guid id, CancellationToken ct = default)
        => _api.PostJsonAsync<object, DocumentReadDto>($"/api/documents/{id}/approve", new { }, ct);

    public Task<DocumentReadDto> RejectAsync(Guid id, string comment, CancellationToken ct = default)
        => _api.PostJsonAsync<object, DocumentReadDto>($"/api/documents/{id}/reject", new { comment }, ct);

    public Task<DocumentVersionReadDto> CreateVersionAsync(Guid documentId, DocumentVersionCreateDto dto, CancellationToken ct = default)
        => _api.PostJsonAsync<DocumentVersionCreateDto, DocumentVersionReadDto>(
            $"/api/documents/{documentId}/versions/create", dto, ct);

    public Task<DocumentVersionReadDto> UploadVersionAsync(Guid documentId, byte[] fileBytes, string fileName, string mimeType, CancellationToken ct = default)
    {
        var content = new MultipartFormDataContent();
        var byteContent = new ByteArrayContent(fileBytes);
        byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
        content.Add(byteContent, "file", fileName);
        return _api.PostFormAsync<DocumentVersionReadDto>(
            $"/api/documents/{documentId}/versions/upload", content, ct);
    }

    public async Task<string> GetDownloadUrlAsync(Guid documentId, int versionId, CancellationToken ct = default)
    {
        var raw = await _api.GetAsync<dynamic>(
            $"/api/documents/{documentId}/versions/{versionId}/download-url", ct);
        return raw?.url?.ToString() ?? string.Empty;
    }

    public Task CreateDocumentUnits(Guid documentId, CreateDocumentUnitDto unitIds, CancellationToken ct = default)
        => _api.PostJsonAsync<object, object>($"/api/documents/{documentId}/units", unitIds.UnitsIds, ct);
    
    public Task<IReadOnlyList<MvDocumentVersionReadDto>> GetDocumentVersionsAsync(Guid documentId, CancellationToken ct = default)
        => _api.GetAsync<IReadOnlyList<MvDocumentVersionReadDto>>($"/api/documents/{documentId}/versions", ct);
    public Task<IReadOnlyList<MvDocumentApprovalReadDto>> GetDocumentApprovalsAsync(Guid documentId, CancellationToken ct = default)
        => _api.GetAsync<IReadOnlyList<MvDocumentApprovalReadDto>>($"/api/documents/{documentId}/approvals", ct);

    public Task<DocumentApprovalReadDto> GetApprovalByStep(Guid documentId, int stepIndex, int versionId,
        CancellationToken ct = default)
        => _api.GetAsync<DocumentApprovalReadDto>(
            $"/api/documents/{documentId}/versions/{versionId}/approval/{stepIndex}", ct);

    public Task<ShareLinkDto> CreateShareLink(Guid documentId, CancellationToken ct = default)
        => _api.PostJsonAsync<object, ShareLinkDto>($"/api/documents/{documentId}/share-link", new {}, ct);

    public Task<DocumentReadDto> ConfirmShareLink(string shareLink, CancellationToken ct = default)
        => _api.PostJsonAsync<object, DocumentReadDto>($"{shareLink}", new {}, ct);
}