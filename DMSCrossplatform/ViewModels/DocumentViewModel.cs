using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Android.Util;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using BarcodeLib.BarcodeReader;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform;
using DMSCrossplatform.Infrastructure;
using DMSCrossplatform.Infrastructure.Logging;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using QRCoder;

namespace DMSCrossplatform.ViewModels;

public partial class DocumentViewModel : ViewModelBase
{
    private readonly IDocumentService _documentService;
    private readonly IApprovalRouteService _approvalRouteService;
    private readonly ILogger<DocumentViewModel> _log;
    private readonly IStorageProvider? _storageProvider;
    private readonly INavigationService<MenuRegionState> _navigationService;
    private readonly ISessionService _sessionService;
    private readonly IDownloadSaver _downloadSaver;
        
    public static string? Mode = null;

    [ObservableProperty] private DocumentFullReadDto? _document;
    [ObservableProperty] private ObservableCollection<MvDocumentVersionReadDto> _versions = [];
    [ObservableProperty] private MvDocumentVersionReadDto? _selectedVersion;
    [ObservableProperty] private ObservableCollection<ApprovalRouteReadDto> _availableRoutes = [];
    [ObservableProperty] private ApprovalRouteReadDto? _selectedRoute;
    [ObservableProperty] private RouteGraphDto? _documentRouteGraph;
    [ObservableProperty] private ObservableCollection<MvDocumentApprovalReadDto> _approvalHistory = [];
    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private bool _isAuthor;
    [ObservableProperty] private bool _canApprove ;
    [ObservableProperty] private bool _canReject;
    [ObservableProperty] private bool _canSubmit;
    [ObservableProperty] private bool _canDownload;
    [ObservableProperty] private bool _canUploadVersion;
    [ObservableProperty] private bool _showRouteSelector;
    [ObservableProperty] private bool _showShareQr;
    
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _showRejectDialog;
    
    [ObservableProperty] private string? _selectedFilePath;
    [ObservableProperty] private string? _selectedFileName;
    [ObservableProperty] private string _rejectComment = string.Empty;
    [ObservableProperty] private string? _pdfUrl;
    
    [ObservableProperty] private int _selectedTabIndex;
    
    [ObservableProperty] private RouteGraphDto? _selectedSubmitRouteGraph;
    
    [ObservableProperty] private Bitmap _qrCodeBitmap;
    

    [ObservableProperty] private byte[] _qrCodeBytes;

    public DocumentViewModel(
        IDocumentService documentService,
        IApprovalRouteService approvalRouteService,
        ILogger<DocumentViewModel> log,
        INavigationService<MenuRegionState> navigationService,
        ISessionService sessionService,
        IDownloadSaver downloadSaver)
    {
        _documentService = documentService;
        _approvalRouteService = approvalRouteService;
        _log = log;
        _navigationService = navigationService;
        _storageProvider = App.storageProvider;
        _sessionService = sessionService;
        _downloadSaver = downloadSaver;
        if (App.SelectedDocumentId.HasValue)
            _ = InitializeAsync(App.SelectedDocumentId.Value);
        else
            IsLoading = false;
    }

    private async Task InitializeAsync(Guid documentId)
    {
        IsLoading = true;
        try
        {
            var docTask = LoadDocumentAsync(documentId);
            var versionsTask = _documentService.GetDocumentVersionsAsync(documentId);
            var approvalsTask = _documentService.GetDocumentApprovalsAsync(documentId);

            await Task.WhenAll(docTask, versionsTask, approvalsTask);

            Document = docTask.Result;
            Versions = new ObservableCollection<MvDocumentVersionReadDto>(versionsTask.Result);
            ApprovalHistory = new ObservableCollection<MvDocumentApprovalReadDto>(approvalsTask.Result);

            if (Versions.Any())
            {
                SelectedVersion = Versions.First();
            }

            await UpdatePermissions();

            if (Document?.RouteId.HasValue == true)
            {
                await LoadDocumentRoute(Document.RouteId.Value);
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to initialize document view");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<DocumentFullReadDto> LoadDocumentAsync(Guid documentId)
    {

        var docs = await _documentService.ListAsync(mode: Mode);
        var doc = docs.FirstOrDefault(d => d.Id == documentId);

        if (doc == null)
        {
            throw new InvalidOperationException($"Document with ID {documentId} not found");
        }

        return doc;
    }

    private async Task LoadDocumentRoute(int routeId)
    {
        try
        {
            DocumentRouteGraph = await _approvalRouteService.GetGraphAsync(routeId);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to load document route");
        }
    }

    private async Task UpdatePermissions()
    {
        if (Document == null) return;

        var currentUser = _sessionService.GetCurrentUser();
        if (currentUser == null) return;

        IsAuthor = Document.AuthorId == currentUser.UserId;

        // Статусы: 1 - опубликован, 2 - черновик, 3 - на согласовании, 4 - не согласован (или отклонен)
        CanSubmit = IsAuthor && Document.StatusId == 2; // Черновик
        CanUploadVersion = IsAuthor && (Document.StatusId == 1 || Document.StatusId == 4) && Document.RouteId != null; // Отклонен или опубликован
        CanDownload = (Document.StatusId == 1 || Document.StatusId == 4) && Document.RouteId != null; 

        // Проверяем, может ли текущий пользователь согласовывать/отклонять
        if (Document.StatusId == 3 && Document.RouteId.HasValue) // На согласовании
        {
            try
            {
                var versionId = Document.LatestVersionId != 0
                    ? Document.LatestVersionId
                    : Versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault()?.Id ?? 0;

                if (versionId == 0)
                    return;

                var approval = await _documentService.GetApprovalByStep(
                    Document.Id,
                    Document.CurrentStepIndex,
                    versionId);

                CanApprove = approval.ApproverId == currentUser.UserId && !approval.IsApproved;
                CanReject = approval.ApproverId == currentUser.UserId && !approval.IsApproved;
           
            }
            catch(Exception ex)
            {
                CanApprove = false;
                CanReject = false;
            }
        }
    }

    partial void OnSelectedVersionChanged(MvDocumentVersionReadDto? value)
    {
        if (value != null)
        {
            _ = LoadPdfUrlAsync(value.Id);
        }
    }

    private async Task LoadPdfUrlAsync(int versionId)
    {
        if (Document == null) return;

        try
        {
            PdfUrl = await _documentService.GetDownloadUrlAsync(Document.Id, versionId);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to load PDF URL");
        }
    }

    [RelayCommand]
    private async Task SubmitForApproval()
    {
        if (Document == null) return;
        
        var routes = await _approvalRouteService.ListAsync();
        AvailableRoutes = new ObservableCollection<ApprovalRouteReadDto>(routes);
        ShowRouteSelector = true;
    }

    partial void OnSelectedRouteChanged(ApprovalRouteReadDto? value)
    {
        if (value == null)
        {
            SelectedSubmitRouteGraph = null;
            return;
        }

        _ = LoadSelectedSubmitRouteGraphAsync(value.Id);
    }

    private async Task LoadSelectedSubmitRouteGraphAsync(int routeId)
    {
        try
        {
            SelectedSubmitRouteGraph = await _approvalRouteService.GetGraphAsync(routeId);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to load selected submit route graph");
        }
    }

    [RelayCommand]
    private async Task ConfirmSubmit()
    {
        if (Document == null || SelectedRoute == null) return;

        try
        {
            await _documentService.SubmitAsync(Document.Id, SelectedRoute.Id);
            ShowRouteSelector = false;
            await InitializeAsync(Document.Id);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to submit document for approval");
        }
    }

    [RelayCommand]
    private async Task UploadNewVersion()
    {
        if (_storageProvider == null)
        {
                ErrorMessage = "Выбор файла недоступен";
                return;
        }

        try
        {
            var options = new FilePickerOpenOptions
            {
                Title = "Выберите документ",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("Документы")
                    {
                        Patterns = ["*.pdf"],
                        AppleUniformTypeIdentifiers = ["com.adobe.pdf"],
                        MimeTypes =
                        [
                            "application/pdf"
                        ]
                    }
                ]
            };

            var files = await _storageProvider.OpenFilePickerAsync(options);

            if (files.Count == 1)
            {
                var file = files[0];
                SelectedFilePath = file.Path.LocalPath;
                SelectedFileName = file.Name;

                if (OperatingSystem.IsAndroid())
                {
                    await CopyFileForAndroidAsync(file);
                }

                if (Document.RouteId == null)
                {
                    ErrorMessage = $"Маршрут не прикреплен к документу";
                    return;
                }

                var fileBytes = File.ReadAllBytes(SelectedFilePath);

                var provider = new FileExtensionContentTypeProvider();
                string contentType = provider.TryGetContentType(SelectedFilePath, out contentType)
                    ? contentType
                    : "application/octet-stream";


                var doc = await _documentService.UploadVersionAsync(
                    Document.Id, fileBytes,
                    SelectedFileName,
                    contentType
                );



                await _documentService.SubmitAsync(Document.Id, (int)Document.RouteId);
                Mode = "my";
                _ = InitializeAsync(Document.Id);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{ex.Message}";
        }
    }

    private async Task CopyFileForAndroidAsync(IStorageFile file)
    {

        try
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var targetPath = Path.Combine(appDataPath, "temp_documents", file.Name);

            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

            await using var sourceStream = await file.OpenReadAsync();
            await using var targetStream = File.Create(targetPath);

            await sourceStream.CopyToAsync(targetStream);

            SelectedFilePath = targetPath;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{ex.Message}";
        }
    }

    [RelayCommand]
    private void CancelSubmit()
    {
        ShowRouteSelector = false;
        SelectedRoute = null;
    }

    [RelayCommand]
    private async Task Approve()
    {
        if (Document == null) return;
        var currentUser = _sessionService.GetCurrentUser();
        IsAuthor = Document.AuthorId == currentUser.UserId;
        try
        {
            var index = DocumentRouteGraph.Nodes.Last().StepIndex;
            if (Document.CurrentStepIndex == index)
            {
                Mode = "all";
                CanApprove = false;
                CanReject = false;
            }
            await _documentService.ApproveAsync(Document.Id);
            await InitializeAsync(Document.Id);
            if (!IsAuthor)
            {
                _navigationService.NavigateTo<DocumentsListViewModel>();
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to approve document");
        }
    }

    [RelayCommand]
    private void RequestReject()
    {
        RejectComment = string.Empty;
        ShowRejectDialog = true;
    }

    [RelayCommand]
    private void CancelReject()
    {
        RejectComment = string.Empty;
        ShowRejectDialog = false;
    }

    [RelayCommand]
    private async Task ConfirmReject()
    {
        if (Document == null || string.IsNullOrWhiteSpace(RejectComment)) return;

        try
        {
            await _documentService.RejectAsync(Document.Id, RejectComment.Trim());
            ShowRejectDialog = false;
            await InitializeAsync(Document.Id);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to reject document");
        }
    }

    [RelayCommand]
    private void Back()
    {
        _navigationService.NavigateTo<DocumentsListViewModel>();
    }

    [RelayCommand]
    private async Task DownloadQr()
    {
        if (_storageProvider == null || QrCodeBytes == null)
        {
            ErrorMessage = "Невозможно скачать файл";
            return;
        }
        
        await _downloadSaver.SaveAsync(QrCodeBytes, "qrcode.png", "image/png");
    }

    [RelayCommand]
    private async Task Download()
    {
        if (_storageProvider == null)
        {
            ErrorMessage = "Невозможно скачать файл";
            return;
        }

        if (PdfUrl == null)
        {
            ErrorMessage = "URL для скачивания документа не найдена";
            return;
        }
     
        var client = new HttpClient();
        await using var stream = await client.GetStreamAsync(PdfUrl);
        await _downloadSaver
            .SaveAsync(stream, Guid.NewGuid() + ".pdf", "application/pdf");

    }

    [RelayCommand]
    private async Task Share()
    {
        try
        {
            var shareLink = await _documentService.CreateShareLink(Document.Id);
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(shareLink.ShareLink, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            QrCodeBytes = qrCode.GetGraphic(20);
            var bitmap = new Bitmap(new MemoryStream(QrCodeBytes));
            QrCodeBitmap = bitmap;
            ShowShareQr = true;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to share document");
        }
    }
    

    

    [RelayCommand]
    private void CancelShare()
    {
        ShowShareQr = false;
    }
}
