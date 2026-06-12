using Avalonia.Media;

namespace DMSCrossplatform.Pdf.Controls;

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