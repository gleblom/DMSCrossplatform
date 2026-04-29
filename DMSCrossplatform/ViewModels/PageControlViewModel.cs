using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure.Navigation;

namespace DMSCrossplatform.ViewModels;

public partial class PageControlViewModel:ViewModelBase
{
    private readonly INavigationService _navigation;
    [ObservableProperty] private ViewModelBase? _currentView;

    public PageControlViewModel(INavigationService navigation)
    {
        _navigation = navigation;
        _navigation.CurrentChanged += (_, _) => CurrentView = _navigation.Current;
        
        _navigation.NavigateTo<DocumentsListViewModel>();
    }

}