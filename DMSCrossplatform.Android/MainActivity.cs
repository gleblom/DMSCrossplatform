using System;
using System.Text.Json.Nodes;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;
using Avalonia.Threading;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Services;
using DMSCrossplatform.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DMSCrossplatform.Android;

[Activity(
    Label = "DMSCrossplatform.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
    private ShellHost _host;
    
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        AndroidActivityHost.SetCurrent(this);
        ProcessNotificationIntent(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        ProcessNotificationIntent(intent);
    }

    private void ProcessNotificationIntent(Intent intent)
    {
        _host = App.Services.GetRequiredService<ShellHost>();
        if (intent != null && intent.HasExtra("dataJson"))
        {
            var dataJson = intent.GetStringExtra("dataJson");
            var node = JsonNode.Parse(dataJson);
            
            Console.WriteLine($"Intent Json Data: {dataJson}");
            
            var documentId = Guid.Parse(node["documentId"].ToString());
            
            Console.WriteLine($"Notification document id '{documentId}'");
            
            App.SelectedDocumentId = documentId;
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
    

    protected override void OnResume()
    {
        base.OnResume();
        AndroidActivityHost.SetCurrent(this);
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        AndroidPermissionRequester.HandleRequestPermissionsResult(requestCode, grantResults);
    }
}