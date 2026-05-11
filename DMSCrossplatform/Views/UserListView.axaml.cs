using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DMSCrossplatform.ViewModels;

namespace DMSCrossplatform.Views;

public partial class UserListView : UserControl
{
    public UserListView()
    {
        InitializeComponent();
    }


    private void DataGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (e.AddedItems.Count > 0 && DataContext is UserListViewModel { IsPaneOpen: false } vm) vm.IsPaneOpen = true;
        });
    }
}