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
using DMSCrossplatform.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DMSCrossplatform.ViewModels;

public partial class UserListViewModel: ViewModelBase
{
    private readonly IUserService _userService;
    private readonly IDictionariesService _dictionariesService;
    private readonly IPolicy _policy;
    
    [ObservableProperty] private UserEditViewModel _userEditViewModel = App.Services.GetRequiredService<UserEditViewModel>();
    
    [ObservableProperty] private string _searchQuery;
    
    [ObservableProperty] private MultiSelectViewModel<RoleReadDto> _roles;
    [ObservableProperty] private MultiSelectViewModel<UnitReadDto> _units;

    [ObservableProperty] private bool _isPaneOpen;
    [ObservableProperty] private bool _canSeeUnits;
    
    public Action<object> OpenEditor { get; set; }

    private UserFullDto _selectedUser;

    public UserFullDto SelectedUser
    {
        get => _selectedUser;
        set
        {
            SetProperty(ref _selectedUser, value);
            if (SelectedUser != null)
            {
                IsPaneOpen = true;
                UserEditViewModel.SelectedRole = UserEditViewModel.Roles.FirstOrDefault(r => r.Id == SelectedUser.RoleId);
                UserEditViewModel.SelectedUnit = UserEditViewModel.Units.FirstOrDefault(u => u.Id == SelectedUser.UnitId);
                UserEditViewModel.User = value;
                UserEditViewModel.IsUserEdit = true;
                UserEditViewModel.Unit = UserEditViewModel.SelectedUnit.Name;
                UserEditViewModel.Role = UserEditViewModel.SelectedRole.Name;
                UserEditViewModel.LoadRoleCategories(SelectedUser.RoleId);
            }
        }
    }

    
    [ObservableProperty] private ObservableCollection<UserFullDto> _users;
    
    public UserListViewModel(
        IDictionariesService dictionariesService,
        IUserService userService, IPolicyFactory factory)
    {
        _dictionariesService = dictionariesService;
        _userService = userService;
        
        _policy = factory.CreatePolicy();

        CanSeeUnits = _policy.CanSeeUnits;

        _ = LoadData();

    }

    [RelayCommand]
    private void RefreshUsers()
    {
        _ =  LoadData();
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
        var units = _policy.CanSeeUnits ? Units?
            .Items
            .Where(i => i.IsSelected)
            .Select(c => new UnitReadDto()
            {
                Id = c.Item.Id
            })
            .Select(c => c.Id)
            .ToList(): null;
        var roles = Roles?
            .Items
            .Where(i => i.IsSelected)
            .Select(c => new RoleReadDto()
            {
                Id = c.Item.Id
            })
            .Select(c => c.Id)
            .ToList();
        var users = await LoadUsers(SearchQuery,units, roles);
        Users = new ObservableCollection<UserFullDto>(_policy.Users(users.ToList()));
    }

    private async Task<IReadOnlyList<UserFullDto>> LoadUsers(string? userName = null, List<int>? units = null, List<int>? roles = null)
    {
        return await _userService.GetAllAsync(userName, units, roles);
    }

    private async Task LoadRoles()
    {
        var roles = await _dictionariesService.GetRolesAsync();
        Roles = new MultiSelectViewModel<RoleReadDto>
        (
            _policy.Roles(roles.ToList()),
            r => r.Name, "Должность",
            selectAllByDefault: true
            );
    }

    private async Task LoadUnits()
    {
        var units = await _dictionariesService.GetUnitsAsync();
        Units = new MultiSelectViewModel<UnitReadDto>(units, u => u.Name, "Отдел", selectAllByDefault: true);
    }


    [RelayCommand]
    private void TogglePane()
    {
        IsPaneOpen = !IsPaneOpen;
        UserEditViewModel.IsUserEdit = false;
        SelectedUser = null;
        UserEditViewModel.User = new UserFullDto();
        UserEditViewModel.SelectedRole = null;
        UserEditViewModel.SelectedUnit = null;
    }
    
}