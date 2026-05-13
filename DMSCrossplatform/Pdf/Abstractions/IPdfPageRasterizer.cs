using System.Threading;
using System.Threading.Tasks;

namespace DMSCrossplatform.Pdf.Abstractions;

public interface IPdfPageRasterizer
{
    Task<PdfRasterResult> RasterizeAsync(
        int pageIndex,
        double scale,
        CancellationToken ct);
}