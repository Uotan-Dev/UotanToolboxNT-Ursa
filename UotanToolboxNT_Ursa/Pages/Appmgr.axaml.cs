using Avalonia.Controls;
using UotanToolboxNT_Ursa.ViewModels;

namespace UotanToolboxNT_Ursa.Pages;

public partial class Appmgr : UserControl
{
    public Appmgr()
    {
        InitializeComponent();
        DataContext = new AppmgrViewModel();
    }
}