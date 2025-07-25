using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using UotanToolboxNT_Ursa.Models;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class GlobalLogViewModel : ObservableObject
{
    [ObservableProperty] public static string _logContent = "";
    public GlobalLogViewModel()
    {

    }
}