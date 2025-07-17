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
        DataContext = new SplashWindowViewModel();
    }

    protected override Task<Window?> CreateNextWindow()
    {
        return Task.FromResult(DialogResult is true
            ? new MainWindow()
            {
                DataContext = new MainWindowViewModel(),
                Width = 1197,
                Height = 825
            }
            : (Window?)null);
    }
}