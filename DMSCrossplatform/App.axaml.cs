using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using DMSCrossplatform.Infrastructure;
using DMSCrossplatform.Infrastructure.Storage;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.Services;
using DMSCrossplatform.ViewModels;
using DMSCrossplatform.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DMSCrossplatform;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = default!;
    public static DocumentFullReadDto? SelectedDocument { get; set; }
    
    public static IStorageProvider? storageProvider;

    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {

        AppSettings settings = new();
        var tokenStorage = AppContainer.GetRequiredService<ISessionBlobStore>();
        
        var jsonTokenStorage = new JsonTokenStorage(tokenStorage);

        var client = AppContainer.GetRequiredService<IWebAuthnClient>();

        var sc = new ServiceCollection();
        sc.AddDmsClient(settings, jsonTokenStorage, client);
        Services = sc.BuildServiceProvider();
        
        var shell = Services.GetRequiredService<ShellHost>();
        shell.ShowStartup();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow()
            {
                DataContext = shell
            };
            storageProvider =  desktop.MainWindow.StorageProvider;

        }
        else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
        {
            singleViewFactoryApplicationLifetime.MainViewFactory =
                () => new MainView()
                {
                    DataContext = shell
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
    }
}
