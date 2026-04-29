using DMSCrossplatform.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace DMSCrossplatform.Android;

public static class AndroidServicesRegistration
{
    public static IServiceCollection AddPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<ISessionBlobStore, AndroidKeystoreBlobStore>();

        return services;
    }
}