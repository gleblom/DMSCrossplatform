using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaApplication1.Pdf.Abstractions;
using Docnet.Core;
using Docnet.Core.Models;
using Docnet.Core.Readers;
using SkiaSharp;

namespace AvaloniaApplication1.Pdf.Rendering;

public sealed class DocnetRasterizer : IPdfPageRasterizer
{
    private readonly byte[] _bytes;
    private readonly object _syncLock = new();

    public DocnetRasterizer(byte[] bytes)
    {
        _bytes = bytes;
    }

    public Task<PdfRasterResult> RasterizeAsync(int pageIndex, double scale, CancellationToken ct)
    {
        return Task.Run(() =>
        {
            lock (_syncLock) 
            {
                ct.ThrowIfCancellationRequested();
                using var doc = DocLib.Instance.GetDocReader(_bytes, new PageDimensions(scale));
                using var page = doc.GetPageReader(pageIndex);

                var rawBytes = page.GetImage();
                var baseWidth = page.GetPageWidth();
                var baseHeight = page.GetPageHeight();


                var width = (int)(baseWidth * scale);
                var height = (int)(baseHeight * scale);

                ct.ThrowIfCancellationRequested();


                var scaledImageData = ScaleImage(rawBytes, baseWidth, baseHeight, width, height);
                
                Premultiply(scaledImageData);

                return new PdfRasterResult
                {
                    Width = width,
                    Height = height,
                    Bgra = scaledImageData
                };
            }
        }, ct);
            
    }
    private static void Premultiply(byte[] data)
    {
        for (int i = 0; i < data.Length; i += 4)
        {
            byte a = data[i + 3];
            if (a == 255) continue;

            data[i + 0] = (byte)(data[i + 0] * a / 255); // B
            data[i + 1] = (byte)(data[i + 1] * a / 255); // G
            data[i + 2] = (byte)(data[i + 2] * a / 255); // R
        }
    }


    private byte[] ScaleImage(byte[] rawImageData, int originalWidth, int originalHeight, int newWidth, int newHeight)
    {

        using var originalBitmap =
            new SKBitmap(originalWidth, originalHeight, SKColorType.Bgra8888, SKAlphaType.Premul);

        Marshal.Copy(rawImageData, 0, originalBitmap.GetPixels(), rawImageData.Length);


        using var scaledBitmap = new SKBitmap(newWidth, newHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(scaledBitmap);

        using var paint = new SKPaint();
        paint.FilterQuality = SKFilterQuality.High;


        canvas.DrawBitmap(originalBitmap, new SKRect(0, 0, newWidth, newHeight), paint);
        canvas.Flush();

        var result = new byte[newWidth * newHeight * 4];
        Marshal.Copy(scaledBitmap.GetPixels(), result, 0, result.Length);

        return result;
    }

}