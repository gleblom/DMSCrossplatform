using System;
using System.Threading.Tasks;
using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;
using AndroidX.Camera.View;
using AndroidX.Core.Content;
using AndroidX.Lifecycle;
using Avalonia;
using Avalonia.Android;
using Avalonia.Controls;
using Avalonia.Platform;
using DMSCrossplatform.Infrastructure.Android;
using Java.Lang;

namespace DMSCrossplatform.Android;

public class CameraPreviewHost : NativeControlHost, ICameraPreviewHost
{
    private PreviewView? _previewView;
    private ProcessCameraProvider _cameraProvider;
    private ImageCapture? _imageCapture;
    public Action<string>? OnQrCodeDetected { get; set; }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var activity = new AndroidActivityHost().Current 
                       ?? throw new InvalidOperationException("Android activity is not available.");
        

        _previewView = new PreviewView(activity);
        _previewView.SetImplementationMode(PreviewView.ImplementationMode.Compatible);
        
        var executor = ContextCompat.GetMainExecutor(_previewView.Context);
        var cameraProviderFuture = ProcessCameraProvider.GetInstance(activity);
        
        cameraProviderFuture.AddListener(new Runnable(() =>
        {
            _cameraProvider = (ProcessCameraProvider)cameraProviderFuture.Get();
            
            var preview = new Preview.Builder().Build();
            preview.SetSurfaceProvider(executor, _previewView.SurfaceProvider);


            _imageCapture = new ImageCapture.Builder()
                .SetCaptureMode(ImageCapture.CaptureModeMinimizeLatency)
                .Build();
            
            var cameraSelector = CameraSelector.DefaultBackCamera;
            
            var imageAnalysis = new ImageAnalysis.
                Builder()
                .SetBackpressureStrategy(ImageAnalysis.StrategyKeepOnlyLatest)
                .Build();
            Console.WriteLine($"OnQrCodeDetected {OnQrCodeDetected}");
            
            imageAnalysis.SetAnalyzer(executor, new QrCodeAnalyzer(result =>
            {
                Console.WriteLine($"QR Code Analyzer Result: {result}");
                OnQrCodeDetected?.Invoke(result);
                
            }));
            

            if (activity is ILifecycleOwner lifecycleOwner)
            {
                _cameraProvider.UnbindAll();
                
                _cameraProvider.BindToLifecycle(lifecycleOwner, cameraSelector, preview, _imageCapture, imageAnalysis);
            }
        }), executor);
        
        
        return new AndroidViewControlHandle(_previewView);
    }
    public Task<string> CaptureAsync()
    {
        if (_previewView is null || _cameraProvider is null)
            throw new InvalidOperationException("Camera preview is not initialized yet.");

        var cacheDir = _previewView.Context.CacheDir;
        var file = Java.IO.File.CreateTempFile("camera_", ".jpg", cacheDir);

        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var executor = ContextCompat.GetMainExecutor(_previewView.Context);

        var outputOptions = new ImageCapture.OutputFileOptions.Builder(file).Build();
        

        _imageCapture.TakePicture(outputOptions, executor, new SaveCallback(tcs, file.AbsolutePath));

        return tcs.Task;
    }
    

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Dispose();
    }

    public void Dispose()
    {
        try
        {
            _cameraProvider?.UnbindAll();
        }
        catch
        {
            // ignore
        }

        _cameraProvider = null;
        _previewView = null;
    }
    private sealed class SaveCallback : Java.Lang.Object, ImageCapture.IOnImageSavedCallback
    {
        private readonly TaskCompletionSource<string> _tcs;
        private readonly string _path;

        public SaveCallback(TaskCompletionSource<string> tcs, string path)
        {
            _tcs = tcs;
            _path = path;
        }

        public void OnCaptureStarted()
        {
            
        }

        public void OnError(ImageCaptureException? p0)
        {
            _tcs.TrySetException(new InvalidOperationException(p0.Message, p0));
        }

        public void OnImageSaved(ImageCapture.OutputFileResults? p0)
        {
            _tcs.TrySetResult(_path);
        }
    }
}