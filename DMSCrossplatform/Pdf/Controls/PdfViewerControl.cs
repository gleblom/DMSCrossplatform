using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using DMSCrossplatform.Pdf.Abstractions;
using DMSCrossplatform.Pdf.Rendering;

namespace DMSCrossplatform.Pdf.Controls;

public class PdfViewerControl : UserControl
{
    protected WriteableBitmap? _bitmap;
    protected Matrix _matrix = Matrix.Identity;
    protected double _rasterScale = 1.0;

    protected int _pageIndex;
    protected int _version;

    private Point? _last;
    private bool _panning;

    private CancellationTokenSource? _cts;
    private DispatcherTimer _debounce;

    public IPdfPageRasterizer? Rasterizer { get; set; }

    public PdfViewerControl()
    {
        
        _debounce = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };
        _debounce.Tick += (_, _) =>
        {
            _debounce.Stop();
            RenderAsync();
        };
    }

    public override void Render(DrawingContext ctx)
    {
        ctx.Custom(new PdfPageDrawOperation(
            new Rect(Bounds.Size),
            _bitmap,
            _matrix,
            _version));
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {

        base.OnPointerWheelChanged(e);
        e.Handled = true;
        var zoom = e.Delta.Y > 0 ? 1.1 : 0.9;
        var pos = e.GetPosition(this);

        var m =
            Matrix.CreateTranslation(-pos.X, -pos.Y) *
            Matrix.CreateScale(zoom, zoom) *
            Matrix.CreateTranslation(pos.X, pos.Y);

        _matrix = m * _matrix;

        InvalidateVisual();
        RestartDebounce();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        e.Handled = true;

        _panning = true;
        _last = e.GetPosition(this);
        e.Pointer.Capture(this);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        e.Handled = true;
        if (!_panning || _last == null) return;

        var cur = e.GetPosition(this);
        var d = cur - _last.Value;

        _matrix = Matrix.CreateTranslation(d.X, d.Y) * _matrix;
        _last = cur;

        InvalidateVisual();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        e.Handled = true;

        _panning = false;
        _last = null;
        e.Pointer.Capture(null);

        RestartDebounce();
    }

    protected void RestartDebounce()
    {
        _debounce.Stop();
        _debounce.Start();
    }

    private void RenderAsync()
    {
        if (Rasterizer == null) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        var scale = Math.Sqrt(_matrix.M11 * _matrix.M11);

        Task.Run(async () =>
        {
            var r = await Rasterizer.RasterizeAsync(_pageIndex, scale, ct);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var wb = CreateBitmap(r);

                _bitmap = wb;
                _rasterScale = scale;
                _version++;

                InvalidateVisual();
            });
        });
    }

    private static WriteableBitmap CreateBitmap(PdfRasterResult r)
    {
        var wb = new WriteableBitmap(
            new PixelSize(r.Width, r.Height),
            new Vector(96, 96),
            Avalonia.Platform.PixelFormat.Bgra8888,
            Avalonia.Platform.AlphaFormat.Premul);

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