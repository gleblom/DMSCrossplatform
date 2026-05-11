using DMSCrossplatform.Infrastructure;
using DMSCrossplatform.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace DMSCrossplatform.Desktop;

public static class DesktopServiceRegistration
{
    public static IServiceCollection AddPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<ISessionBlobStore, WindowsDpapiBlobStore>();
        services.AddSingleton<IWebAuthnClient, WindowsWebAuthnClient>();

        return services;
    }
}