using System;
using DMSCrossplatform.ViewModels;

namespace DMSCrossplatform.Infrastructure.Navigation;

public interface INavigationService<TState>
    where TState : ViewState
{
    void NavigateTo<TViewModel>() where TViewModel : class;
}