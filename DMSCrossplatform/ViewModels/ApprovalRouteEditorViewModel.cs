using System;
using System.Collections.Generic;
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

public partial class RouteStepViewModel : ObservableObject
{
    [ObservableProperty] private int _stepIndex;
    [ObservableProperty] private int? _nodeId;
    [ObservableProperty] private Guid? _approverId;
    [ObservableProperty] private string? _approverName;
    [ObservableProperty] private string? _approverEmail;
    [ObservableProperty] private SimpleDto? _selectedUnit;
    [ObservableProperty] private ObservableCollection<UserFullDto> _filteredUsers = [];
    [ObservableProperty] private UserFullDto? _selectedUser;
}

public partial class ApprovalRouteEditorViewModel : ViewModelBase
{
    private readonly IApprovalRouteService _approvalRouteService;
    private readonly IUserService _userService;
    private readonly IDictionariesService _dictionariesService;
    private readonly INavigationService<MenuRegionState> _navigationService;
    private readonly ILogger<ApprovalRouteEditorViewModel> _log;

    [ObservableProperty] private string _routeName = string.Empty;
    [ObservableProperty] private int? _routeId;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private ObservableCollection<RouteStepViewModel> _steps = [];
    [ObservableProperty] private ObservableCollection<SimpleDto> _units = [];
    [ObservableProperty] private ObservableCollection<UserFullDto> _availableUsers = [];
    [ObservableProperty] private RouteStepViewModel? _selectedStep;
    [ObservableProperty] private RouteGraphDto? _routeGraph;
    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private bool _isSaving = false;
    [ObservableProperty] private bool _canDelete = false;

    public ApprovalRouteEditorViewModel(
        IApprovalRouteService approvalRouteService,
        IUserService userService,
        IDictionariesService dictionariesService,
        INavigationService<MenuRegionState> navigationService,
        ILogger<ApprovalRouteEditorViewModel> log)
    {
        _approvalRouteService = approvalRouteService;
        _userService = userService;
        _dictionariesService = dictionariesService;
        _navigationService = navigationService;
        _log = log;
        if (App.SelectedApprovalRoute != null) _routeId = App.SelectedApprovalRoute.Id;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            var unitsTask = _dictionariesService.GetUnitsSimpleAsync();
            var usersTask = _userService.GetAllAsync();

            await Task.WhenAll(unitsTask, usersTask);

            Units = new ObservableCollection<SimpleDto>(unitsTask.Result);
            AvailableUsers = new ObservableCollection<UserFullDto>(usersTask.Result);

            if (RouteId.HasValue)
            {
                await LoadExistingRouteAsync(RouteId.Value);
                CanDelete = true;
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to initialize route editor");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadExistingRouteAsync(int routeId)
    {
        try
        {
            var route = await _approvalRouteService.GetAsync(routeId);
            var graph = await _approvalRouteService.GetGraphAsync(routeId);

            RouteName = route.Name;
            RouteGraph = graph;

            var sortedNodes = graph.Nodes.OrderBy(n => n.StepIndex).ToList();
            Steps.Clear();

            foreach (var node in sortedNodes)
            {
                var selectedUser = AvailableUsers.FirstOrDefault(u => u.UserId == node.ApproverId);
                var selectedUnit = selectedUser == null
                    ? null
                    : Units.FirstOrDefault(u => u.Id == selectedUser.UnitId);

                var step = new RouteStepViewModel
                {
                    StepIndex = node.StepIndex,
                    NodeId = node.Id,
                    ApproverId = node.ApproverId,
                    ApproverName = node.ApproverFullName,
                    ApproverEmail = node.ApproverEmail,
                    SelectedUnit = selectedUnit,
                    SelectedUser = selectedUser,
                    FilteredUsers = selectedUnit == null
                        ? []
                        : new ObservableCollection<UserFullDto>(
                            AvailableUsers.Where(u => u.UnitId == selectedUnit.Id))
                };
                step.PropertyChanged += StepPropertyChanged;
                Steps.Add(step);
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to load existing route");
        }
    }

    [RelayCommand]
    private void AddStep()
    {
        var newStep = new RouteStepViewModel
        {
            StepIndex = Steps.Count
        };
        newStep.PropertyChanged += StepPropertyChanged;
        Steps.Add(newStep);
        UpdateRouteGraph();
    }

    [RelayCommand]
    private void RemoveStep(RouteStepViewModel step)
    {
        if (Steps.Remove(step))
        {
            for (int i = 0; i < Steps.Count; i++)
            {
                Steps[i].StepIndex = i;
            }
            UpdateRouteGraph();
        }
    }

    [RelayCommand]
    private void MoveStepUp(RouteStepViewModel step)
    {
        var index = Steps.IndexOf(step);
        if (index > 0)
        {
            Steps.Move(index, index - 1);
            for (int i = 0; i < Steps.Count; i++)
            {
                Steps[i].StepIndex = i;
            }
            UpdateRouteGraph();
        }
    }

    [RelayCommand]
    private void MoveStepDown(RouteStepViewModel step)
    {
        var index = Steps.IndexOf(step);
        if (index < Steps.Count - 1)
        {
            Steps.Move(index, index + 1);
            for (int i = 0; i < Steps.Count; i++)
            {
                Steps[i].StepIndex = i;
            }
            UpdateRouteGraph();
        }
    }

    partial void OnSelectedStepChanged(RouteStepViewModel? oldValue, RouteStepViewModel? newValue)
    {
        if (oldValue != null)
        {
            oldValue.PropertyChanged -= StepPropertyChanged;
        }

        if (newValue != null)
        {
            newValue.PropertyChanged += StepPropertyChanged;
        }
    }

    private void StepPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RouteStepViewModel.SelectedUser))
        {
            if (sender is RouteStepViewModel step && step.SelectedUser != null)
            {
                step.ApproverId = step.SelectedUser.UserId;
                step.ApproverName = step.SelectedUser.FullName;
                step.ApproverEmail = step.SelectedUser.Email;
                UpdateRouteGraph();
            }
        }
        else if (e.PropertyName == nameof(RouteStepViewModel.SelectedUnit))
        {
            if (sender is RouteStepViewModel step)
            {
                if (step.SelectedUnit != null)
                {
                    var filteredUsers = AvailableUsers
                        .Where(u => u.UnitId == step.SelectedUnit.Id)
                        .ToList();
                    step.FilteredUsers = new ObservableCollection<UserFullDto>(filteredUsers);
                }
                else
                {
                    step.FilteredUsers.Clear();
                }
                step.SelectedUser = null;
            }
        }
    }

    private void UpdateRouteGraph()
    {
        var nodes = new List<RouteGraphNodeDto>();
        var edges = new List<RouteEdgeReadDto>();

        for (int i = 0; i < Steps.Count; i++)
        {
            var step = Steps[i];
            var node = new RouteGraphNodeDto
            {
                Id = step.NodeId ?? -(i + 1), 
                StepIndex = i,
                ApproverId = step.ApproverId ?? Guid.Empty,
                ApproverEmail = step.ApproverEmail,
                ApproverFullName = step.ApproverName,
                IsStart = i == 0,
                    IsTerminal = i == Steps.Count - 1,
                    Level = 0
            };
            nodes.Add(node);

            if (i > 0)
            {
                edges.Add(new RouteEdgeReadDto
                {
                    FromNodeId = nodes[i - 1].Id,
                    ToNodeId = node.Id
                });
            }
        }

        RouteGraph = new RouteGraphDto
        {
            Route = new ApprovalRouteReadDto { Name = RouteName },
            Nodes = nodes,
            Edges = edges
        };
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(RouteName))
        {
            _log.LogWarning("Cannot save route: name is empty or less than 2 steps");
            ErrorMessage = "Невозможно сохранить маршрут: Пустое имя маршрута";
            return;
        }

        if (Steps.Count < 2)
        {
            _log.LogWarning("Cannot save route:less than 2 steps");
            ErrorMessage = "Невозможно сохранить маршрут: маршрут содержит менее двух этапов";
            return;
        }
        if (Steps.Any(s => !s.ApproverId.HasValue))
        {
            _log.LogWarning("Cannot save route: some steps don't have approver assigned");
            ErrorMessage = "Невозможно сохранить маршрут: к этапу не прикреплен сотрудник";
            return;
        }
        
        var approvers = Steps.Select(s => s.ApproverId).ToList();
        if (approvers.Count != approvers.Distinct().Count())
        {
            _log.LogWarning("Cannot save route: duplicate approvers found");
            ErrorMessage = "Невозможно сохранить маршрут: один сотрудник указан дважды";
            return;
        }

        IsSaving = true;
        try
        {
            ApprovalRouteReadDto route;

            if (RouteId.HasValue)
            {
                route = await _approvalRouteService.UpdateAsync(
                    RouteId.Value,
                    new ApprovalRouteUpdateDto { Name = RouteName });

          
                if (RouteGraph != null)
                {
                    foreach (var edge in RouteGraph.Edges.Where(e => e.Id > 0))
                    {
                        await _approvalRouteService.DeleteEdgeAsync(RouteId.Value, edge.Id);
                    }

                    foreach (var node in RouteGraph.Nodes.Where(n => n.Id > 0))
                    {
                        await _approvalRouteService.DeleteNodeAsync(RouteId.Value, node.Id);
                    }
                }
            }
            else
            {

                route = await _approvalRouteService.CreateAsync(
                    new ApprovalRouteCreateDto { Name = RouteName });
                RouteId = route.Id;
            }


            var createdNodes = new List<RouteNodeReadDto>();
            foreach (var step in Steps)
            {
                var node = await _approvalRouteService.CreateNodeAsync(
                    route.Id,
                    new RouteNodeCreateDto
                    {
                        ApproverId = step.ApproverId!.Value,
                        StepIndex = step.StepIndex
                    });
                createdNodes.Add(node);
                step.NodeId = node.Id;
            }


            for (int i = 1; i < createdNodes.Count; i++)
            {
                await _approvalRouteService.CreateEdgeAsync(
                    route.Id,
                    new RouteEdgeCreateDto
                    {
                        FromNodeId = createdNodes[i - 1].Id,
                        ToNodeId = createdNodes[i].Id
                    });
            }

            _log.LogInformation("Route saved successfully");
            ErrorMessage = null; 
            _navigationService.NavigateTo<ApprovalRoutesListViewModel>();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to save route");
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _navigationService.NavigateTo<ApprovalRoutesListViewModel>();
    }
}
