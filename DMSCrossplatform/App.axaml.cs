using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

using Avalonia.Markup.Xaml;
using DMSCrossplatform.Infrastructure;
using DMSCrossplatform.Infrastructure.Storage;
using DMSCrossplatform.Services;
using DMSCrossplatform.ViewModels;
using DMSCrossplatform.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DMSCrossplatform;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = default!;



    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {

        AppSettings settings = new();
        var tokenStorage = AppContainer.GetRequiredService<ISessionBlobStore>();
        
        var jsonTokenStorage = new JsonTokenStorage(tokenStorage);

        var sc = new ServiceCollection();
        sc.AddDmsClient(settings, jsonTokenStorage);
        Services = sc.BuildServiceProvider();

        
        


        
        var shell = Services.GetRequiredService<ShellViewModel>();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow()
            {
                DataContext = shell
            };
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
        }

        base.OnFrameworkInitializationCompleted();
    }
}
