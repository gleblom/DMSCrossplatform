using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure.Logging;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.Services;
using Microsoft.Extensions.Logging;

namespace DMSCrossplatform.ViewModels;

public partial class ApprovalRoutesListViewModel : ViewModelBase
{
    private readonly IApprovalRouteService _approvalRouteService;
    private readonly INavigationService<MenuRegionState> _navigationService;
    private readonly ILogger<ApprovalRoutesListViewModel> _log;

    [ObservableProperty] private ObservableCollection<ApprovalRouteReadDto> _routes = [];
    [ObservableProperty] private ApprovalRouteReadDto? _selectedRoute;
    [ObservableProperty] private RouteGraphDto? _selectedRouteGraph;
    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private string _loadingMessage = "Загрузка маршрутов...";

    public ApprovalRoutesListViewModel(
        IApprovalRouteService approvalRouteService,
        INavigationService<MenuRegionState> navigationService,
        ILogger<ApprovalRoutesListViewModel> log)
    {
        _approvalRouteService = approvalRouteService;
        _navigationService = navigationService;
        _log = log;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            await LoadRoutesAsync();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to load approval routes");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadRoutesAsync()
    {
        var routes = await _approvalRouteService.ListAsync();
        Routes = new ObservableCollection<ApprovalRouteReadDto>(routes);
    }

    partial void OnSelectedRouteChanged(ApprovalRouteReadDto? value)
    {
        if (value != null)
        {
            _ = LoadRouteGraphAsync(value.Id);
        }
    }

    private async Task LoadRouteGraphAsync(int routeId)
    {
        try
        {
            SelectedRouteGraph = await _approvalRouteService.GetGraphAsync(routeId);
            
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to load route graph for route {RouteId}", routeId);
        }
    }

    [RelayCommand]
    private void CreateRoute()
    {
        App.SelectedApprovalRoute = null;
        _navigationService.NavigateTo<ApprovalRouteEditorViewModel>();
    }

    [RelayCommand]
    private void EditRoute()
    {
        if (SelectedRoute != null)
        {
            App.SelectedApprovalRoute = SelectedRoute;
            _navigationService.NavigateTo<ApprovalRouteEditorViewModel>();
        }
    }

    [RelayCommand]
    private async Task DeleteRoute()
    {
        if (SelectedRoute != null)
        {
            var route = SelectedRoute;
            try
            {
                await _approvalRouteService.DeleteAsync(route.Id);
                Routes.Remove(route);
                SelectedRoute = null;
                SelectedRouteGraph = null;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to delete route {RouteId}", route.Id);
            }
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        IsLoading = true;
        try
        {
            await LoadRoutesAsync();
            if (SelectedRoute != null)
            {
                var updatedRoute = Routes.FirstOrDefault(r => r.Id == SelectedRoute.Id);
                if (updatedRoute != null)
                {
                    SelectedRoute = updatedRoute;
                }
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to refresh routes");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
