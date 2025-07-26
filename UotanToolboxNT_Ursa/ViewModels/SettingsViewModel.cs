using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using UotanToolboxNT_Ursa.Models;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLightTheme;

    public AvaloniaList<string> LanguageList { get; } = ["Settings_Default", "English", "简体中文"];

    [ObservableProperty]
    private string _selectedLanguageList;

    public SettingsViewModel()
    {
        // 加载设置
        var settings = SettingsModel.Load();
        _isLightTheme = settings.IsLightTheme;
        _selectedLanguageList = settings.SelectedLanguageList;
    }

    partial void OnIsLightThemeChanged(bool value)
    {
        SaveSettings();
    }

    partial void OnSelectedLanguageListChanged(string value)
    {
        SaveSettings();
    }

    private void SaveSettings()
    {
        var settings = new SettingsModel
        {
            IsLightTheme = IsLightTheme,
            SelectedLanguageList = SelectedLanguageList
        };
        SettingsModel.Save(settings);
    }
}