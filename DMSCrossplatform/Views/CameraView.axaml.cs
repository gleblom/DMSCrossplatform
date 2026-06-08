using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using DMSCrossplatform.Infrastructure;
using DMSCrossplatform.Infrastructure.Android;
using DMSCrossplatform.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DMSCrossplatform.Views;

public partial class CameraView : UserControl
{
    private readonly IAndroidActivityHost _activityHost;
    private ICameraPreviewHost? _cameraHost;
    private readonly IAndroidPermissionRequester _requester;
    
    public CameraView()
    {
        InitializeComponent();
        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
        _cameraHost = App.Services.GetRequiredService<ICameraPreviewHost>();
        _activityHost = App.Services.GetRequiredService<IAndroidActivityHost>();
        _requester = App.Services.GetRequiredService<IAndroidPermissionRequester>();
        
    }
    private async void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        StatusText.Text = "Запрашиваю доступ к камере...";
    
        if (OperatingSystem.IsAndroid())
        {
            var activity = _activityHost.Current
                           ?? throw new InvalidOperationException("Android activity is not available.");
    
            var granted = await _requester.RequestCameraAsync(activity);
            if (!granted)
            {
                StatusText.Text = "Доступ к камере не предоставлен.";
                return;
            }
    
            _cameraHost.OnQrCodeDetected = HandleQrCode;
            CameraHostContainer.Content = _cameraHost;
            StatusText.Text = "Камера готова.";
    
           
        }
        else
        {
            StatusText.Text = "Этот пример работает на Android.";            
        }
    
    }
    private async void HandleQrCode(string qrResult)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (DataContext is CameraViewModel viewModel)
            {
                _ = viewModel.HandleQrCodeAsync(qrResult);
            }
        });
    }
    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (_cameraHost != null)
        {
            _cameraHost.OnQrCodeDetected = null; 
            _cameraHost.Dispose();
            _cameraHost = null;
        }
        CameraHostContainer.Content = null;
    }
}
