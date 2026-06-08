using CommunityToolkit.Mvvm.ComponentModel;
using DMSCrossplatform.Infrastructure.Navigation;

namespace DMSCrossplatform.ViewModels;

public partial class UserViewModel: ViewModelBase
{
    [ObservableProperty] private string _infoText = "Отсканируйте QR-код с документом";
    private readonly INavigationService<MenuRegionState> _navigationService;
    
    public UserViewModel(INavigationService<MenuRegionState> navigationService)
    {
        _navigationService = navigationService;
        NavigateToMyDocumentsListViewModel();
    }

    private void NavigateToMyDocumentsListViewModel()
    {
        _navigationService.NavigateTo<MyDocumentsListViewModel>();
    }
}