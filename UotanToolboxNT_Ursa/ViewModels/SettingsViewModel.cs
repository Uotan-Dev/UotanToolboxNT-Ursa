using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private bool _isLightTheme;
    public AvaloniaList<string> LanguageList { get; } = ["Settings_Default", "English", "¼òÌåÖÐÎÄ"];
    [ObservableProperty] private string _selectedLanguageList;
    public SettingsViewModel()
    {

    }
}