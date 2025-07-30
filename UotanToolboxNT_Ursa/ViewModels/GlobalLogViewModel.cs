using CommunityToolkit.Mvvm.ComponentModel;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class GlobalLogViewModel : ObservableObject
{
    [ObservableProperty] public static string _logContent = "";
    [ObservableProperty] public static string _logContent1 = "";
    [ObservableProperty] public static string _logContent2 = "";
    [ObservableProperty] public static string _logContent3 = "";
    public GlobalLogViewModel()
    {

    }
}