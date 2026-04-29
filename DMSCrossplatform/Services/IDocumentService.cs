using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public interface IDocumentService
{
    Task<IReadOnlyList<DocumentFullReadDto>> ListAsync(int? statusId = null, int? categoryId = null,
        string? search = null, string? mode = null, CancellationToken ct = default);
    Task<DocumentReadDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<DocumentReadDto> CreateAsync(DocumentCreateDto dto, CancellationToken ct = default);
    Task<DocumentReadDto> SubmitAsync(Guid id, int routeId, CancellationToken ct = default);
    Task<DocumentReadDto> ApproveAsync(Guid id, CancellationToken ct = default);
    Task<DocumentReadDto> RejectAsync(Guid id, string comment, CancellationToken ct = default);
    Task<DocumentVersionReadDto> CreateVersionAsync(Guid documentId, DocumentVersionCreateDto dto, CancellationToken ct = default);
    Task<DocumentVersionReadDto> UploadVersionAsync(Guid documentId, byte[] fileBytes, string fileName, string mimeType, CancellationToken ct = default);
    Task<string> GetDownloadUrlAsync(Guid documentId, int versionId, CancellationToken ct = default);
}