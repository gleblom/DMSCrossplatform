using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using DMSCrossplatform.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DMSCrossplatform.Android
{
    [Application]
    public class Application : AvaloniaAndroidApplication<App>
    {
        protected Application(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        {
            var services = new ServiceCollection();
            services.AddPlatformServices();
            var provider = services.BuildServiceProvider();
            AppContainer.InitServices(provider);
            return base.CustomizeAppBuilder(builder)
                .WithInterFont();
        }
    }
}