using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.Services;
using Microsoft.Extensions.Logging;

namespace DMSCrossplatform.ViewModels;

public partial class CompanyCreateViewModel: ViewModelBase
{
    private readonly IUserService _userService;
    private readonly ICompanyService _companyService;
    private readonly ILogger<RegisterViewModel> _log;
    private readonly ISessionService _sessionService;
    private readonly ShellHost _shellHost;
    
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _companyName = string.Empty;
    
    
    public CompanyCreateViewModel(
        ICompanyService companyService, 
        IUserService userService, 
        ShellHost shellHost, 
        ILogger<RegisterViewModel> log,
        ISessionService sessionService)
    {
        _companyService = companyService;
        _userService = userService;
        _shellHost = shellHost;
        _log = log;
        _sessionService = sessionService;
    }

    [RelayCommand]
    private async Task CreateCompany()
    {
        ErrorMessage = null;
        if (CompanyName == string.Empty)
        {
            ErrorMessage = "Введите название организации";
        }
        else
        {
            try
            {
                IsBusy = true;
                var company = await _companyService.CreateAsync(new CompanyCreateDto
                {
                    DirectorId = _sessionService.CurrentUser.UserId,
                    Name = CompanyName
                });
                
                
                await _userService.CreateProfileAsync(new ProfileDto
                {
                    Id = _sessionService.CurrentUser.UserId,
                    FirstName = _sessionService.CurrentUser.FirstName,
                    SecondName = _sessionService.CurrentUser.SecondName,
                    ThirdName = _sessionService.CurrentUser.ThirdName,
                    CompanyId = company.Id,
                    RoleId = _sessionService.CurrentUser.RoleId
                });
                
                _shellHost.ShowMenu();

            }
            catch (ApiException ex)
            {
                _log.LogWarning(ex, "Profile creation failed");
                ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Login unexpected error");
                ErrorMessage = "Не удалось подключиться к серверу.";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}