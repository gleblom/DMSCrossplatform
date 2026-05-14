using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure.Policy;
using DMSCrossplatform.Infrastructure.Validation;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.Services;
using DMSCrossplatform.ViewModels.Custom;
using Microsoft.Extensions.Logging;

namespace DMSCrossplatform.ViewModels;

public partial class RoleEditViewModel : ViewModelBase
{
    private readonly IDictionariesService _dictionariesService;
    private readonly ILogger<RoleEditViewModel> _log;
    private readonly IPolicy _policy;
    private IRoleEditState _state;

    [ObservableProperty] private string? _successMessage;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _canSeeUnits;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _role = string.Empty;
    [ObservableProperty] private RoleReadDto? _selectedRole;
    [ObservableProperty] private ObservableCollection<RoleReadDto> _roles = new();
    [ObservableProperty] private MultiSelectViewModel<SimpleDto> _categories = new(Array.Empty<SimpleDto>(), c => c.Name, "Тип документа");

    public RoleEditViewModel(
        ILogger<RoleEditViewModel> log,
        IDictionariesService dictionariesService,
        IPolicyFactory factory)
    {
        _log = log;
        _dictionariesService = dictionariesService;
        _policy = factory.CreatePolicy();
        CanSeeUnits = _policy.CanSeeUnits;
        _state = new CreateRoleState();
        _ = LoadDictionaries();
    }

    public event EventHandler? Saved;

    public async Task LoadDictionaries()
    {
        try
        {
            var categoriesTask = _dictionariesService.GetCategoriesAsync();
            var rolesTask = _dictionariesService.GetRolesAsync();
            await Task.WhenAll(categoriesTask, rolesTask);

            Categories = new MultiSelectViewModel<SimpleDto>(categoriesTask.Result, c => c.Name, "Тип документа");
            Roles = new ObservableCollection<RoleReadDto>(_policy.Roles(rolesTask.Result.ToList()));
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to load role dictionaries");
            ErrorMessage = "Ошибка при загрузке справочников должностей";
        }
    }

    [RelayCommand]
    private void Add()
    {
        SelectedRole = new RoleReadDto();
        Role = string.Empty;
        _state = new CreateRoleState();
        ClearCategorySelection();
        ClearMessages();
    }

    [RelayCommand]
    private async Task Edit()
    {
        if (SelectedRole is null)
        {
            return;
        }

        Role = SelectedRole.Name;
        _state = new UpdateRoleState(SelectedRole.Id);
        await SelectRoleCategories(SelectedRole.Id);
        ClearMessages();
    }

    [RelayCommand]
    public async Task Save()
    {
        ClearMessages();
        var categoryIds = Categories.GetSelectedItems().Select(c => c.Id).ToList();
        var err = Validators.ValidateEntityName(Role, "Должность") ??
                  (categoryIds.Count == 0 ? "Укажите хотя бы 1 категорию" : null);

        if (err is not null)
        {
            ErrorMessage = err;
            return;
        }

        try
        {
            IsBusy = true;
            SuccessMessage = await _state.Save(this, categoryIds);
            await LoadDictionaries();
            Saved?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Role saving failed {ExMessage}", ex.Message);
            ErrorMessage = "Ошибка при обработке должности";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SelectRoleCategories(int roleId)
    {
        ClearCategorySelection();
        var selectedCategoryIds = (await _dictionariesService.GetRoleCategoriesAsync(roleId))
            .Select(c => c.CategoryId)
            .ToHashSet();

        foreach (var category in Categories.Items)
        {
            category.IsSelected = selectedCategoryIds.Contains(category.Item.Id);
        }
    }

    private void ClearCategorySelection()
    {
        foreach (var category in Categories.Items)
        {
            category.IsSelected = false;
        }
    }

    private void ClearMessages()
    {
        SuccessMessage = null;
        ErrorMessage = null;
    }

    private interface IRoleEditState
    {
        Task<string> Save(RoleEditViewModel vm, System.Collections.Generic.List<int> categoryIds);
    }

    private sealed class CreateRoleState : IRoleEditState
    {
        public async Task<string> Save(RoleEditViewModel vm, System.Collections.Generic.List<int> categoryIds)
        {
            var role = await vm._dictionariesService.CreateRoleAsync(new RoleCreateDto
            {
                Name = vm.Role
            });

            await vm._dictionariesService.AddRoleCategoryAsync(new RoleCategoryDto
            {
                RoleId = role.Id,
                CategoryIds = categoryIds
            });

            return "Должность создана";
        }
    }

    private sealed class UpdateRoleState : IRoleEditState
    {
        private readonly int _roleId;

        public UpdateRoleState(int roleId)
        {
            _roleId = roleId;
        }

        public async Task<string> Save(RoleEditViewModel vm, System.Collections.Generic.List<int> categoryIds)
        {
            var role = await vm._dictionariesService.UpdateRoleAsync(_roleId, new RoleUpdateDto
            {
                Name = vm.Role
            });

            await vm._dictionariesService.UpdateRoleCategoryAsync(new RoleCategoryDto
            {
                RoleId = role.Id,
                CategoryIds = categoryIds
            });

            return "Должность обновлена";
        }
    }
}
