using CommunityToolkit.Mvvm.ComponentModel;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class GlobalLogViewModel : ObservableObject
{
    [ObservableProperty] public static string _logContent = "";
    public GlobalLogViewModel()
    {

    }
}