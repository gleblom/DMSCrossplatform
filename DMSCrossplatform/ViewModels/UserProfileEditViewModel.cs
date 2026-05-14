using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Infrastructure.Policy;
using DMSCrossplatform.Infrastructure.Validation;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.Services;
using Microsoft.Extensions.Logging;

namespace DMSCrossplatform.ViewModels;

public partial class UserProfileEditViewModel : ViewModelBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly ISessionService _sessionService;
    private readonly IDictionariesService _dictionariesService;
    private readonly ILogger<UserProfileEditViewModel> _log;
    private readonly IPolicy _policy;

    private IUserProfileEditState _state;

    [ObservableProperty] private string? _successMessage;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private UserFullDto _user = new();
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private bool _canSeeUnits;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private RoleReadDto? _selectedRole;
    [ObservableProperty] private UnitReadDto? _selectedUnit;
    [ObservableProperty] private ObservableCollection<UnitReadDto> _units = new();
    [ObservableProperty] private ObservableCollection<RoleReadDto> _roles = new();

    public UserProfileEditViewModel(
        ILogger<UserProfileEditViewModel> log,
        IDictionariesService dictionariesService,
        ISessionService sessionService,
        IAuthService authService,
        IUserService userService,
        IPolicyFactory factory)
    {
        _log = log;
        _dictionariesService = dictionariesService;
        _sessionService = sessionService;
        _authService = authService;
        _userService = userService;
        _policy = factory.CreatePolicy();
        CanSeeUnits = _policy.CanSeeUnits;
        _state = new CreateUserState();
        ApplyState();
        _ = LoadDictionaries();
    }

    public event EventHandler? Saved;

    public void BeginCreate()
    {
        User = new UserFullDto();
        Password = string.Empty;
        SelectedRole = null;
        SelectedUnit = null;
        _state = new CreateUserState();
        ApplyState();
        ClearMessages();
    }

    public async Task BeginEdit(UserFullDto user)
    {
        User = user;
        Password = string.Empty;
        await LoadDictionaries();
        SelectedRole = Roles.FirstOrDefault(r => r.Id == user.RoleId);
        SelectedUnit = Units.FirstOrDefault(u => u.Id == user.UnitId);
        _state = new UpdateUserState();
        ApplyState();
        ClearMessages();
    }

    public async Task LoadDictionaries()
    {
        try
        {
            var unitsTask = _dictionariesService.GetUnitsAsync();
            var rolesTask = _dictionariesService.GetRolesAsync();
            await Task.WhenAll(unitsTask, rolesTask);

            Roles = new ObservableCollection<RoleReadDto>(_policy.Roles(rolesTask.Result.ToList()));
            Units = new ObservableCollection<UnitReadDto>(_policy.Units(unitsTask.Result.ToList()));
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to load user dictionaries");
            ErrorMessage = "Ошибка при загрузке справочников пользователя";
        }
    }

    [RelayCommand]
    public async Task Save()
    {
        ClearMessages();

        var err = Validate();
        if (err is not null)
        {
            ErrorMessage = err;
            return;
        }

        try
        {
            IsBusy = true;
            SuccessMessage = await _state.Save(this);
            Saved?.Invoke(this, EventArgs.Empty);
        }
        catch (ApiException ex)
        {
            _log.LogWarning(ex, "User saving failed {ExMessage}", ex.Message);
            ErrorMessage = "Ошибка при работе с пользователем";
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Unexpected user saving error {ExMessage}", ex.Message);
            ErrorMessage = "Ошибка при работе с пользователем";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private string? Validate()
    {
        if (SelectedRole is null)
        {
            return "Укажите должность";
        }

        if (_policy is not DirectorPolicy && SelectedUnit is null)
        {
            return "Укажите отдел";
        }

        return Validators.ValidateName(User.FirstName, "Имя") ??
               Validators.ValidateName(User.SecondName, "Фамилия") ??
               Validators.ValidateName(User.ThirdName, "Отчество") ??
               Validators.ValidateEmail(User.Email) ??
               Validators.ValidatePhone(User.Phone) ??
               _state.ValidatePassword(Password);
    }

    private void ApplyState()
    {
        Title = _state.Title;
    }

    private void ClearMessages()
    {
        SuccessMessage = null;
        ErrorMessage = null;
    }

    private interface IUserProfileEditState
    {
        string Title { get; }
        string? ValidatePassword(string password);
        Task<string> Save(UserProfileEditViewModel vm);
    }

    private sealed class CreateUserState : IUserProfileEditState
    {
        public string Title => "Добавление пользователя";

        public string? ValidatePassword(string password)
        {
            return Validators.ValidatePassword(password);
        }

        public async Task<string> Save(UserProfileEditViewModel vm)
        {
            var user = await vm._authService.RegisterAsync(new UserCreateDto
            {
                Email = vm.User.Email ?? string.Empty,
                Password = vm.Password,
                Phone = vm.User.Phone
            });

            await vm._userService.UpdateProfileAsync(new ProfileDto
            {
                CompanyId = vm._sessionService.CurrentUser?.CompanyId,
                Id = user.Id,
                FirstName = vm.User.FirstName,
                SecondName = vm.User.SecondName,
                ThirdName = vm.User.ThirdName,
                RoleId = vm.SelectedRole!.Id,
                UnitId = vm.SelectedUnit?.Id
            });

            return "Новый сотрудник успешно добавлен";
        }
    }

    private sealed class UpdateUserState : IUserProfileEditState
    {
        public string Title => "Редактирование пользователя";

        public string? ValidatePassword(string password)
        {
            return null;
        }

        public async Task<string> Save(UserProfileEditViewModel vm)
        {
            var unitId = vm._policy is not DirectorPolicy ? vm.SelectedUnit?.Id : null;

            await vm._userService.UpdateProfileAsync(new ProfileDto
            {
                FirstName = vm.User.FirstName,
                SecondName = vm.User.SecondName,
                ThirdName = vm.User.ThirdName,
                CompanyId = vm._sessionService.CurrentUser?.CompanyId,
                Id = vm.User.UserId,
                RoleId = vm.SelectedRole!.Id,
                UnitId = unitId
            });

            return "Профиль сотрудника успешно обновлен";
        }
    }
}
