using System.ComponentModel.Design;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Infrastructure.Logging;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Infrastructure.Storage;
using DMSCrossplatform.Services;
using DMSCrossplatform.ViewModels;
using DMSCrossplatform.ViewModels.Custom;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace DMSCrossplatform.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDmsClient(
        this IServiceCollection services, AppSettings settings, ITokenStorage tokenStorage)
    {
        services.AddSingleton(settings);
        services.AddSingleton(tokenStorage);

        // Логирование
        AppLogger.Configure();
        services.AddLogging(b => b.ClearProviders().AddNLog());

        // HTTP-инфраструктура
        services.AddTransient<AuthHeaderHandler>();
        services.AddHttpClient<IApiClient, ApiClient>(c =>
            {
                c.BaseAddress = new System.Uri(settings.ApiBaseUrl);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

        // Сервисы домена
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<INotificationService, NotificationService>();

        services.AddTransient<IDictionariesService, DictionariesService>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<ICompanyService, CompanyService>();
        services.AddTransient<IDocumentService, DocumentService>();
        services.AddTransient<IApprovalRouteService, ApprovalRouteService>();


        // ViewModel-и
        services.AddTransient<UploadDocumentViewModel>();
        services.AddTransient<BaseDocumentsListViewModel>();
        services.AddTransient<DocumentsListViewModel>();
        services.AddTransient<ProfileCreateViewModel>();
        services.AddTransient<CompanyCreateViewModel>();
        services.AddTransient<MenuShellViewModel>();
        services.AddTransient<ShellViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<PageControlViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<ForgotPasswordViewModel>();

        return services;
        
 
    }
}