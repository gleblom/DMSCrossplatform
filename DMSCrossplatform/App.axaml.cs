using System;
using Android.Renderscripts;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using DMSCrossplatform.Infrastructure;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.ViewModels;
using DMSCrossplatform.Views;
using Microsoft.Extensions.DependencyInjection;
using Plugin.LocalNotification;

namespace DMSCrossplatform;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = default!;
    
    public static IServiceCollection ServiceCollection = new ServiceCollection();
    public static ApprovalRouteReadDto? SelectedApprovalRoute { get; set; }
    public static Guid? SelectedDocumentId { get; set; }

    public static readonly AppSettings Settings = new();
    
    public static IStorageProvider? storageProvider;
    
    private IServiceScope? _appScope;

    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        
        ServiceCollection.AddDmsClient(Settings);
        Services = ServiceCollection.BuildServiceProvider(validateScopes: true);
        
        _appScope = Services.CreateScope();
        
        var shell = _appScope.ServiceProvider.GetRequiredService<ShellHost>();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow()
            {
                DataContext = shell
            };
            storageProvider = desktop.MainWindow.StorageProvider;
            
            
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
        {
            var mainView = new MainView()
            {
                DataContext = shell
            };
            singleViewFactoryApplicationLifetime.MainViewFactory = () => mainView;

            mainView.Loaded += (_, _) =>
            {
                var topLevel = TopLevel.GetTopLevel(mainView);
                storageProvider = topLevel?.StorageProvider;
            };
            
        }
        
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView()
            {
                DataContext = shell
            };
            storageProvider = TopLevel.GetTopLevel(singleViewPlatform.MainView)?.StorageProvider;
        }

        base.OnFrameworkInitializationCompleted();

        Dispatcher.UIThread.Post(shell.ShowStartup, DispatcherPriority.Background);
    }
    
}
