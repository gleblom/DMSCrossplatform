using System;
using DMSCrossplatform.ViewModels;

namespace DMSCrossplatform.Infrastructure.Navigation;

public interface INavigationService
{
    ViewModelBase? Current { get; }
    event EventHandler? CurrentChanged;
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    void NavigateTo(ViewModelBase viewModel);
    bool CanGoBack { get; }
    void GoBack();
}