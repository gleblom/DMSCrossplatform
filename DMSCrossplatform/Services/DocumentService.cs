using System;
using System.Collections.Generic;
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

    public async Task<IReadOnlyList<DocumentFullReadDto>> ListAsync(int? statusId = null, int? categoryId = null,
        string? search = null, string? mode = null, CancellationToken ct = default)
    {
        var query = new System.Collections.Generic.List<string>();
        if (statusId.HasValue) query.Add($"status_id={statusId}");
        if (categoryId.HasValue) query.Add($"category_id={categoryId}");
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
}