using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UotanToolboxNT_Ursa.Models;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    private string _progressDisk = "0", _memLevel = "0", _status = "--", _bLStatus = "--",
    _vABStatus = "--", _codeName = "--", _vNDKVersion = "--", _cPUCode = "--",
    _powerOnTime = "--", _deviceBrand = "--", _deviceModel = "--", _systemSDK = "--",
    _cPUABI = "--", _displayHW = "--", _density = "--", _boardID = "--", _platform = "--",
    _compile = "--", _kernel = "--", _selectedSimpleContent = "", _diskType = "--",
    _batteryLevel = "0", _batteryInfo = "--", _useMem = "--", _diskInfo = "--";
    [ObservableProperty] private bool _isConnecting;
    [ObservableProperty] private bool _commonDevicesList;
    [ObservableProperty] private AvaloniaList<string> _simpleContent = [];

    // 添加缓存机制，避免频繁刷新同一设备
    private DeviceBase? _lastRefreshedDevice;
    private DateTime _lastRefreshTime = DateTime.MinValue;
    private readonly TimeSpan _refreshInterval = TimeSpan.FromSeconds(3); // 3秒内不重复刷新同一设备

    public HomeViewModel()
    {
        Global.DeviceManager.CurrentDeviceChanged += OnCurrentDeviceChanged;
        Global.DeviceManager.DeviceListChanged += OnDeviceListChanged;

        UpdateDeviceList();

        if (Global.DeviceManager.CurrentDevice != null)
        {
            _ = UpdateDeviceDisplayAsync(refreshDeviceInfo: false);

            _ = Task.Run(async () =>
            {
                try
                {
                    await UpdateDeviceDisplayAsync(refreshDeviceInfo: true);
                }
                catch (Exception ex)
                {
                    GlobalLogModel.AddLog($"初始化设备信息失败: {ex.Message}", GlobalLogModel.LogLevel.Warning);
                }
            });
        }

        GlobalLogModel.UpdateHardwareInfoCards();
    }

    /// <summary>
    /// 当前设备变化事件处理
    /// </summary>
    private async void OnCurrentDeviceChanged(object? sender, DeviceChangedEventArgs e)
    {
        await UpdateDeviceDisplayAsync(refreshDeviceInfo: false);

        _ = Task.Run(async () =>
        {
            try
            {
                await UpdateDeviceDisplayAsync(refreshDeviceInfo: true);
            }
            catch (Exception ex)
            {
                GlobalLogModel.AddLog($"刷新设备信息失败: {ex.Message}", GlobalLogModel.LogLevel.Warning);
            }
        });
    }

    /// <summary>
    /// 设备列表变化事件处理
    /// </summary>
    private void OnDeviceListChanged(object? sender, DeviceListChangedEventArgs e)
    {
        CommonDevicesList = Global.DeviceManager.ConnectedDevices.Count > 0;

        UpdateDeviceList();
    }

    /// <summary>
    /// 更新设备显示信息
    /// </summary>
    /// <param name="refreshDeviceInfo">是否刷新设备详细信息</param>
    private async Task UpdateDeviceDisplayAsync(bool refreshDeviceInfo = true)
    {
        var device = Global.DeviceManager.CurrentDevice;
        if (device == null)
        {
            // 清空显示
            ResetDeviceDisplay();
            _lastRefreshedDevice = null;
            return;
        }

        if (refreshDeviceInfo)
        {
            var now = DateTime.Now;
            var shouldRefresh = _lastRefreshedDevice != device ||
                                (now - _lastRefreshTime) > _refreshInterval;

            if (shouldRefresh)
            {
                await device.RefreshDeviceInfoAsync();
                _lastRefreshedDevice = device;
                _lastRefreshTime = now;
            }
        }

        // 更新显示属性
        DeviceBrand = device.Brand;
        DeviceModel = device.Model;
        CodeName = device.CodeName;
        Status = device.Status;
        BLStatus = device.BootloaderStatus;
        VABStatus = device.VABStatus;
        VNDKVersion = device.NDKVersion;
        CPUCode = device.CPUCode;
        PowerOnTime = device.PowerOnTime;
        SystemSDK = device.SystemSDK;
        CPUABI = device.CPUABI;
        DisplayHW = device.DisplayHW;
        Density = device.Density;
        BoardID = device.BoardID;
        Platform = device.Platform;
        Compile = device.Compile;
        Kernel = device.Kernel;
        DiskType = device.DiskType;
        BatteryLevel = device.BatteryLevel;
        BatteryInfo = device.BatteryInfo;
        UseMem = device.MemoryUsage;
        DiskInfo = device.DiskInfo;
        ProgressDisk = device.DiskProgress;
        MemLevel = device.MemoryLevel;
    }

    /// <summary>
    /// 重置设备显示
    /// </summary>
    private void ResetDeviceDisplay()
    {
        DeviceBrand = "--";
        DeviceModel = "--";
        CodeName = "--";
        Status = "--";
        BLStatus = "--";
        VABStatus = "--";
        VNDKVersion = "--";
        CPUCode = "--";
        PowerOnTime = "--";
        SystemSDK = "--";
        CPUABI = "--";
        DisplayHW = "--";
        Density = "--";
        BoardID = "--";
        Platform = "--";
        Compile = "--";
        Kernel = "--";
        DiskType = "--";
        BatteryLevel = "0";
        BatteryInfo = "--";
        UseMem = "--";
        DiskInfo = "--";
        ProgressDisk = "0";
        MemLevel = "0";
    }

    /// <summary>
    /// 更新设备列表
    /// </summary>
    private void UpdateDeviceList()
    {
        SimpleContent.Clear();

        var devices = Global.DeviceManager.ConnectedDevices;
        if (devices.Count == 0)
        {
            SimpleContent.Add("无设备连接");
            SelectedSimpleContent = "无设备连接";
        }
        else
        {
            foreach (var device in devices)
            {
                var deviceDisplayName = $"{device.SerialNumber} ({device.Mode})";
                SimpleContent.Add(deviceDisplayName);
            }

            if (Global.DeviceManager.CurrentDevice != null)
            {
                var currentDeviceDisplay = $"{Global.DeviceManager.CurrentDevice.SerialNumber} ({Global.DeviceManager.CurrentDevice.Mode})";
                if (SimpleContent.Contains(currentDeviceDisplay))
                {
                    SelectedSimpleContent = currentDeviceDisplay;
                }
            }
            else if (SimpleContent.Count > 0)
            {
                SelectedSimpleContent = SimpleContent[0];
            }
        }
    }

    /// <summary>
    /// 处理设备选择变化
    /// </summary>
    partial void OnSelectedSimpleContentChanged(string value)
    {
        if (string.IsNullOrEmpty(value) || value == "无设备连接")
        {
            return;
        }

        _ = Task.Run(() =>
        {
            try
            {
                //格式：SerialNumber (Mode)
                var parts = value.Split(" (");
                if (parts.Length == 2)
                {
                    var serialNumber = parts[0];
                    var modeStr = parts[1].TrimEnd(')');

                    if (Enum.TryParse<DeviceMode>(modeStr, out var mode))
                    {
                        var device = Global.DeviceManager.ConnectedDevices
                            .FirstOrDefault(d => d.SerialNumber == serialNumber && d.Mode == mode);

                        if (device != null && device != Global.DeviceManager.CurrentDevice)
                        {
                            Global.DeviceManager.SetCurrentDevice(device);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalLogModel.AddLog($"切换设备失败: {ex.Message}", GlobalLogModel.LogLevel.Error);
            }
        });
    }

    [RelayCommand]
    public async Task FreshDeviceList()
    {
        IsConnecting = true;

        try
        {
            await Global.DeviceManager.ScanDevicesAsync();
            UpdateDeviceList();

            await UpdateDeviceDisplayAsync(refreshDeviceInfo: false);

            _ = Task.Run(async () =>
            {
                try
                {
                    await UpdateDeviceDisplayAsync(refreshDeviceInfo: true);
                }
                catch (Exception ex)
                {
                    GlobalLogModel.AddLog($"刷新设备详细信息失败: {ex.Message}", GlobalLogModel.LogLevel.Warning);
                }
            });

            GlobalLogModel.UpdateHardwareInfoCards();
        }
        catch (Exception ex)
        {
            GlobalLogModel.AddLog($"扫描设备失败: {ex.Message}", GlobalLogModel.LogLevel.Error);
        }
        finally
        {
            IsConnecting = false;
        }
    }

    [RelayCommand]
    public async Task OpenAFDI()
    {
        var device = Global.DeviceManager.CurrentDevice;
        if (device != null && device.Mode == DeviceMode.Adb)
        {
            _ = await device.GetApplicationListAsync();
            // TODO: 打开应用管理界面，显示应用列表
        }
    }

    [RelayCommand]
    public Task Open9008DI() =>
        // TODO: 处理9008模式设备信息
        Task.CompletedTask;

    [RelayCommand]
    public Task OpenUSBP() => Task.CompletedTask;


    [RelayCommand]
    public Task OpenReUSBP() =>
        // TODO: 重新枚举USB端口
        Task.CompletedTask;

    [RelayCommand]
    public async Task RebootSys()
    {
        var device = Global.DeviceManager.CurrentDevice;
        if (device != null)
        {
            await device.RebootToModeAsync(DeviceMode.Adb);
        }
    }

    [RelayCommand]
    public async Task RebootRec()
    {
        var device = Global.DeviceManager.CurrentDevice;
        if (device != null)
        {
            await device.RebootToModeAsync(DeviceMode.Recovery);
        }
    }

    [RelayCommand]
    public async Task RebootBL()
    {
        var device = Global.DeviceManager.CurrentDevice;
        if (device != null)
        {
            await device.RebootToModeAsync(DeviceMode.Fastboot);
        }
    }

    [RelayCommand]
    public async Task RebootFB()
    {
        var device = Global.DeviceManager.CurrentDevice;
        if (device != null)
        {
            await device.RebootToModeAsync(DeviceMode.Fastboot);
        }
    }

    [RelayCommand]
    public async Task PowerOff()
    {
        var device = Global.DeviceManager.CurrentDevice;
        if (device != null)
        {
            await device.PowerOffAsync();
        }
    }

    [RelayCommand]
    public async Task RebootEDL()
    {
        var device = Global.DeviceManager.CurrentDevice;
        if (device != null)
        {
            await device.RebootToModeAsync(DeviceMode.EDL);
        }
    }
}