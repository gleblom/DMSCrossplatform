using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using DMSCrossplatform.Pdf.Abstractions;

namespace DMSCrossplatform.Pdf.Controls;

public sealed class PdfVirtualizingViewer : UserControl
{
    private readonly Canvas _canvas = new()
    {
        Background = Brushes.Transparent
    };

    private readonly ScrollViewer _scroll;

    private readonly Dictionary<int, PdfPageControl> _realized = new();
    private readonly Stack<PdfPageControl> _pool = new();
    private readonly List<double> _rawOffsets = new();
    private readonly List<Size> _pageSizes = new();

    private double _fitScale = 1.0;
    private double _userZoom = 1.0;
    private double _maxRawPageWidth = 0;
    private bool _isInitialFitApplied;

    private double _pinchStartZoom = 1.0;
    private bool _pinchActive;

    public IPdfDocumentSource? Document { get;  set; }
    public IPdfPageRasterizer? Rasterizer { get; set; }

    public double PageSpacing { get; set; } = 16;
    public double PreloadMargin { get; set; } = 800;
    public double MaxUserZoom { get; set; } = 4.0;
    public double MinUserZoom { get; set; } = 0.5;

    private double GlobalScale => _fitScale * _userZoom;

    public PdfVirtualizingViewer()
    {
        _scroll = new ScrollViewer
        {
            Content = _canvas,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        Content = _scroll;

        _scroll.ScrollChanged += (_, _) => UpdateVirtualization();

        GestureRecognizers.Add(new PinchGestureRecognizer());
        AddHandler(PinchEvent, OnPinch, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AddHandler(PinchEndedEvent, OnPinchEnded, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);

        SizeChanged += (_, _) =>
        {
            if (Document != null && !_isInitialFitApplied && Bounds.Width > 0)
                ApplyInitialFit();
        };
    }

    public void Init(IPdfDocumentSource doc)
    {
        Document = doc;
        _isInitialFitApplied = false;
        _maxRawPageWidth = 0;
        
        _pageSizes.Clear();

 
        for (int i = 0; i < doc.PageCount; i++)
        {
            var (w, h) = doc.GetPageSize(i);
            _pageSizes.Add(new Size(w, h));
            
            if (w > _maxRawPageWidth) _maxRawPageWidth = w;
        }

        if (Bounds.Width > 0)
            ApplyInitialFit();
    }
    

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        e.Handled = true;

        var zoomFactor = e.Delta.Y > 0 ? 1.1 : 0.9;
        UpdateZoom(_userZoom * zoomFactor);
    }

    private void OnPinch(object? sender, PinchEventArgs e)
    {
        e.Handled = true;

        if (!_pinchActive)
        {
            _pinchActive = true;
            _pinchStartZoom = _userZoom;
        }

        UpdateZoom(_pinchStartZoom * e.Scale);
    }

    private void OnPinchEnded(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        _pinchActive = false;
        _pinchStartZoom = _userZoom;
    }

    private void ApplyInitialFit()
    {
        if (Document is null || Bounds.Width <= 0 || _maxRawPageWidth <= 0)
            return;

        _fitScale = Bounds.Width / _maxRawPageWidth;
        _isInitialFitApplied = true;
        RecalculateLayout();
    }

    private void UpdateZoom(double targetUserZoom)
    {
        if (Document is null || _maxRawPageWidth <= 0)
            return;

        var newUserZoom = Math.Clamp(targetUserZoom, MinUserZoom, MaxUserZoom);

        if (Math.Abs(_userZoom - newUserZoom) < 0.0001)
            return;

        _userZoom = newUserZoom;
        RecalculateLayout();
    }

    private void RecalculateLayout()
    {
        if (Document == null || _pageSizes.Count == 0) return;

        double y = 0;
        _rawOffsets.Clear();

        for (int i = 0; i < Document.PageCount; i++)
        {
            _rawOffsets.Add(y);
            // Берем высоту из локального кэша, никаких вызовов Docnet!
            y += (_pageSizes[i].Height * GlobalScale) + 16; 
        }

        _canvas.Width = Math.Max(Bounds.Width, _maxRawPageWidth * GlobalScale);
        _canvas.Height = y;

        foreach (var kvp in _realized)
        {
            int idx = kvp.Key;
            var page = kvp.Value;
            var size = _pageSizes[idx]; // Из кэша

            page.Width = size.Width * GlobalScale;
            page.Height = size.Height * GlobalScale;
            Canvas.SetTop(page, _rawOffsets[idx]);
            Canvas.SetLeft(page, (_canvas.Width - page.Width) / 2);
            page.RenderScale = GlobalScale;
        }

        UpdateVirtualization();
    }

    private void UpdateVirtualization()
    {
        if (Document == null || _pageSizes.Count == 0 || _rawOffsets.Count == 0) return;

        double top = _scroll.Offset.Y - 800; // Буфер сверху
        double bottom = _scroll.Offset.Y + _scroll.Viewport.Height + 800; // Буфер снизу

        // 2. БИНАРНЫЙ ПОИСК (O(log N))
        // Мгновенно находим индекс первой страницы, попадающей в видимую зону
        int startIdx = _rawOffsets.BinarySearch(top);
        if (startIdx < 0) startIdx = ~startIdx - 1; // Берем предыдущую страницу, если не точное совпадение
        startIdx = Math.Max(0, startIdx);

        var visibleIndices = new HashSet<int>();

       
        for (int i = startIdx; i < Document.PageCount; i++)
        {
            double y = _rawOffsets[i];
            

            if (y > bottom) break; 

            visibleIndices.Add(i);

            if (!_realized.ContainsKey(i))
                Realize(i);
        }
        
        var keysToRecycle = _realized.Keys.Where(k => !visibleIndices.Contains(k)).ToList();
        foreach (var k in keysToRecycle)
        {
            Recycle(k);
        }
    }

    private void Realize(int i)
    {
        if (Document is null || Rasterizer is null)
            return;

        var page = _pool.Count > 0 ? _pool.Pop() : new PdfPageControl
        {
            Background = Brushes.Transparent
        };

        var (w, h) = _pageSizes[i];
        var scale = GlobalScale;

        page.PageIndex = i;
        page.Rasterizer = Rasterizer;
        page.RenderScale = scale;
        page.Width = w * scale;
        page.Height = h * scale;

        Canvas.SetTop(page, _rawOffsets[i]);
        Canvas.SetLeft(page, (_canvas.Width - page.Width) / 2);

        if (!_canvas.Children.Contains(page))
            _canvas.Children.Add(page);

        _realized[i] = page;
    }

    private void Recycle(int i)
    {
        if (!_realized.TryGetValue(i, out var page))
            return;

        _realized.Remove(i);
        _canvas.Children.Remove(page);
        page.CancelRender();

        page.Rasterizer = null;
        _pool.Push(page);
    }
}