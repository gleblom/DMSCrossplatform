using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure.Navigation;

namespace DMSCrossplatform.ViewModels;

public partial class MenuShellViewModel: ViewModelBase
{
    private readonly INavigationService _navigation;

    public MenuShellViewModel(INavigationService navigation)
    {
        _navigation = navigation;
    }

    [RelayCommand]
    private void NavigatePublicDocs()
    {
        _navigation.NavigateTo<DocumentsListViewModel>();
    }
    [RelayCommand]
    private void NavigateNewDoc()
    {
        _navigation.NavigateTo<UploadDocumentViewModel>();
    }
}