using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Services;
using Microsoft.Extensions.Logging;

namespace DMSCrossplatform.ViewModels;

public class DocumentsListViewModel: BaseDocumentsListViewModel
{
    public DocumentsListViewModel(ILogger<ApiClient> log, IDictionariesService dictionaryService, IDocumentService documentService, IUserService userService) : base(log, dictionaryService, documentService, userService)
    {
    }
}