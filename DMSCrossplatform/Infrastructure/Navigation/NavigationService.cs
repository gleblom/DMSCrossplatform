using System;
using System.Collections.Generic;
using DMSCrossplatform.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DMSCrossplatform.Infrastructure.Navigation;


public sealed class NavigationService<TState> : INavigationService<TState>
    where TState : ViewState
{
    private readonly IServiceProvider _sp;
    private readonly TState _state;

    public NavigationService(IServiceProvider sp, TState state)
    {
        _sp = sp;
        _state = state;
    }

    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        _state.CurrentViewModel = _sp.GetRequiredService<TViewModel>();
    }
}