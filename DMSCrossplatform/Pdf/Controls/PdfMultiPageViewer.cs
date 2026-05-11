using System.Collections.Generic;
using Avalonia.Controls;
using AvaloniaApplication1.Pdf.Abstractions;

namespace AvaloniaApplication1.Pdf.Controls;

public class PdfMultiPageViewer : UserControl
{
    private readonly StackPanel _stack;
    private readonly ScrollViewer _scroll;

    private readonly List<PdfPageControl> _realizedPages = new();

    public IPdfPageRasterizer? Rasterizer { get; set; }

    public int PageCount { get; set; }

    public PdfMultiPageViewer()
    {
        _stack = new StackPanel
        {
            Spacing = 16
        };

        _scroll = new ScrollViewer
        {
            Content = _stack
        };

        Content = _scroll;

        _scroll.ScrollChanged += OnScrollChanged;
    }

    public void InitializePages(int pageCount)
    {
        PageCount = pageCount;

        _stack.Children.Clear();
        _realizedPages.Clear();

        for (int i = 0; i < pageCount; i++)
        {
            var page = new PdfPageControl
            {
                Height = 800, // placeholder
                Rasterizer = Rasterizer
            };

            page.PageIndex = i;

            _stack.Children.Add(page);
            _realizedPages.Add(page);
        }
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        UpdateVirtualization();
    }

    private void UpdateVirtualization()
    {
        var viewport = _scroll.Viewport;
        var offset = _scroll.Offset;

        double top = offset.Y;
        double bottom = offset.Y + viewport.Height;

        for (int i = 0; i < _realizedPages.Count; i++)
        {
            var page = _realizedPages[i];

            var bounds = page.Bounds;

            bool isVisible =
                bounds.Bottom >= top - 500 &&
                bounds.Top <= bottom + 500;

            page.IsVisible = isVisible;
        }
    }
}