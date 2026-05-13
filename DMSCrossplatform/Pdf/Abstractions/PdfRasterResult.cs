namespace DMSCrossplatform.Pdf.Abstractions;

public sealed class PdfRasterResult
{
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required byte[] Bgra { get; init; } 
}