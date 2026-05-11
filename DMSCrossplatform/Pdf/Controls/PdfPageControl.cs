using Avalonia.Media;

namespace AvaloniaApplication1.Pdf.Controls;

public class PdfPageControl : PdfViewerControl
{
    public PdfPageControl()
    {
        Background = Brushes.Transparent;
    }
    public int PageIndex
    {
        get => _pageIndex;
        set
        {
            _pageIndex = value;
            TriggerInitialRender();
        }
    }

    private void TriggerInitialRender()
    {
        RestartDebounce();
    }
}