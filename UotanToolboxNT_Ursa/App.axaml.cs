using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using UotanToolboxNT_Ursa.Models;
using UotanToolboxNT_Ursa.ViewModels;
using UotanToolboxNT_Ursa.Views;

namespace UotanToolboxNT_Ursa;

public partial class App : Application
{
    private class LogSink : ILogSink
    {
        public bool IsEnabled(LogEventLevel level, string area) => level >= LogEventLevel.Warning;

        public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
        {
            GlobalLogModel.AddLog($"[Avalonia {level}] [{area}] {messageTemplate}", MapLevel(level));
        }

        public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
        {
            try
            {
                var msg = string.Format(messageTemplate, propertyValues);
                GlobalLogModel.AddLog($"[Avalonia {level}] [{area}] {msg}", MapLevel(level));
            }
            catch
            {
                GlobalLogModel.AddLog($"[Avalonia {level}] [{area}] {messageTemplate}", MapLevel(level));
            }
        }

        private static GlobalLogModel.LogLevel MapLevel(LogEventLevel level) => level switch
        {
            LogEventLevel.Error or LogEventLevel.Fatal => GlobalLogModel.LogLevel.Error,
            LogEventLevel.Warning => GlobalLogModel.LogLevel.Warning,
            LogEventLevel.Information => GlobalLogModel.LogLevel.Info,
            _ => GlobalLogModel.LogLevel.Debug
        };
    }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        // 设置全局异常捕获
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            GlobalLogModel.AddLog($"Unhandled Exception: {e.ExceptionObject}", GlobalLogModel.LogLevel.Error);
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            GlobalLogModel.AddLog($"Unobserved Task Exception: {e.Exception}", GlobalLogModel.LogLevel.Error);
            e.SetObserved();
        };

        // 注册 Avalonia 日志接收器，捕获绑定错误等内部信息
        Logger.Sink = new LogSink();

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
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MvvmSplashWindow
            {
                DataContext = new SplashWindowViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            DisableAvaloniaDataAnnotationValidation();
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
        // 移除数据注解校验
        var dataAnnotationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataAnnotationPluginsToRemove)
        {
            _ = BindingPlugins.DataValidators.Remove(plugin);
        }

        var exceptionPluginsToRemove =
            BindingPlugins.DataValidators.OfType<ExceptionValidationPlugin>().ToArray();

        foreach (var plugin in exceptionPluginsToRemove)
        {
            _ = BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}