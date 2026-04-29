using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DMSCrossplatform.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DMSCrossplatform.Views;

public partial class PageControlView : ContentPage
{
    public PageControlView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<PageControlViewModel>();
    }
}