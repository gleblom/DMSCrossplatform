using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using DMSCrossplatform.Pdf.Abstractions;
using DMSCrossplatform.Pdf.Rendering;

namespace DMSCrossplatform.Pdf.Controls;

public class PdfViewerControl : UserControl
{
    protected WriteableBitmap? _bitmap;
    protected int _pageIndex;
    protected int _version;

    private readonly DispatcherTimer _debounce;
    private CancellationTokenSource? _cts;

    private double _renderScale = 1.0;
    public double RenderScale
    {
        get => _renderScale;
        set
        {
            if (Math.Abs(_renderScale - value) > 0.0001)
            {
                _renderScale = value;
                RestartDebounce();
            }
        }
    }

    public IPdfPageRasterizer? Rasterizer { get; set; }

    public PdfViewerControl()
    {
        Background = Brushes.Transparent;

        _debounce = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _debounce.Tick += (_, _) =>
        {
            _debounce.Stop();
            TriggerBackgroundRender();
        };
    }

    public override void Render(DrawingContext ctx)
    {
        ctx.Custom(new PdfPageDrawOperation(
            new Rect(Bounds.Size),
            _bitmap,
            Matrix.Identity,
            _version));
    }

    protected void RestartDebounce()
    {
        _debounce.Stop();
        _debounce.Start();
    }

    public void CancelRender()
    {
        _debounce.Stop();
        _cts?.Cancel();
    }
    protected void TriggerBackgroundRender()
    {
        if (Rasterizer is null || _pageIndex < 0 || Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;
        var targetScale = _renderScale;

        Task.Run(async () =>
        {
            try
            {
                var result = await Rasterizer.RasterizeAsync(_pageIndex, targetScale, ct);
                if (ct.IsCancellationRequested)
                    return;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (ct.IsCancellationRequested)
                        return;

                    var wb = CreateBitmap(result);
                    _bitmap = wb;
                    _version++;
                    InvalidateVisual();
                });
            }
            catch (OperationCanceledException)
            {
            }
        }, ct);
    }

    protected static WriteableBitmap CreateBitmap(PdfRasterResult r)
    {
        var wb = new WriteableBitmap(
            new PixelSize(r.Width, r.Height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);

        using var fb = wb.Lock();

        int srcStride = r.Width * 4;
        int dstStride = fb.RowBytes;

        unsafe
        {
            fixed (byte* src = r.Bgra)
            {
                for (int y = 0; y < r.Height; y++)
                {
                    Buffer.MemoryCopy(
                        src + y * srcStride,
                        (byte*)fb.Address + y * dstStride,
                        dstStride,
                        srcStride);
                }
            }
        }

        return wb;
    }
}