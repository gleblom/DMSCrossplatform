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

public partial class RegisterViewModel: ViewModelBase
{
    private readonly IAuthService _auth;
    private readonly INavigationService<StartupRegionState> _nav;
    private readonly ILogger<RegisterViewModel> _log;


    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _successMessage;
    [ObservableProperty] private bool _isBusy;

    public RegisterViewModel(IAuthService auth, INavigationService<StartupRegionState>  nav, ILogger<RegisterViewModel> log)
    {
        _auth = auth;
        _nav = nav;
        _log = log;
    }



    [RelayCommand]
    private async Task RegisterAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        var err =
            Validators.ValidateEmail(Email) ??
            Validators.ValidatePhone(Phone) ??
            Validators.ValidatePassword(Password);
        if (err is not null) { ErrorMessage = err; return; }

        try
        {
            IsBusy = true;
            await _auth.RegisterAsync(new UserCreateDto
            {
                Email = Email, Password = Password, Phone = Phone
            });
            SuccessMessage = "На указанный email отправлено письмо для подтверждения.";
            
            

        }
        catch (ApiException ex)
        {
            _log.LogWarning(ex, "Registration failed");
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Registration unexpected error");
            ErrorMessage = "Не удалось подключиться к серверу.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel() => _nav.NavigateTo<LoginViewModel>();
}