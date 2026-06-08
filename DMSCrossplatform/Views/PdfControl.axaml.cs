using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using DMSCrossplatform.Pdf.Controls;
using DMSCrossplatform.Pdf.Models;
using DMSCrossplatform.Pdf.Rendering;

namespace DMSCrossplatform.Views;

public partial class PdfControl : UserControl
{
    public static readonly StyledProperty<string?> PdfUrlProperty =
        AvaloniaProperty.Register<PdfControl, string?>(nameof(PdfUrl));

    public string? PdfUrl
    {
        get => GetValue(PdfUrlProperty);
        set => SetValue(PdfUrlProperty, value);
    }

    private PdfVirtualizingViewer? _pdfViewer;

    public PdfControl()
    {
        InitializeComponent();
        this.PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == PdfUrlProperty)
        {
            _ = LoadPdfAsync(PdfUrl);
        }
    }

    private async Task LoadPdfAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            LoadingText.Text = "PDF не выбран";
            return;
        }

        LoadingText.Text = "Загрузка PDF...";
        LoadingText.IsVisible = true;

        try
        {
            // Загружаем PDF файл
            using var httpClient = new HttpClient();
            var pdfBytes = await httpClient.GetByteArrayAsync(url);

            // Сохраняем во временный файл для SimplePdfDocument
            var tempFile = Path.GetTempFileName();
            await File.WriteAllBytesAsync(tempFile, pdfBytes);

            // Создаем PDF документ и rasterizer
            var pdfDocument = new SimplePdfDocument(tempFile);
            var rasterizer = new DocnetRasterizer(pdfBytes);

 
            _pdfViewer = new PdfVirtualizingViewer
            {
                Document = pdfDocument,
                Rasterizer = rasterizer
            };


            

            _pdfViewer.Init(pdfDocument);
            PdfContainer.Content = _pdfViewer;

            LoadingText.IsVisible = false;
        }
        catch (Exception ex)
        {
            LoadingText.Text = $"Ошибка загрузки PDF: {ex.Message}";
            LoadingText.IsVisible = true;
        }
    }
}
