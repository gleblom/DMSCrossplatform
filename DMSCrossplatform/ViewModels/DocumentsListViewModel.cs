using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Services;
using Microsoft.Extensions.Logging;

namespace DMSCrossplatform.ViewModels;

public class DocumentsListViewModel: BaseDocumentsListViewModel
{
    public DocumentsListViewModel(
        INavigationService<MenuRegionState> navigation,
        ILogger<ApiClient> log, 
        IDictionariesService dictionaryService, 
        IDocumentService documentService, 
        IUserService userService, string? mode = "all") : 
        base(navigation, log, dictionaryService, documentService, userService, mode)
    {

    }

    public static void SwitchMode()
    {
        Mode = Mode == "all" ? "incoming" : "all";
    }
    
}