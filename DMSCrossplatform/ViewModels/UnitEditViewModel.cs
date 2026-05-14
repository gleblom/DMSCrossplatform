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
using Microsoft.Extensions.Logging;

namespace DMSCrossplatform.ViewModels;

public partial class UnitEditViewModel : ViewModelBase
{
    private readonly IDictionariesService _dictionariesService;
    private readonly ISessionService _sessionService;
    private readonly ILogger<UnitEditViewModel> _log;
    private IUnitEditState _state;

    [ObservableProperty] private string? _successMessage;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _canSeeUnits;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _unit = string.Empty;
    [ObservableProperty] private UnitReadDto? _selectedUnit;
    [ObservableProperty] private ObservableCollection<UnitReadDto> _units = new();

    public UnitEditViewModel(
        ILogger<UnitEditViewModel> log,
        IDictionariesService dictionariesService,
        ISessionService sessionService,
        IPolicyFactory factory)
    {
        _log = log;
        _dictionariesService = dictionariesService;
        _sessionService = sessionService;
        CanSeeUnits = factory.CreatePolicy().CanSeeUnits;
        _state = new CreateUnitState();
        _ = LoadDictionaries();
    }

    public event EventHandler? Saved;

    public async Task LoadDictionaries()
    {
        try
        {
            var units = await _dictionariesService.GetUnitsAsync();
            Units = new ObservableCollection<UnitReadDto>(units);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to load unit dictionaries");
            ErrorMessage = "Ошибка при загрузке справочников отделов";
        }
    }

    [RelayCommand]
    private void Add()
    {
        SelectedUnit = new UnitReadDto();
        Unit = string.Empty;
        _state = new CreateUnitState();
        ClearMessages();
    }

    [RelayCommand]
    private void Edit()
    {
        if (SelectedUnit is null)
        {
            return;
        }

        Unit = SelectedUnit.Name;
        _state = new UpdateUnitState(SelectedUnit.Id);
        ClearMessages();
    }

    [RelayCommand]
    private async Task Save()
    {
        ClearMessages();
        var err = Validators.ValidateEntityName(Unit, "Отдел");
        if (err is not null)
        {
            ErrorMessage = err;
            return;
        }

        try
        {
            IsBusy = true;
            SuccessMessage = await _state.Save(this);
            await LoadDictionaries();
            Saved?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Unit saving failed {ExMessage}", ex.Message);
            ErrorMessage = "Ошибка при обработке отдела";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearMessages()
    {
        SuccessMessage = null;
        ErrorMessage = null;
    }

    private interface IUnitEditState
    {
        Task<string> Save(UnitEditViewModel vm);
    }

    private sealed class CreateUnitState : IUnitEditState
    {
        public async Task<string> Save(UnitEditViewModel vm)
        {
            await vm._dictionariesService.CreateUnitAsync(new UnitCreateDto
            {
                CompanyIds = [vm._sessionService.CurrentUser?.CompanyId],
                Name = vm.Unit
            });

            return "Отдел создан";
        }
    }

    private sealed class UpdateUnitState : IUnitEditState
    {
        private readonly int _unitId;

        public UpdateUnitState(int unitId)
        {
            _unitId = unitId;
        }

        public async Task<string> Save(UnitEditViewModel vm)
        {
            await vm._dictionariesService.UpdateUnitAsync(_unitId, new UnitUpdateDto
            {
                Name = vm.Unit
            });

            return "Отдел обновлен";
        }
    }
}
