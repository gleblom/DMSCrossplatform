using System.IO;
using DMSCrossplatform.Pdf.Abstractions;
using Docnet.Core;
using Docnet.Core.Models;

namespace DMSCrossplatform.Pdf.Models;

public sealed class SimplePdfDocument : IPdfDocumentSource
{
    private readonly byte[] _bytes;

    public int PageCount { get; }

    public SimplePdfDocument(string path)
    {
        _bytes = File.ReadAllBytes(path);

        using var doc = DocLib.Instance.GetDocReader(_bytes, new PageDimensions(1));
        PageCount = doc.GetPageCount();
    }

    public (double width, double height) GetPageSize(int pageIndex)
    {
        using var doc = DocLib.Instance.GetDocReader(_bytes, new PageDimensions(1));
        using var page = doc.GetPageReader(pageIndex);
        

        return (page.GetPageWidth(), page.GetPageHeight());
    }
}