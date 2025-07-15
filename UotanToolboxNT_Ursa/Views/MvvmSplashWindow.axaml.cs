using System.Threading.Tasks;
using Avalonia.Controls;
using UotanToolboxNT_Ursa.ViewModels;
using Ursa.Controls;

namespace UotanToolboxNT_Ursa.Views;

public partial class MvvmSplashWindow : SplashWindow
{
    public MvvmSplashWindow()
    {
        InitializeComponent();
    }

    protected override async Task<Window?> CreateNextWindow()
    {
        if (this.DialogResult is true)
        {
            return new MainWindow()
            {
                DataContext = new MainWindowViewModel(),
                Width = 1100,
                Height = 800
            };
        }
        return null;
    }
}