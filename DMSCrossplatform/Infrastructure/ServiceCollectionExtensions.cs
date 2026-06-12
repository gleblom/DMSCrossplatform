using System.ComponentModel.Design;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Infrastructure.Logging;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Infrastructure.Policy;
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
        this IServiceCollection services, AppSettings settings)
    {
        services.AddSingleton(settings);
        services.AddSingleton<ITokenStorage, JsonTokenStorage>();
        services.AddSingleton<IDeviceIdentityStore>(_ =>
            new FileDeviceIdentityStore(System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                "DMSCrossplatform",
                "device.id")));

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
        services.AddSingleton<ShellHost>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<INotificationService, NotificationService>();
        
        //Ролевая политика
        services.AddSingleton<IPolicy, ClerkPolicy>();
        services.AddSingleton<IPolicy, AdminPolicy>();
        services.AddSingleton<IPolicy, UserPolicy>();
        services.AddSingleton<IPolicy, DirectorPolicy>();
        services.AddScoped<IPolicyFactory, PolicyFactory>();
        
        //Навигация
        services.AddScoped<StartupRegionState>();
        services.AddScoped<MenuRegionState>();

        services.AddScoped<StartupShellViewModel>();
        services.AddScoped<MenuShellViewModel>();

        services.AddScoped<INavigationService<StartupRegionState>, NavigationService<StartupRegionState>>();
        services.AddScoped<INavigationService<MenuRegionState>, NavigationService<MenuRegionState>>();
        
        //Сервисы
        services.AddTransient<IDictionariesService, DictionariesService>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<ICompanyService, CompanyService>();
        services.AddTransient<IPushService, PushService>();
        services.AddTransient<IDocumentService, DocumentService>();
        services.AddTransient<IApprovalRouteService, ApprovalRouteService>();


        // ViewModel-и
        
        services.AddTransient<NotificationViewModel>();
        services.AddTransient<CameraViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<ValidateOtpViewModel>();
        services.AddTransient<UserProfileEditViewModel>();
        services.AddTransient<RoleEditViewModel>();
        services.AddTransient<UnitEditViewModel>();
        services.AddTransient<UserEditViewModel>();
        services.AddTransient<PdfViewModel>();
        services.AddTransient<UserListViewModel>();
        services.AddTransient<UploadDocumentViewModel>();
        services.AddTransient<BaseDocumentsListViewModel>();
        services.AddTransient<DocumentsListViewModel>();
        services.AddTransient<MyDocumentsListViewModel>();
        services.AddTransient<ProfileCreateViewModel>();
        services.AddTransient<CompanyCreateViewModel>();
        services.AddTransient<MenuShellViewModel>();
        services.AddTransient<StartupShellViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<ForgotPasswordViewModel>();

        // Маршруты согласования
        services.AddTransient<ApprovalRoutesListViewModel>();
        services.AddTransient<ApprovalRouteEditorViewModel>();

        // Просмотр документа
        services.AddTransient<DocumentViewModel>();

        return services;
        
 
    }
}
