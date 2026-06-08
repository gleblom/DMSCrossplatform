using System;
using System.Threading.Tasks;

namespace DMSCrossplatform.Infrastructure.Android;

public interface ICameraPreviewHost: IDisposable
{
    Task<string> CaptureAsync();
    Action<string>? OnQrCodeDetected { get; set; }
}