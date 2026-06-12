using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using DMSCrossplatform.Infrastructure;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Services;
using DMSCrossplatform.ViewModels;
using Microsoft.Windows.ApplicationModel.DynamicDependency;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.PushNotifications;

namespace DMSCrossplatform.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    
    private static ShellHost _host;
    private static INotificationService _notificationService;
    private static ILogger<WindowsPushNotifications> _logger;
    private static IDocumentService _documentService;
    private static IPushService _pushService;
    private static ISessionService _sessionService;
    
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {

        
        var services = App.ServiceCollection;
        services.AddPlatformServices();
        // Bootstrap.Initialize(0x00020013);
        
        AppNotificationManager.Default.NotificationInvoked += OnNotificationClicked;
        PushNotificationManager.Default.PushReceived += OnPushReceived;
        AppNotificationManager.Default.Register();
        PushNotificationManager.Default.Register(); ;
        
        
        // var provider = services.BuildServiceProvider();
        // AppContainer.InitServices(App.Services);
        
        var appBuilder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
        appBuilder.AfterSetup(_ =>
        {
            var lifetime = appBuilder.Instance?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            if (lifetime != null)
            {
                lifetime.ShutdownRequested += (_, _) =>
                {
                    AppNotificationManager.Default.Unregister();
                };
            }
        });

        return appBuilder;
    }
    private static async void OnPushReceived(PushNotificationManager sender, PushNotificationReceivedEventArgs args)
    {
        try
        {
            _logger = App.Services.GetRequiredService<ILogger<WindowsPushNotifications>>();
            _notificationService = App.Services.GetRequiredService<INotificationService>();
            _pushService = App.Services.GetRequiredService<IPushService>();
            _sessionService = App.Services.GetRequiredService<ISessionService>();
            var doc = new XmlDocument();
            
            
            var payloadBytes = args.Payload;
            var xmlPayload = Encoding.UTF8.GetString(payloadBytes);
            
            doc.LoadXml(xmlPayload);
            
            var toastNode = doc.SelectSingleNode("toast");
            
            if (toastNode != null && toastNode.Attributes.GetNamedItem("launch") != null)
            {
                var launchPayload = toastNode.Attributes.GetNamedItem("launch").Value;
                var decoded = HttpUtility.HtmlDecode(launchPayload); 
                var query = HttpUtility.ParseQueryString(decoded);
                var notifId = query["notification_id"];
                if (notifId != null)
                {
                    
                    var notification = await _pushService.GetNotification(int.Parse(notifId));

                    if (_sessionService.CurrentUser == null ||
                        notification.UserId != _sessionService.CurrentUser?.UserId) return;
                    var appNotification = new AppNotification(xmlPayload);
                    AppNotificationManager.Default.Show(appNotification);
                    await _notificationService.AddNotification(int.Parse(notifId));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
    
    
    private static async void OnNotificationClicked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        try
        {
            _host = App.Services.GetRequiredService<ShellHost>();
            var launchPayload = args.Argument;
            var decoded = HttpUtility.HtmlDecode(launchPayload);
            var query = HttpUtility.ParseQueryString(decoded);
            var documentId = Guid.Parse(query["document_id"] ?? string.Empty);
            _documentService = App.Services.GetRequiredService<IDocumentService>();
            var docTask = _documentService.GetAsync(documentId);
            await Task.WhenAll(docTask);
            
            var doc = docTask.Result;
            if (doc.Id == documentId && !string.IsNullOrEmpty(doc.Title)) 
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _host.ExecuteInCurrentScope(sp =>
                    {
                        var nav = sp.GetRequiredService<INavigationService<MenuRegionState>>();
                        nav.NavigateTo<DocumentViewModel>();
                    });
                });
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}