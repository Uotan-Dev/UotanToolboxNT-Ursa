using Avalonia.Controls;
using Avalonia.Interactivity;
using UotanToolboxNT_Ursa.ViewModels;

namespace UotanToolboxNT_Ursa.Pages;

public partial class AboutUsDemo : UserControl
{
    public AboutUsDemo()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is AboutUsDemoViewModel vm)
        {
            var launcher = TopLevel.GetTopLevel(this)?.Launcher;
            vm.Launcher = launcher;
        }
    }
}