using System;
using System.Diagnostics;
using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using DMSCrossplatform.Infrastructure;
using Firebase;
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
            var services = App.ServiceCollection;
            services.AddPlatformServices();
            // services.AddDmsClient(App.Settings);
            // var provider = services.BuildServiceProvider();
            // AppContainer.InitServices(provider);

            return base.CustomizeAppBuilder(builder)
                .WithInterFont();
        }
    }
}