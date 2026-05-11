using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Rendering.SceneGraph;

namespace AvaloniaApplication1.Pdf.Rendering;



public sealed class PdfPageDrawOperation : ICustomDrawOperation
{
    private readonly WriteableBitmap? _bmp;
    private readonly Matrix _matrix;
    private readonly Rect _bounds;
    private readonly int _version;

    public PdfPageDrawOperation(Rect bounds, WriteableBitmap? bmp, Matrix matrix, int version)
    {
        _bounds = bounds;
        _bmp = bmp;
        _matrix = matrix;
        _version = version;
    }

    public Rect Bounds => _bounds;

    public void Dispose() { }

    public bool HitTest(Point p) => false;

    public bool Equals(ICustomDrawOperation? other) =>
        other is PdfPageDrawOperation o &&
        ReferenceEquals(_bmp, o._bmp) &&
        _matrix == o._matrix &&
        _version == o._version;

    public void Render(ImmediateDrawingContext context)
    {
        if (_bmp is null) return;

        using (context.PushSetTransform(_matrix))
        {
            var r = new Rect(_bmp.Size);
            context.DrawBitmap(_bmp, r, r);
        }
    }
}