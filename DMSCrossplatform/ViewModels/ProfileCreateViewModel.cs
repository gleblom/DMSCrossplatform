using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Infrastructure.Validation;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.Services;
using Microsoft.Extensions.Logging;

namespace DMSCrossplatform.ViewModels;

public partial class ProfileCreateViewModel: ViewModelBase
{
    private readonly INavigationService<StartupRegionState> _nav;
    private readonly ILogger<RegisterViewModel> _log;
    private readonly IUserService _userService;
    private readonly ISessionService _sessionService;
    private readonly IAuthService _auth;
    
    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _secondName = string.Empty;
    [ObservableProperty] private string _thirdName = string.Empty;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isBusy;

    public ProfileCreateViewModel(
        INavigationService<StartupRegionState> nav, 
        ILogger<RegisterViewModel> log, IUserService userService, 
        ISessionService sessionService,
        IAuthService auth)
    {
        _nav = nav;
        _log = log;
        _userService = userService;
        _sessionService = sessionService;
        _auth = auth;
    }

    [RelayCommand]
    private async Task CreateProfile()
    {
        ErrorMessage = null;
        var err =
            Validators.ValidateName(FirstName, "Имя") ??
            Validators.ValidateName(SecondName, "Фамилия") ??
            Validators.ValidateName(ThirdName, "Отчество");
        if (err is not null) { ErrorMessage = err; return; }

        try
        {
            if (_sessionService.CurrentUser != null)
            {
                IsBusy = true;
                await _userService.CreateProfileAsync(new ProfileDto
                {
                    Id = _sessionService.CurrentUser.UserId,
                    FirstName = FirstName,
                    SecondName = SecondName,
                    ThirdName = SecondName,
                    RoleId = 1,
                    CompanyId = null,
                    UnitId = null,
                });
                _sessionService.CurrentUser = await _auth.GetMeAsync();
                _nav.NavigateTo<CompanyCreateViewModel>();
            }
        }
        catch (ApiException ex)
        {
            _log.LogWarning(ex, "Profile creation failed");
            ErrorMessage = "Неизвестная ошибка";
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Profile unexpected error");
            ErrorMessage = "Неизвестная ошибка";
        }
        finally
        {
            IsBusy = false;
        }
    }
}