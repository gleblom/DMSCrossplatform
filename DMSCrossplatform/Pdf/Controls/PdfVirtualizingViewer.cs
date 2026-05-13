using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using DMSCrossplatform.Pdf.Abstractions;

namespace DMSCrossplatform.Pdf.Controls;

public class PdfVirtualizingViewer : UserControl
{
    private readonly Canvas _canvas = new();
    private readonly ScrollViewer _scroll;

    private readonly Dictionary<int, PdfPageControl> _realized = new();
    private readonly Stack<PdfPageControl> _pool = new();

    private List<double> _offsets = new();

    public IPdfDocumentSource? Document { get; set; }
    public IPdfPageRasterizer? Rasterizer { get; set; }

    public PdfVirtualizingViewer()
    {
        _scroll = new ScrollViewer { Content = _canvas };
        Content = _scroll;

        _scroll.ScrollChanged += (_, _) => Update();
    
        // Добавьте это: обновление при изменении размера окна/контрола
        SizeChanged += (_, _) => {
            if (Document != null) {
                _canvas.Width = Bounds.Width; // Canvas должен быть шириной с экран
                Update(); 
            }
        };
    }

    public void Init(IPdfDocumentSource doc)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => Init(doc));
            return;
        }
        Document = doc;

        double y = 0;
        _offsets.Clear();

        for (int i = 0; i < doc.PageCount; i++)
        {
            _offsets.Add(y);
            var (_, h) = doc.GetPageSize(i);
            y += h + 16;
        }

        _canvas.Height = y;

        Update();
    }

    private void Update()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(Update);
            return;
        }
        if (Document == null) return;

        double top = _scroll.Offset.Y;
        double bottom = top + _scroll.Viewport.Height;

        for (int i = 0; i < Document.PageCount; i++)
        {
            var y = _offsets[i];

            bool visible = y < bottom + 800 && y > top - 800;

            if (visible && !_realized.ContainsKey(i))
                Realize(i);
            else if (!visible && _realized.ContainsKey(i))
                Recycle(i);
        }
    }

    private void Realize(int i)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => Realize(i));
            return;
        }
        var page = _pool.Count > 0 ? _pool.Pop() : new PdfPageControl();
        
        
        page.PageIndex = i;
        page.Rasterizer = Rasterizer!;

        var (w, h) = Document!.GetPageSize(i);
        
        page.Width = w; 
        page.Height = h;

        Canvas.SetTop(page, _offsets[i]);

        if (!_canvas.Children.Contains(page))
            _canvas.Children.Add(page);
    
        _realized[i] = page;
    }

    private void Recycle(int i)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => Recycle(i));
            return;
        }
        var page = _realized[i];

        _realized.Remove(i);
        _canvas.Children.Remove(page);

        page.Rasterizer = null;

        _pool.Push(page);
    }
    
}