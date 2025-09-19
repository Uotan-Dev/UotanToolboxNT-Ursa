using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UotanToolboxNT_Ursa.Helper;
using UotanToolboxNT_Ursa.Models;
using UotanToolboxNT_Ursa.Models.DeviceCore;
using static UotanToolboxNT_Ursa.Models.GlobalLogModel;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class AppmgrViewModel : ObservableObject
{
    /// 加载中
    [ObservableProperty]
    private bool _isInstalling = false, _isBusy = false;

    /// APK文件路径
    [ObservableProperty]
    private string _apkFile = string.Empty;

    /// 搜索框
    [ObservableProperty]
    private bool _sBoxEnabled = true;
    [ObservableProperty]
    private string _search = string.Empty;
    [ObservableProperty]
    private string _sBoxWater = string.Empty;

    /// 是否显示系统应用
    [ObservableProperty]
    private bool _isSystemAppDisplayed = false;

    /// 应用程序列表
    [ObservableProperty]
    private ObservableCollection<ApplicationInfo> _applications = [];

    /// 是否有应用
    [ObservableProperty]
    private bool _hasItems = false;

    /// 当前选中的应用信息
    [ObservableProperty]
    private ApplicationInfo? _selectedApplication;
    private ApplicationInfo[]? _allApplicationInfos;
    private List<ApplicationInfo>? _applicationInfos;

    public AppmgrViewModel()
    {
        // 设置默认搜索框水印
        SBoxWater = GetTranslation("Appmgr_SearchApp");

        // 监听搜索关键词变化
        PropertyChanged += OnPropertyChanged;
    }

    /// <summary>
    /// 获取翻译文本
    /// </summary>
    /// <param name="key">翻译键</param>
    /// <returns>翻译后的文本</returns>
    private static string GetTranslation(string key) => LanguageResourceHelper.GetLanguageResource<string>(key) ?? key;

    /// <summary>
    /// 属性变化事件处理
    /// </summary>
    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Search))
        {
            FilterApplications();
        }
        else if (e.PropertyName == nameof(IsSystemAppDisplayed))
        {
            _ = ConnectAsync();
        }
    }

    /// <summary>
    /// 执行ADB命令的辅助方法
    /// </summary>
    private static async Task<string> ExecuteAdbCommand(AdvancedSharpAdbClient.Models.DeviceData deviceData, string command)
    {
        try
        {
            var receiver = new AdvancedSharpAdbClient.Receivers.ConsoleOutputReceiver();
            await Task.Run(() => Global.AdbClient.ExecuteRemoteCommand(command, deviceData, receiver, System.Text.Encoding.UTF8));
            return receiver.ToString().Trim();
        }
        catch (Exception ex)
        {
            AddLog($"执行命令 '{command}' 失败：{ex.Message}", LogLevel.Warning);
            return string.Empty;
        }
    }

    /// <summary>
    /// 运行应用
    /// </summary>
    [RelayCommand]
    public async Task RunAppAsync()
    {
        if (SelectedApplication == null)
        {
            AddLog("未选择应用", LogLevel.Warning);
            return;
        }

        try
        {
            var currentDevice = Global.DeviceManager.CurrentDevice;
            if (currentDevice == null)
            {
                AddLog("未连接设备", LogLevel.Error);
                return;
            }

            if (currentDevice is not AdbDevice adbDevice)
            {
                AddLog("当前设备不支持运行应用操作", LogLevel.Error);
                return;
            }

            AddLog($"正在启动应用：{SelectedApplication.DisplayName ?? SelectedApplication.Name}", LogLevel.Info);

            var deviceData = Global.AdbClient.GetDevices().FirstOrDefault(d => d.Serial == currentDevice.SerialNumber);
            if (deviceData == null)
            {
                AddLog("无法获取设备信息", LogLevel.Error);
                return;
            }

            var startCommand = $"am start -n {SelectedApplication.Name}/.MainActivity";
            var result = await ExecuteAdbCommand(deviceData, startCommand);

            if (string.IsNullOrEmpty(result) || result.Contains("Error") || result.Contains("does not exist"))
            {
                var launchCommand = $"monkey -p {SelectedApplication.Name} -c android.intent.category.LAUNCHER 1";
                result = await ExecuteAdbCommand(deviceData, launchCommand);
            }

            if (!string.IsNullOrEmpty(result) && !result.Contains("Error"))
            {
                AddLog($"应用启动成功：{SelectedApplication.DisplayName ?? SelectedApplication.Name}", LogLevel.Info);
            }
            else
            {
                AddLog($"应用启动失败：{result}", LogLevel.Error);
            }
        }
        catch (Exception ex)
        {
            AddLog($"启动应用时发生异常：{ex.Message}", LogLevel.Error);
        }
    }

    /// <summary>
    /// 强制停止应用
    /// </summary>
    [RelayCommand]
    public async Task ForceStopAppAsync()
    {
        if (SelectedApplication == null)
        {
            AddLog("未选择应用", LogLevel.Warning);
            return;
        }

        try
        {
            var currentDevice = Global.DeviceManager.CurrentDevice;
            if (currentDevice == null)
            {
                AddLog("未连接设备", LogLevel.Error);
                return;
            }

            if (currentDevice is not AdbDevice)
            {
                AddLog("当前设备不支持强制停止应用操作", LogLevel.Error);
                return;
            }

            AddLog($"正在强制停止应用：{SelectedApplication.DisplayName ?? SelectedApplication.Name}", LogLevel.Info);

            var deviceData = Global.AdbClient.GetDevices().FirstOrDefault(d => d.Serial == currentDevice.SerialNumber);
            if (deviceData == null)
            {
                AddLog("无法获取设备信息", LogLevel.Error);
                return;
            }
            var stopCommand = $"am force-stop {SelectedApplication.Name}";
            var result = await ExecuteAdbCommand(deviceData, stopCommand);

            AddLog($"应用强制停止完成：{SelectedApplication.DisplayName ?? SelectedApplication.Name}", LogLevel.Info);
        }
        catch (Exception ex)
        {
            AddLog($"强制停止应用时发生异常：{ex.Message}", LogLevel.Error);
        }
    }

    /// <summary>
    /// 激活应用
    /// </summary>
    [RelayCommand]
    public async Task ActivateAppAsync()
    {
        if (SelectedApplication == null)
        {
            return;
        }

        try
        {
            // 实现启用/禁用应用的逻辑
            await Task.Delay(500); // 模拟执行过程
        }
        catch
        {
            // 处理异常
        }
    }

    /// <summary>
    /// 禁用应用
    /// </summary>
    [RelayCommand]
    public async Task DisableAppAsync()
    {
        if (SelectedApplication == null)
        {
            AddLog("未选择应用", LogLevel.Warning);
            return;
        }

        try
        {
            var currentDevice = Global.DeviceManager.CurrentDevice;
            if (currentDevice == null)
            {
                AddLog("未连接设备", LogLevel.Error);
                return;
            }

            if (currentDevice is not AdbDevice)
            {
                AddLog("当前设备不支持禁用应用操作", LogLevel.Error);
                return;
            }

            AddLog($"正在禁用应用：{SelectedApplication.DisplayName ?? SelectedApplication.Name}", LogLevel.Info);

            var deviceData = Global.AdbClient.GetDevices().FirstOrDefault(d => d.Serial == currentDevice.SerialNumber);
            if (deviceData == null)
            {
                AddLog("无法获取设备信息", LogLevel.Error);
                return;
            }

            var disableCommand = $"pm disable-user --user 0 {SelectedApplication.Name}";
            var result = await ExecuteAdbCommand(deviceData, disableCommand);

            if (result.Contains("disabled"))
            {
                AddLog($"应用禁用成功：{SelectedApplication.DisplayName ?? SelectedApplication.Name}", LogLevel.Info);
            }
            else
            {
                AddLog($"应用禁用失败：{result}", LogLevel.Error);
            }
        }
        catch (Exception ex)
        {
            AddLog($"禁用应用时发生异常：{ex.Message}", LogLevel.Error);
        }
    }

    /// <summary>
    /// 启用应用
    /// </summary>
    [RelayCommand]
    public async Task EnableAppAsync()
    {
        if (SelectedApplication == null)
        {
            AddLog("未选择应用", LogLevel.Warning);
            return;
        }

        try
        {
            var currentDevice = Global.DeviceManager.CurrentDevice;
            if (currentDevice == null)
            {
                AddLog("未连接设备", LogLevel.Error);
                return;
            }

            if (currentDevice is not AdbDevice)
            {
                AddLog("当前设备不支持启用应用操作", LogLevel.Error);
                return;
            }

            AddLog($"正在启用应用：{SelectedApplication.DisplayName ?? SelectedApplication.Name}", LogLevel.Info);

            var deviceData = Global.AdbClient.GetDevices().FirstOrDefault(d => d.Serial == currentDevice.SerialNumber);
            if (deviceData == null)
            {
                AddLog("无法获取设备信息", LogLevel.Error);
                return;
            }

            var enableCommand = $"pm enable {SelectedApplication.Name}";
            var result = await ExecuteAdbCommand(deviceData, enableCommand);

            if (result.Contains("enabled"))
            {
                AddLog($"应用启用成功：{SelectedApplication.DisplayName ?? SelectedApplication.Name}", LogLevel.Info);
            }
            else
            {
                AddLog($"应用启用失败：{result}", LogLevel.Error);
            }
        }
        catch (Exception ex)
        {
            AddLog($"启用应用时发生异常：{ex.Message}", LogLevel.Error);
        }
    }

    /// <summary>
    /// 提取APK
    /// </summary>
    [RelayCommand]
    public async Task ExtractApkAsync()
    {
        if (SelectedApplication == null)
        {
            return;
        }

        try
        {
            // 实现提取APK的逻辑
            await Task.Delay(1000); // 模拟提取过程
        }
        catch
        {
            // 处理异常
        }
    }

    /// <summary>
    /// 卸载应用
    /// </summary>
    [RelayCommand]
    public async Task UninstallAppAsync()
    {
        if (SelectedApplication == null)
        {
            return;
        }

        try
        {
            // 实现卸载应用的逻辑
            await Task.Delay(1000); // 模拟卸载过程
            // 卸载成功后从列表中移除
            Applications.Remove(SelectedApplication);
            SelectedApplication = null;
        }
        catch
        {
            // 处理异常
        }
    }

    /// <summary>
    /// 保留数据卸载应用
    /// </summary>
    [RelayCommand]
    public async Task KeepDataUninstallAppAsync()
    {
        if (SelectedApplication == null)
        {
            return;
        }

        try
        {
            // 实现保留数据卸载应用的逻辑
            await Task.Delay(1000); // 模拟卸载过程
            // 卸载成功后从列表中移除
            Applications.Remove(SelectedApplication);
            SelectedApplication = null;
        }
        catch
        {
            // 处理异常
        }
    }

    /// <summary>
    /// 清除应用数据
    /// </summary>
    [RelayCommand]
    public async Task ClearAppDataAsync()
    {
        if (SelectedApplication == null)
        {
            AddLog("未选择应用", LogLevel.Warning);
            return;
        }

        try
        {
            var currentDevice = Global.DeviceManager.CurrentDevice;
            if (currentDevice == null)
            {
                AddLog("未连接设备", LogLevel.Error);
                return;
            }

            if (currentDevice is not AdbDevice)
            {
                AddLog("当前设备不支持清除应用数据操作", LogLevel.Error);
                return;
            }

            AddLog($"正在清除应用数据：{SelectedApplication.DisplayName ?? SelectedApplication.Name}", LogLevel.Info);

            var deviceData = Global.AdbClient.GetDevices().FirstOrDefault(d => d.Serial == currentDevice.SerialNumber);
            if (deviceData == null)
            {
                AddLog("无法获取设备信息", LogLevel.Error);
                return;
            }

            var clearCommand = $"pm clear {SelectedApplication.Name}";
            var result = await ExecuteAdbCommand(deviceData, clearCommand);

            if (result.Contains("Success"))
            {
                AddLog($"应用数据清除成功：{SelectedApplication.DisplayName ?? SelectedApplication.Name}", LogLevel.Info);
            }
            else
            {
                AddLog($"应用数据清除失败：{result}", LogLevel.Error);
            }
        }
        catch (Exception ex)
        {
            AddLog($"清除应用数据时发生异常：{ex.Message}", LogLevel.Error);
        }
    }

    /// <summary>
    /// 选择APK文件
    /// </summary>
    [RelayCommand]
    public async Task SelectApkFileAsync()
    {
        try
        {
            if (FileSelectionHandler != null)
            {
                await FileSelectionHandler.Invoke();
            }
        }
        catch (Exception ex)
        {
            AddLog($"选择应用时发生异常：{ex.Message}", LogLevel.Error);

        }
    }

    /// <summary>
    /// 安装APK文件
    /// </summary>
    [RelayCommand]
    public async Task InstallApkAsync()
    {
        if (string.IsNullOrEmpty(ApkFile))
        {
            return;
        }

        IsInstalling = true;

        try
        {
            // 这里需要实现具体的APK安装逻辑
            await Task.Delay(2000); // 模拟安装过程

            // 安装完成后刷新应用列表
            await ConnectAsync();
        }
        catch
        {
            // 处理安装异常
        }
        finally
        {
            IsInstalling = false;
        }
    }

    /// <summary>
    /// 过滤应用程序列表
    /// </summary>
    private void FilterApplications()
    {
        if (_applicationInfos != null && _allApplicationInfos != null)
        {
            if (!string.IsNullOrEmpty(Search))
            {
                var filteredApps = _allApplicationInfos
                    .Where(app => app.DisplayName?.Contains(Search, StringComparison.OrdinalIgnoreCase) == true ||
                                  app.Name.Contains(Search, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(app => app.Name)
                    .ToList();

                Applications = new ObservableCollection<ApplicationInfo>(filteredApps);
            }
            else
            {
                var sortedApps = _allApplicationInfos
                    .Where(info => info != null)
                    .OrderBy(app => app.Name)
                    .ToList();

                Applications = new ObservableCollection<ApplicationInfo>(sortedApps);
            }
        }
    }

    /// <summary>
    /// 连接设备并获取应用列表
    /// </summary>
    [RelayCommand]
    public async Task ConnectAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        SBoxEnabled = false;
        HasItems = false;

        try
        {
            var currentDevice = Global.DeviceManager.CurrentDevice;
            if (currentDevice == null)
            {
                AddLog("未连接设备", LogLevel.Warning);
                Applications.Clear();
                return;
            }

            if (currentDevice is not AdbDevice adbDevice)
            {
                AddLog("当前设备不支持应用管理操作", LogLevel.Warning);
                Applications.Clear();
                return;
            }

            AddLog("正在获取应用列表...", LogLevel.Info);
            var applicationList = await adbDevice.GetApplicationListAsync(!IsSystemAppDisplayed);

            if (applicationList != null && applicationList.Count > 0)
            {
                _allApplicationInfos = [.. applicationList];
                _applicationInfos = applicationList;

                FilterApplications();

                HasItems = Applications.Count > 0;
                AddLog($"成功获取 {Applications.Count} 个应用", LogLevel.Info);
            }
            else
            {
                Applications.Clear();
                _allApplicationInfos = null;
                _applicationInfos = null;
                HasItems = false;
                AddLog("未找到任何应用", LogLevel.Warning);
            }
        }
        catch (Exception ex)
        {
            AddLog($"获取应用列表时发生异常：{ex.Message}", LogLevel.Error);
            Applications.Clear();
            _allApplicationInfos = null;
            _applicationInfos = null;
            HasItems = false;
        }
        finally
        {
            IsBusy = false;
            SBoxEnabled = true;
        }
    }

    /// <summary>
    /// 文件选择处理委托
    /// </summary>
    public Func<Task>? FileSelectionHandler { get; set; }

    /// <summary>
    /// 设置选中的APK文件路径（由View调用）
    /// </summary>
    /// <param name="filePath">文件路径</param>
    public void SetSelectedApkFile(string filePath) => ApkFile = filePath;

}