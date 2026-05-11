using Avalonia;
using Avalonia.Controls;

namespace AvaloniaApplication1.Pdf.Controls;

public class PdfVirtualizingPanel : Panel
{
    public double PageSpacing { get; set; } = 16;

    protected override Size MeasureOverride(Size availableSize)
    {
        double y = 0;

        foreach (var child in Children)
        {
            child.Measure(new Size(availableSize.Width, double.PositiveInfinity));
            y += child.DesiredSize.Height + PageSpacing;
        }

        return new Size(availableSize.Width, y);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double y = 0;

        foreach (var child in Children)
        {
            var h = child.DesiredSize.Height;

            child.Arrange(new Rect(0, y, finalSize.Width, h));

            y += h + PageSpacing;
        }

        return finalSize;
    }
}