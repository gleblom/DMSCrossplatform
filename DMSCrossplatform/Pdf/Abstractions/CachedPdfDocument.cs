using System;
using System.IO;
using Docnet.Core;
using Docnet.Core.Models;

namespace DMSCrossplatform.Pdf.Abstractions;

public sealed class CachedPdfDocument : IPdfDocumentSource, IDisposable
{
    private readonly byte[] _bytes;
    private readonly (double Width, double Height)[] _sizes;

    public int PageCount => _sizes.Length;

    public CachedPdfDocument(string path)
    {
        _bytes = File.ReadAllBytes(path);

        using var doc = DocLib.Instance.GetDocReader(_bytes, new PageDimensions(1, 1));
        var count = doc.GetPageCount();

        _sizes = new (double Width, double Height)[count];

        for (int i = 0; i < count; i++)
        {
            using var page = doc.GetPageReader(i);
            _sizes[i] = (page.GetPageWidth(), page.GetPageHeight());
        }
    }

    public (double width, double height) GetPageSize(int pageIndex) => _sizes[pageIndex];

    public byte[] GetBytes() => _bytes;

    public void Dispose()
    {
       
    }
}