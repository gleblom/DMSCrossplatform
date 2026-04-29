using CommunityToolkit.Mvvm.ComponentModel;

namespace DMSCrossplatform.Models;

public class SelectableItem<T> : ObservableObject
{
    private T _item;
    public T Item
    {
        get => _item;
        set => SetProperty(ref _item, value);
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    private string _displayText = string.Empty;
    public string DisplayText
    {
        get => _displayText;
        set => SetProperty(ref _displayText, value);
    }

    public SelectableItem(T item, string displayText)
    {
        Item = item;
        DisplayText = displayText;
    }
}