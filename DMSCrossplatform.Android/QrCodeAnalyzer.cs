using System.Threading.Tasks;
using AndroidX.Camera.Core;
using System;
using Android.Gms.Tasks;
using Android.Util;
using Java.Interop;
using Java.Util;
using Xamarin.Google.MLKit.Vision.BarCode;
using Xamarin.Google.MLKit.Vision.Barcode.Common;
using Xamarin.Google.MLKit.Vision.Common;
using Object = Java.Lang.Object;
using Task = Android.Gms.Tasks.Task;

namespace DMSCrossplatform.Android;

public class QrCodeAnalyzer: Object, ImageAnalysis.IAnalyzer
{
    private IBarcodeScanner _scanner;
    private readonly Action<string> _onQrDetected;

    public QrCodeAnalyzer(Action<string> callback)
    {
        _onQrDetected = callback;
        var options = new BarcodeScannerOptions.Builder()
            .SetBarcodeFormats(Barcode.FormatQrCode)
            .Build();
        _scanner = BarcodeScanning.GetClient(options);
    }

    public Size? DefaultTargetResolution => null;

    public void Analyze(IImageProxy? p0)
    {
        var mediaImage = p0.Image;
        if (mediaImage != null)
        {
            var image = InputImage.FromMediaImage(mediaImage, p0.ImageInfo.RotationDegrees);
            _scanner
                .Process(image)
                .AddOnSuccessListener(new OnSuccessListener(_onQrDetected))
                .AddOnCompleteListener(new OnCompleteListener(p0));
                
        }
    }
    private sealed class OnSuccessListener(Action<string> onQrDetected) : Object, IOnSuccessListener
    {
        public void OnSuccess(Object? result)
        {
            if (result is Object jObj)
            {   
                if (jObj.JavaCast<ArrayList>() is ArrayList barcodeList)
                {
                    for (int i = 0; i < barcodeList.Size(); i++)
                    {
                        if (barcodeList.Get(i) is Barcode barcode && !string.IsNullOrWhiteSpace(barcode.RawValue))
                        {
                            onQrDetected?.Invoke(barcode.RawValue);
                            Console.WriteLine("barcode.RawValue " + barcode.RawValue);
                            break; 
                        }
                    }
                }
            }
        }
    }

    private sealed class OnCompleteListener(IImageProxy proxy) : Object, IOnCompleteListener
    {
        public void OnComplete(Task task)
        {
            proxy.Close(); 
        }
    }
}