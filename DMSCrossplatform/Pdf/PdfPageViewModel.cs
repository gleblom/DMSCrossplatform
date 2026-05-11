namespace AvaloniaApplication1.Pdf;

public sealed class PdfPageViewModel
{
    public int PageIndex { get; }

    public double Width { get; set; }
    public double Height { get; set; }

    public PdfPageViewModel(int index)
    {
        PageIndex = index;
    }
}