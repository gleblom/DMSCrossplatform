using System;
using Microsoft.Extensions.DependencyInjection;

namespace DMSCrossplatform.Infrastructure;

public class AppContainer
{
    public static IServiceProvider Services { get; private set; }

    public static void InitServices(IServiceProvider provider)
    {
        Services = provider;
    }
    
    public static T GetRequiredService<T>() => Services.GetRequiredService<T>();
}