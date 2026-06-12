using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure.Policy;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.Services;
using DMSCrossplatform.ViewModels.Custom;

namespace DMSCrossplatform.ViewModels;

public partial class UserListViewModel : ViewModelBase
{
    private readonly IUserService _userService;
    private readonly IDictionariesService _dictionariesService;
    private readonly IPolicy _policy;

    [ObservableProperty] private UserEditViewModel _userEditViewModel;
    [ObservableProperty] private string? _searchQuery;
    [ObservableProperty] private MultiSelectViewModel<RoleReadDto>? _roles;
    [ObservableProperty] private MultiSelectViewModel<UnitReadDto>? _units;
    [ObservableProperty] private bool _isPaneOpen;
    [ObservableProperty] private bool _canSeeUnits;
    [ObservableProperty] private ObservableCollection<UserFullDto> _users = new();

    private UserFullDto? _selectedUser;

    public UserFullDto? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (SetProperty(ref _selectedUser, value) && value is not null)
            {
                _ = BeginEditUser(value);
            }
        }
    }

    public UserListViewModel(
        IDictionariesService dictionariesService,
        IUserService userService,
        UserEditViewModel userEditViewModel,
        IPolicyFactory factory)
    {
        _dictionariesService = dictionariesService;
        _userService = userService;
        UserEditViewModel = userEditViewModel;
        UserEditViewModel.Saved += OnEditorSaved;

        _policy = factory.CreatePolicy();
        CanSeeUnits = _policy.CanSeeUnits;

        _ = LoadData();
        
    }

    [RelayCommand]
    private void RefreshUsers()
    {
        _ = LoadData();
    }

    [RelayCommand]
    private async Task Clear()
    {
        SearchQuery = string.Empty;
        await LoadData();
    }

    private async Task LoadData()
    {
        var users = await LoadUsers();
        Users = new ObservableCollection<UserFullDto>(_policy.Users(users.ToList()));
        await LoadRoles();
        await LoadUnits();
    }

    [RelayCommand]
    private async Task Search()
    {
        var units = _policy.CanSeeUnits
            ? Units?.Items.Where(i => i.IsSelected).Select(c => c.Item.Id).ToList()
            : null;

        var roles = Roles?.Items.Where(i => i.IsSelected).Select(c => c.Item.Id).ToList();
        var users = await LoadUsers(SearchQuery, units, roles);
        Users = new ObservableCollection<UserFullDto>(_policy.Users(users.ToList()));
    }

    private async Task<IReadOnlyList<UserFullDto>> LoadUsers(string? userName = null, List<int>? units = null, List<int>? roles = null)
    {
        return await _userService.GetAllAsync(userName, units, roles);
    }

    private async Task LoadRoles()
    {
        var roles = await _dictionariesService.GetRolesAsync();
        Roles = new MultiSelectViewModel<RoleReadDto>(
            _policy.Roles(roles.ToList()),
            r => r.Name,
            "Должность",
            selectAllByDefault: true);
    }

    [RelayCommand]
    private void OpenUser(Guid? userId)
    {
        var user = Users.FirstOrDefault(u => u.UserId == userId);
        if (user is not null)
        {
            SelectedUser = user;
        }
    }

    private async Task LoadUnits()
    {
        var units = await _dictionariesService.GetUnitsAsync();
        Units = new MultiSelectViewModel<UnitReadDto>(
            _policy.Units(units.ToList()),
            u => u.Name,
            "Отдел",
            selectAllByDefault: true);
    }

    [RelayCommand]
    private void TogglePane()
    {
        IsPaneOpen = !IsPaneOpen;
        SelectedUser = null;
        UserEditViewModel.BeginCreateUser();
    }

    private async Task BeginEditUser(UserFullDto user)
    {
        IsPaneOpen = true;
        await UserEditViewModel.BeginEditUser(user);
    }

    private async void OnEditorSaved(object? sender, EventArgs e)
    {
        await LoadData();
    }
}
