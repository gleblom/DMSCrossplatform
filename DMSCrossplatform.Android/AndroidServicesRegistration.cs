using DMSCrossplatform.Infrastructure;
using DMSCrossplatform.Infrastructure.Android;
using DMSCrossplatform.Infrastructure.Storage;
using DMSCrossplatform.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DMSCrossplatform.Android;

public static class AndroidServicesRegistration
{
    public static IServiceCollection AddPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<ISessionBlobStore, AndroidKeystoreBlobStore>();
        services.AddSingleton<IWebAuthnClient, AndroidWebAuthnClient>();
        services.AddSingleton<IDownloadSaver, AndroidDownloadSaver>();
        services.AddSingleton<ICameraPreviewHost, CameraPreviewHost>();
        services.AddSingleton<IAndroidActivityHost, AndroidActivityHost>();
        services.AddSingleton<IAndroidPermissionRequester, AndroidPermissionRequester>();
        services.AddSingleton<IAndroidGetFcmToken, MyFirebaseMessagingService>();
        services.AddSingleton<IAndroidPasskeySignalSync, AndroidPasskeySignalSync>();

        return services;
    }
}
