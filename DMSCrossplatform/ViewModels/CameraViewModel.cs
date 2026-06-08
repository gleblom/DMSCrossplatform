using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Services;

namespace DMSCrossplatform.ViewModels;

public partial class CameraViewModel : ViewModelBase
{
    private readonly IDocumentService _documentService;
    private readonly INavigationService<MenuRegionState> _navigationService;
    private bool _isQrCodeHandled;

    [ObservableProperty] private string _infoText = "Отсканируйте QR-код с документом";

    public CameraViewModel(
        IDocumentService documentService,
        INavigationService<MenuRegionState> navigationService)
    {
        _documentService = documentService;
        _navigationService = navigationService;
    }

    public async Task HandleQrCodeAsync(string qrResult)
    {
        if (_isQrCodeHandled)
            return;

        _isQrCodeHandled = true;

        try
        {
            var document = await _documentService.ConfirmShareLink(qrResult);
            App.SelectedDocumentId = document.Id;
            _navigationService.NavigateTo<DocumentViewModel>();
        }
        catch (Exception ex)
        {
            _isQrCodeHandled = false;
            InfoText = ex.Message;
        }
    }
}
