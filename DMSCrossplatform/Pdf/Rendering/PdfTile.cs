using Avalonia;
using Avalonia.Media.Imaging;

namespace AvaloniaApplication1.Pdf.Rendering;

public readonly record struct TileKey(int Page, int X, int Y, int ScaleBucket);

public sealed class PdfTile
{
    public int X { get; }
    public int Y { get; }
    public double Scale { get; }

    public WriteableBitmap? Bitmap;

    public bool IsRendering;

    public PdfTile(int x, int y, double scale)
    {
        X = x;
        Y = y;
        Scale = scale;
    }

    public Rect GetBounds(double tileSize)
    {
        return new Rect(
            X * tileSize,
            Y * tileSize,
            tileSize,
            tileSize);
    }
}