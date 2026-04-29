using System;
using System.Collections.Generic;
using DMSCrossplatform.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DMSCrossplatform.Infrastructure.Navigation;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _provider;
    private readonly Stack<ViewModelBase> _back = new();

    public ViewModelBase? Current { get; private set; }
    public bool CanGoBack => _back.Count > 0;

    public event EventHandler? CurrentChanged;

    
    public NavigationService(IServiceProvider provider) => _provider = provider;

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        TViewModel vm;
        try
        {
            vm = _provider.GetRequiredService<TViewModel>();
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException(
                $"Failed to resolve view model '{typeof(TViewModel).Name}'. " +
                "Make sure it and all its dependencies are registered in the DI container.", ex);
        }
        NavigateTo(vm);
    }

    public void NavigateTo(ViewModelBase vm)
    {
        if (Current is not null) _back.Push(Current);
        Current = vm;
        CurrentChanged?.Invoke(this, EventArgs.Empty);
    }

    public void GoBack()
    {
        if (!CanGoBack) return;
        Current = _back.Pop();
        CurrentChanged?.Invoke(this, EventArgs.Empty);
    }
}