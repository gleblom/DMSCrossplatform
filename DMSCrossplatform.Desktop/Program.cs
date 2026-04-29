using System;
using Avalonia;
using DMSCrossplatform.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DMSCrossplatform.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        var services = new ServiceCollection();
        services.AddPlatformServices();
        var provider = services.BuildServiceProvider();
        AppContainer.InitServices(provider);
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();   
    }
}