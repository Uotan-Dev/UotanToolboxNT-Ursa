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
        var settings = SettingsModel.Load();
        _isLightTheme = settings.IsLightTheme;
        _selectedLanguageList = settings.SelectedLanguageList;
    }
    partial void OnIsLightThemeChanged(bool value)
    {
        GlobalLogModel.AddLog($"主题切换为 {(value ? "浅色" : "深色")}");
        string themeKey = IsLightTheme ? "LightColors" : "DarkColors";
        SaveSettings();
        SettingsModel.ChangeTheme(themeKey);
    }

    partial void OnSelectedLanguageListChanged(string value)
    {
        GlobalLogModel.AddLog($"语言切换为 {value}");
        SaveSettings();
        SettingsModel.ChangeLaguage(value);
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