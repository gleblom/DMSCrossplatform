using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public interface IDocumentService
{
    Task<IReadOnlyList<DocumentFullReadDto>> ListAsync(
        string? startDate = null, string? endDate = null,
        List<Guid>? authors = null, List<int>? statusId = null, List<int>? categoryId = null,
        string? search = null, string? mode = null, CancellationToken ct = default);
    Task<DocumentReadDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<DocumentReadDto> CreateAsync(DocumentCreateDto dto, CancellationToken ct = default);
    Task<DocumentReadDto> SubmitAsync(Guid id, int routeId, CancellationToken ct = default);
    Task<DocumentReadDto> ApproveAsync(Guid id, CancellationToken ct = default);
    Task<DocumentReadDto> RejectAsync(Guid id, string comment, CancellationToken ct = default);
    Task<DocumentVersionReadDto> CreateVersionAsync(Guid documentId, DocumentVersionCreateDto dto, CancellationToken ct = default);
    Task<DocumentVersionReadDto> UploadVersionAsync(Guid documentId, byte[] fileBytes, string fileName, string mimeType, CancellationToken ct = default);
    Task<string> GetDownloadUrlAsync(Guid documentId, int versionId, CancellationToken ct = default);
    
    Task CreateDocumentUnits(Guid documentId, CreateDocumentUnitDto  unitIds, CancellationToken ct = default);
    
    Task<IReadOnlyList<MvDocumentVersionReadDto>> GetDocumentVersionsAsync(Guid documentId, CancellationToken ct = default);
    
    Task<IReadOnlyList<MvDocumentApprovalReadDto>> GetDocumentApprovalsAsync(Guid documentId, CancellationToken ct = default);
    
    Task<DocumentApprovalReadDto> GetApprovalByStep(Guid documentId, int stepIndex, int versionId, CancellationToken ct = default);
}