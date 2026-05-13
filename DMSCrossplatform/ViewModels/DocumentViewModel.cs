using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform;
using DMSCrossplatform.Infrastructure.Logging;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.Services;
using Microsoft.Extensions.Logging;

namespace DMSCrossplatform.ViewModels;

public partial class DocumentViewModel : ViewModelBase
{
    private readonly IDocumentService _documentService;
    private readonly IApprovalRouteService _approvalRouteService;
    private readonly ILogger<DocumentViewModel> _log;
    private readonly INavigationService<MenuRegionState> _navigationService;
    private readonly ISessionService _sessionService;

    [ObservableProperty] private DocumentFullReadDto? _document;
    [ObservableProperty] private ObservableCollection<MvDocumentVersionReadDto> _versions = [];
    [ObservableProperty] private MvDocumentVersionReadDto? _selectedVersion;
    [ObservableProperty] private ObservableCollection<ApprovalRouteReadDto> _availableRoutes = [];
    [ObservableProperty] private ApprovalRouteReadDto? _selectedRoute;
    [ObservableProperty] private RouteGraphDto? _documentRouteGraph;
    [ObservableProperty] private ObservableCollection<MvDocumentApprovalReadDto> _approvalHistory = [];
    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private bool _isAuthor = false;
    [ObservableProperty] private bool _canApprove = false;
    [ObservableProperty] private bool _canReject = false;
    [ObservableProperty] private bool _canSubmit = false;
    [ObservableProperty] private bool _canUploadVersion = false;
    [ObservableProperty] private bool _showRouteSelector = false;
    [ObservableProperty] private bool _showRejectDialog = false;
    [ObservableProperty] private string _rejectComment = string.Empty;
    [ObservableProperty] private string? _pdfUrl;
    [ObservableProperty] private int _selectedTabIndex = 0;
    [ObservableProperty] private RouteGraphDto? _selectedSubmitRouteGraph;

    public DocumentViewModel(
        IDocumentService documentService,
        IApprovalRouteService approvalRouteService,
        ILogger<DocumentViewModel> log,
        INavigationService<MenuRegionState> navigationService,
        ISessionService sessionService)
    {
        _documentService = documentService;
        _approvalRouteService = approvalRouteService;
        _log = log;
        _navigationService = navigationService;
        _sessionService = sessionService;
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

        var docs = await _documentService.ListAsync();
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

        // Статусы: 1 - черновик, 2 - на согласовании, 3 - отклонен, 4 - опубликован
        CanSubmit = IsAuthor && Document.StatusId == 1; // Черновик
        CanUploadVersion = IsAuthor && (Document.StatusId == 3 || Document.StatusId == 4); // Отклонен или опубликован

        // Проверяем, может ли текущий пользователь согласовывать/отклонять
        if (Document.StatusId == 2 && Document.RouteId.HasValue) // На согласовании
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
            catch
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
    private void CancelSubmit()
    {
        ShowRouteSelector = false;
        SelectedRoute = null;
    }

    [RelayCommand]
    private async Task Approve()
    {
        if (Document == null) return;

        try
        {
            await _documentService.ApproveAsync(Document.Id);
            await InitializeAsync(Document.Id);
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
}
