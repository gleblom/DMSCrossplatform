namespace DMSCrossplatform.ViewModels;

public abstract class ViewState : ViewModelBase
{
    private object? _currentViewModel;

    public object? CurrentViewModel
    {
        get => _currentViewModel;
        set
        {
            _currentViewModel = value;
            OnPropertyChanged();
        }
    }
}