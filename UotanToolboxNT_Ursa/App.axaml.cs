using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using UotanToolboxNT_Ursa.Models;
using UotanToolboxNT_Ursa.ViewModels;
using UotanToolboxNT_Ursa.Views;

namespace UotanToolboxNT_Ursa;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        // 加载并应用设置
        try
        {
            var settings = SettingsModel.Load();
            SettingsModel.ChangeLaguage(settings.SelectedLanguageList);
            SettingsModel.ChangeTheme(settings.IsLightTheme ? "LightColors" : "DarkColors");
        }
        catch (Exception ex)
        {
            // 记录错误但继续启动，避免黑屏
            GlobalLogModel.AddLog($"Settings application failed on startup: {ex.Message}", GlobalLogModel.LogLevel.Error);
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            var vm = new MainWindowViewModel();
            singleViewPlatform.MainView = new MainView
            {
                DataContext = vm.MainViewViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            _ = BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}