using System;
using System.IO;
using System.Threading.Tasks;
using AdvancedSharpAdbClient.Models;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Irihi.Avalonia.Shared.Contracts;
using UotanToolboxNT_Ursa.Models;
using static UotanToolboxNT_Ursa.Models.GlobalLogModel;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class SplashWindowViewModel : ObservableObject, IDialogContext
{
    [ObservableProperty] private double _progress;
    private readonly SplashWindowModel _model;

    public string StatusText
    {
        get => _model.StatusText;
        set
        {
            if (_model.StatusText != value)
            {
                _model.StatusText = value;
                OnPropertyChanged();
            }
        }
    }

    public SplashWindowViewModel()
    {
        _model = new SplashWindowModel();
        InitializeAsync();
    }

    /// <summary>
    /// 初始化应用程序
    /// </summary>
    private async void InitializeAsync()
    {
        try
        {
            UpdateProgress(05, "正在读取设置信息...");
            AddLog("正在读取设置信息...", LogLevel.Info);
            var settings = SettingsModel.Load();
            SettingsModel.ChangeLaguage(settings.SelectedLanguageList);
            SettingsModel.ChangeTheme(settings.IsLightTheme ? "LightColors" : "DarkColors");

        }
        catch (Exception ex)
        {
            AddLog($"读取设置信息时出错：{ex.Message}", LogLevel.Warning);
        }

        UpdateProgress(0, "正在检查运行组件...");
        AddLog("正在检查运行组件...", LogLevel.Info);
        await Task.Delay(100); // 让用户看到状态变化
        PerformCompentCheck();

        UpdateProgress(50, "正在枚举设备...");
        AddLog("正在枚举设备...", LogLevel.Info);
        await Task.Delay(100);
        await PerformHardwareCheckAsync();

        UpdateProgress(100, "初始化完成");
        AddLog("初始化完成", LogLevel.Info);

        // 启动设备管理器
        Global.DeviceManager.Start();
        AddLog("设备管理器已启动", LogLevel.Info);

        RequestClose?.Invoke(this, true);
    }

    /// <summary>
    /// 更新进度和状态文本
    /// </summary>
    private void UpdateProgress(double progress, string status)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Progress = progress;
            StatusText = status;
        });
    }

    /// <summary>
    /// 检查工具箱各组件是否可用
    /// </summary>
    private void PerformCompentCheck()
    {
        try
        {
            UpdateProgress(10, "检查ADB Server运行状态...");
            AddLog("检查ADB Server运行状态...", LogLevel.Info);

            if (!File.Exists(Path.Combine(Global.BinDirectory.FullName, "platform-tools", "adb.exe")))
            {
                UpdateProgress(25, "ADB Server未找到！");
                AddLog("ADB Server未找到！", LogLevel.Info);
                return;
            }

            UpdateProgress(30, "正在启动ADB Server...");
            var result = Global.AdbServer.StartServer(Path.Combine(Global.BinDirectory.FullName, "platform-tools", "adb.exe"), false);

            var statusMessage = result switch
            {
                StartServerResult.Started => "ADB Server启动完成",
                StartServerResult.AlreadyRunning => "ADB Server正在运行",
                StartServerResult.RestartedOutdatedDaemon => "ADB Server已过时，正在重新创建",
                StartServerResult.Starting => "ADB Server正在启动",
                _ => "未知的ADB Server状态"
            };

            UpdateProgress(40, statusMessage);
            AddLog(statusMessage);

        }
        catch (Exception ex)
        {
            UpdateProgress(40, "检查运行组件时出错！");
            AddLog($"检查运行组件时出错！{ex.Message}", LogLevel.Error);
        }
    }

    /// <summary>
    /// 检查用户详细硬件信息
    /// </summary>
    private async Task PerformHardwareCheckAsync()
    {
        UpdateProgress(55, "正在获取硬件信息...");
        AddLog("正在获取硬件信息...", LogLevel.Info);

        // 操作系统信息
        try
        {
            UpdateProgress(55, "正在获取操作系统信息...");
            await Task.Run(Global.HardwareInfo.RefreshOperatingSystem);
        }
        catch (Exception ex)
        {
            AddLog($"获取操作系统信息时出错：{ex.Message}", LogLevel.Warning);
        }

        // 内存状态
        try
        {
            UpdateProgress(60, "正在获取内存状态...");
            await Task.Run(Global.HardwareInfo.RefreshMemoryStatus);
        }
        catch (Exception ex)
        {
            AddLog($"获取内存状态时出错：{ex.Message}", LogLevel.Warning);
        }

        // BIOS信息
        try
        {
            UpdateProgress(65, "正在获取BIOS信息...");
            await Task.Run(Global.HardwareInfo.RefreshBIOSList);
        }
        catch (Exception ex)
        {
            AddLog($"获取BIOS信息时出错：{ex.Message}", LogLevel.Warning);
        }

        // CPU信息
        try
        {
            UpdateProgress(70, "正在获取设备信息...");
            await Task.Run(() => Global.HardwareInfo.RefreshCPUList(false, 500, true));
        }
        catch (Exception ex)
        {
            AddLog($"获取CPU信息时出错：{ex.Message}", LogLevel.Warning);
        }

        // 内存列表
        try
        {
            UpdateProgress(75, "正在获取内存列表...");
            await Task.Run(Global.HardwareInfo.RefreshMemoryList);
        }
        catch (Exception ex)
        {
            AddLog($"获取内存列表时出错：{ex.Message}", LogLevel.Warning);
        }

        // 网络适配器信息
        try
        {
            UpdateProgress(80, "正在获取网络适配器信息...");
            await Task.Run(() => Global.HardwareInfo.RefreshNetworkAdapterList(false, true));
        }
        catch (Exception ex)
        {
            AddLog($"获取网络适配器信息时出错：{ex.Message}", LogLevel.Warning);
        }

        // 声音设备信息
        try
        {
            UpdateProgress(85, "正在获取设备设备信息...");
            await Task.Run(Global.HardwareInfo.RefreshSoundDeviceList);
        }
        catch (Exception ex)
        {
            AddLog($"获取声音设备信息时出错：{ex.Message}", LogLevel.Warning);
        }

        // 显卡信息
        try
        {
            UpdateProgress(90, "正在获取设备信息...");
            await Task.Run(Global.HardwareInfo.RefreshVideoControllerList);
        }
        catch (Exception ex)
        {
            AddLog($"获取显卡信息时出错：{ex.Message}", LogLevel.Warning);
        }

        AddLog("硬件信息获取完成", LogLevel.Info);

        // 更新硬件信息卡片
        UpdateHardwareInfoCards();
    }
    public void Close() => RequestClose?.Invoke(this, false);

    public event EventHandler<object?>? RequestClose;
}