using System;
using Microsoft.Extensions.DependencyInjection;

namespace DMSCrossplatform.ViewModels;

public sealed class ShellHost : ViewModelBase, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private IServiceScope? _currentScope;
    private object? _currentShell;

    public ShellHost(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public object? CurrentShell
    {
        get => _currentShell;
        private set
        {
            _currentShell = value;
            OnPropertyChanged();
        }
    }

    public void ShowStartup()
        => SwitchShell<StartupShellViewModel>();

    public void ShowMenu()
        => SwitchShell<MenuShellViewModel>();

    private void SwitchShell<TShellViewModel>()
        where TShellViewModel : class
    {
        _currentScope?.Dispose();

        _currentScope = _scopeFactory.CreateScope();
        CurrentShell = _currentScope.ServiceProvider.GetRequiredService<TShellViewModel>();
    }

    public void Dispose()
    {
        _currentScope?.Dispose();
    }
}