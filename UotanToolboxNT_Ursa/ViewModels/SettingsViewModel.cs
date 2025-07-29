using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using UotanToolboxNT_Ursa.Models;
using UotanToolboxNT_Ursa.Themes;

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
    //这里是属性变化的处理方法，每添加一个属性都需要添加一个对应的处理方法
    partial void OnIsLightThemeChanged(bool value)
    {
        GlobalLogModel.AddLog($"主题切换为 {(value ? "浅色" : "深色")} 模式", GlobalLogModel.LogLevel.Info);
        string themeKey = IsLightTheme ? "LightColors" : "DarkColors";
        ThemeLoader.ApplyTheme(themeKey);
        SaveSettings();
    }

    partial void OnSelectedLanguageListChanged(string value)
    {
        GlobalLogModel.AddLog($"尝试切换语言为 {value}", GlobalLogModel.LogLevel.Info);
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