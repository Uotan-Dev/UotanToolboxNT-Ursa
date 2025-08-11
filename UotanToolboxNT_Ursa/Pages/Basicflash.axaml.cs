using Avalonia.Controls;
using UotanToolboxNT_Ursa.ViewModels;

namespace UotanToolboxNT_Ursa.Pages;

public partial class Basicflash : UserControl
{
    public Basicflash()
    {
        InitializeComponent();
        DataContext = new BasicflashViewModel();
    }
}