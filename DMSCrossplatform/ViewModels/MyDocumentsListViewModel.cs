using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Services;
using Microsoft.Extensions.Logging;

namespace DMSCrossplatform.ViewModels;

public class MyDocumentsListViewModel: BaseDocumentsListViewModel
{
    public MyDocumentsListViewModel(
        INavigationService<MenuRegionState> navigation,
        ILogger<ApiClient> log, 
        IDictionariesService dictionaryService, 
        IDocumentService documentService, 
        IUserService userService, string? mode = "my") : base(navigation, log, dictionaryService, documentService, userService, mode)
    {
    }
    

    


}