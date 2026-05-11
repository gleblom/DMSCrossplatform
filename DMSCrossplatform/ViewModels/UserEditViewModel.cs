using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Infrastructure.Policy;
using DMSCrossplatform.Infrastructure.Validation;
using DMSCrossplatform.Models;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.Services;
using DMSCrossplatform.ViewModels.Custom;
using Microsoft.Extensions.Logging;

namespace DMSCrossplatform.ViewModels;

public partial class UserEditViewModel: ViewModelBase
{
     private readonly IUserService _userService;
     private readonly IAuthService _authService;
     private readonly ISessionService _sessionService;
     private readonly IDictionariesService _dictionariesService;
     private readonly ILogger<UserEditViewModel> _log;
     private readonly IPolicy _policy;

     [ObservableProperty] private string? _successMessage;
     [ObservableProperty] private string _errorMessage;
     [ObservableProperty] private UserFullDto _user;
     [ObservableProperty] private string _password = string.Empty;
     [ObservableProperty] private string _role = string.Empty;
     [ObservableProperty] private string _unit = string.Empty;

     [ObservableProperty] private bool _canSeeUnits;
     [ObservableProperty] private bool _isRoleEditing;
     [ObservableProperty] private bool _isUnitEditing;
     [ObservableProperty] private bool _isUserEdit;
     [ObservableProperty] private bool _isBusy;
     [ObservableProperty] private bool _passwordEnabled;

     [ObservableProperty] private RoleReadDto _selectedRole;
     [ObservableProperty] private UnitReadDto _selectedUnit;

     [ObservableProperty] private MultiSelectViewModel<SimpleDto> _categories;
     [ObservableProperty] private ObservableCollection<UnitReadDto> _units;
     [ObservableProperty] private ObservableCollection<RoleCategoryReadDto> _roleCategories;
     [ObservableProperty] private ObservableCollection<RoleReadDto> _roles;


     public UserEditViewModel(
         ILogger<UserEditViewModel> log,
         IDictionariesService dictionariesService,
         ISessionService sessionService, 
         IAuthService authService, 
         IUserService userService, IPolicyFactory factory)
     {
         _log = log;
         _dictionariesService = dictionariesService;
         _sessionService = sessionService;
         _authService = authService;
         _userService = userService;
         
         _policy = factory.CreatePolicy();
         
         CanSeeUnits = _policy.CanSeeUnits;
         
         _ = LoadData();
         
     }

     [RelayCommand]
     private void AddRole()
     {
         SelectedRole = new RoleReadDto();
         IsRoleEditing = false;
     }

     [RelayCommand]
     private async Task EditRole()
     {
         if (SelectedRole != null)
         {
             IsRoleEditing = true;
             await LoadRoleCategories(SelectedRole.Id);
             Role = SelectedRole.Name;
         }
         
     }

     [RelayCommand]
     private void AddUnit()
     {
         SelectedUnit = new UnitReadDto();
         IsUnitEditing = false;
     }

     [RelayCommand]
     private void EditUnit()
     {
         if (SelectedUnit != null)
         {
             IsUnitEditing = true;
             
         }
     }

     [RelayCommand]
     private async Task SaveUnit()
     {

         var err =
             Validators.ValidateEntityName(Unit, "Должность");
         if (err is not null) { ErrorMessage = err; return; }

         try
         {
             IsBusy = true;

             UnitReadDto unit;
             switch (IsUnitEditing)
             {
                     
                 case false:
                     unit = await _dictionariesService.CreateUnitAsync(new UnitCreateDto()
                     {
                         CompanyIds = [_sessionService.CurrentUser.CompanyId],
                         Name = Role
                     });
                     
                     SuccessMessage = "Отдел создан";
                     break;
                 case true:
                     unit =  await _dictionariesService.UpdateUnitAsync(SelectedUnit.Id, new UnitUpdateDto()
                     {
                         Name = Role
                     });
                     
                     SuccessMessage = "Отдел обновлен";
                     break;
             }
         }
         catch (Exception ex)
         {
             _log.LogWarning(ex, "Unit creation failed {ExMessage}", ex.Message);
             ErrorMessage = "Ошибка при обработке отдела";
         }
         finally
         {
             IsBusy =  false;
         }
     }

     private async Task LoadData()
     {
         try
         {
             var units = LoadUnits();
             var categories = LoadCategories();
             var roles = LoadRoles();

             await Task.WhenAll(units, categories, roles);
             
             Units = new ObservableCollection<UnitReadDto>(_policy.Units(units.Result.ToList()));
             Categories = new MultiSelectViewModel<SimpleDto>(categories.Result, c => c.Name, "Тип документа",
                 selectAllByDefault: false);
             Roles = new ObservableCollection<RoleReadDto>(_policy.Roles(roles.Result.ToList()));
         }
         catch (Exception ex)
         {
             _log.LogError(ex, "Unexpected error");
         }
         
     }

     public async Task<IReadOnlyList<RoleCategoryReadDto>> LoadRoleCategories(int? id)
     {
         var rc =  await _dictionariesService.GetRoleCategoriesAsync(id);
         RoleCategories =  new ObservableCollection<RoleCategoryReadDto>(rc);

         foreach (var t in Categories.Items)
         {
             foreach (var t1 in rc)
             {
                 if (t.Item.Id == t1.CategoryId)
                 {
                     t.IsSelected = true;
                 }
             }
         }
         return rc;
     }

     private async Task<IReadOnlyList<UnitReadDto>> LoadUnits()
     { 
         var units = await _dictionariesService.GetUnitsAsync();
         return units;
     }

     private async Task<IReadOnlyList<RoleReadDto>> LoadRoles()
     {
         var roles = await _dictionariesService.GetRolesAsync();
         return roles;
     }

     private async Task<IReadOnlyCollection<SimpleDto>> LoadCategories()
     {
         var categories = await _dictionariesService.GetCategoriesAsync();
         return categories;
     }

     [RelayCommand]
     public async Task SaveRole()
     {
         SuccessMessage = null;
         ErrorMessage = null;
         var categories = Categories?
             .Items
             .Where(i => i.IsSelected)
             .Select(c => new SimpleDto()
             {
                 Id = c.Item.Id
             })
             .Select(c => c.Id)
             .ToList();
         var err =
             Validators.ValidateEntityName(Role, "Должность") ??
             (categories.Count == 0 ? "Укажите хотя бы 1 категорию": null);
         if (err is not null) { ErrorMessage = err; return; }

         try
         {
             IsBusy = true;

             RoleReadDto role;
             switch (IsRoleEditing)
             {
                     
                 case false:
                     role = await _dictionariesService.CreateRoleAsync(new RoleCreateDto()
                     {
                         Name = Role
                     });

                     await _dictionariesService.AddRoleCategoryAsync(new RoleCategoryDto()
                     {
                         RoleId = role.Id,
                         CategoryIds = categories
                     });
                     SuccessMessage = "Роль создана";
                     break;
                 case true:
                     role =  await _dictionariesService.UpdateRoleAsync(SelectedRole.Id, new RoleUpdateDto()
                     {
                         Name = Role
                     });

                     await _dictionariesService.UpdateRoleCategoryAsync(new RoleCategoryDto()
                     {
                         RoleId = role.Id,
                         CategoryIds = categories
                     });
                     SuccessMessage = "Роль обновлена";
                     break;
             }
         }
         catch (Exception ex)
         {
             _log.LogWarning(ex, "Role creation failed {ExMessage}", ex.Message);
             ErrorMessage = "Ошибка при обработке роли";
         }
         finally
         {
             IsBusy =  false;
         }
     }
     
     

     [RelayCommand]
     public async Task SaveUser()
     {
         SuccessMessage = null;
         ErrorMessage = null;

         var err =
             Validators.ValidateEntityName(SelectedRole.Name, "Должность") ??
             Validators.ValidateEntityName(SelectedUnit.Name, "Отдел") ??
             Validators.ValidateName(User.FirstName, "Имя") ??
             Validators.ValidateName(User.SecondName, "Фамилия") ??
             Validators.ValidateName(User.ThirdName, "Отчество") ??
             Validators.ValidateEmail(User.Email) ??
             Validators.ValidatePhone(User.Phone) ??
             (IsUserEdit ? Validators.ValidatePassword(Password) : null);
         if (err is not null) { ErrorMessage = err; return; }

         try
         {
             IsBusy = true;

             switch (IsUserEdit)
             {
                 case false:
                     var user = _authService.RegisterAsync(new UserCreateDto
                     {
                         Email = User.Email,
                         Password = Password,
                         Phone = User.Phone
                     });

                     await Task.WhenAll(user);

                     await _userService.UpdateProfileAsync(new ProfileDto
                     {
                         CompanyId = _sessionService.CurrentUser.CompanyId,
                         Id = user.Result.Id,
                         FirstName = User.FirstName,
                         SecondName = User.SecondName,
                         ThirdName = User.SecondName,
                         RoleId = SelectedRole.Id,
                         UnitId = SelectedUnit?.Id
                     });

                     SuccessMessage = "Новый сотрудник успешно добавлен";
                     break;
                 case true:
                     await _userService.UpdateProfileAsync(new ProfileDto()
                     {
                         FirstName = User.FirstName,
                         SecondName = User.SecondName,
                         ThirdName = User.ThirdName,
                         CompanyId = _sessionService.CurrentUser.CompanyId,
                         Id = User.UserId,
                         RoleId = SelectedRole.Id,
                         UnitId = SelectedUnit.Id
                     });
                     SuccessMessage = "Профиль сотрудника успешно обновлен";
                     break;
             }

         }
         catch (ApiException ex)
         {
             _log.LogWarning(ex, "User creation failed {ExMessage}", ex.Message);
             ErrorMessage = "Ошибка при работе с пользователем";
         }
         catch (Exception ex)
         {
             _log.LogWarning(ex, "Unexepected error {ExMessage}", ex.Message);
             ErrorMessage = "Ошибка при работе с пользователем";
         }
         finally
         {
             IsBusy = false;
         }
     }
}