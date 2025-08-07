using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AdvancedSharpAdbClient.Models;
using static UotanToolboxNT_Ursa.Models.GlobalLogModel;

namespace UotanToolboxNT_Ursa.Models;

/// <summary>
/// 设备管理器
/// </summary>
public class DeviceManager
{
    private readonly Timer _deviceScanTimer;
    private readonly System.Threading.Lock _lockObject = new();
    private bool _isScanning = false;

    /// <summary>
    /// 当前连接的设备列表
    /// </summary>
    public ObservableCollection<DeviceBase> ConnectedDevices { get; private set; } = [];

    /// <summary>
    /// 当前选中的设备
    /// </summary>
    public DeviceBase? CurrentDevice { get; private set; }

    /// <summary>
    /// 设备列表变化事件
    /// </summary>
    public event EventHandler<DeviceListChangedEventArgs>? DeviceListChanged;

    /// <summary>
    /// 当前设备变化事件
    /// </summary>
    public event EventHandler<DeviceChangedEventArgs>? CurrentDeviceChanged;

    public DeviceManager()
    {
        _deviceScanTimer = new Timer(2000);
        _deviceScanTimer.Elapsed += OnDeviceScanTimer;
        _deviceScanTimer.AutoReset = true;
    }

    /// <summary>
    /// 启动设备管理器
    /// </summary>
    public void Start()
    {
        AddLog("设备管理器已启动", LogLevel.Info);
        _deviceScanTimer.Start();
        _ = Task.Run(ScanDevicesAsync); // 立即执行一次扫描
    }

    /// <summary>
    /// 停止设备管理器
    /// </summary>
    public void Stop()
    {
        _deviceScanTimer.Stop();
        AddLog("设备管理器已停止", LogLevel.Info);
    }

    /// <summary>
    /// 设置当前设备
    /// </summary>
    /// <param name="device">要设置的设备</param>
    public void SetCurrentDevice(DeviceBase? device)
    {
        var oldDevice = CurrentDevice;
        CurrentDevice = device;

        CurrentDeviceChanged?.Invoke(this, new DeviceChangedEventArgs(oldDevice, CurrentDevice));

        if (CurrentDevice != null)
        {
            AddLog($"当前设备已切换到：{CurrentDevice.SerialNumber} ({CurrentDevice.Mode})", LogLevel.Info);
        }
        else
        {
            AddLog("当前设备已清空", LogLevel.Info);
        }
    }

    /// <summary>
    /// 根据序列号获取设备
    /// </summary>
    /// <param name="serialNumber">设备序列号</param>
    /// <returns></returns>
    public DeviceBase? GetDeviceBySerial(string serialNumber)
    {
        lock (_lockObject)
        {
            return ConnectedDevices.FirstOrDefault(d => d.SerialNumber == serialNumber);
        }
    }

    /// <summary>
    /// 获取指定模式的设备列表
    /// </summary>
    /// <param name="mode">设备模式</param>
    /// <returns></returns>
    public List<DeviceBase> GetDevicesByMode(DeviceMode mode)
    {
        lock (_lockObject)
        {
            return [.. ConnectedDevices.Where(d => d.Mode == mode)];
        }
    }

    /// <summary>
    /// 刷新当前设备信息（使用智能缓存）
    /// </summary>
    public async Task<bool> RefreshCurrentDeviceAsync()
    {
        if (CurrentDevice == null)
        {
            AddLog("没有选中的设备", LogLevel.Warning);
            return false;
        }

        return await CurrentDevice.RefreshDeviceInfoAsync();
    }

    /// <summary>
    /// 强制完整刷新当前设备信息（忽略缓存）
    /// </summary>
    public async Task<bool> ForceRefreshCurrentDeviceAsync()
    {
        if (CurrentDevice == null)
        {
            AddLog("没有选中的设备", LogLevel.Warning);
            return false;
        }

        return await CurrentDevice.ForceRefreshFullDeviceInfoAsync();
    }

    /// <summary>
    /// 手动扫描设备
    /// </summary>
    public async Task ScanDevicesAsync()
    {
        if (_isScanning)
        {
            return;
        }

        lock (_lockObject)
        {
            if (_isScanning)
            {
                return;
            }
            _isScanning = true;
        }

        try
        {
            var newDevices = new List<DeviceBase>();

            await ScanAdbDevicesAsync(newDevices);

            await ScanFastbootDevicesAsync(newDevices);

            UpdateDeviceList(newDevices);
        }
        catch (Exception ex)
        {
            AddLog($"扫描设备时出错：{ex.Message}", LogLevel.Error);
        }
        finally
        {
            lock (_lockObject)
            {
                _isScanning = false;
            }
        }
    }

    /// <summary>
    /// 扫描ADB设备
    /// </summary>
    private async Task ScanAdbDevicesAsync(List<DeviceBase> newDevices)
    {
        try
        {
            var adbDevices = await Task.Run(Global.AdbClient.GetDevices);

            foreach (var deviceData in adbDevices)
            {
                if (deviceData.State == DeviceState.Online)
                {
                    var device = new AdbDevice(deviceData);
                    newDevices.Add(device);
                }
            }
        }
        catch (Exception ex)
        {
            AddLog($"扫描ADB设备失败：{ex.Message}", LogLevel.Warning);
        }
    }

    /// <summary>
    /// 扫描Fastboot设备
    /// </summary>
    private async Task ScanFastbootDevicesAsync(List<DeviceBase> newDevices)
    {
        try
        {
            var fastbootPath = Path.Combine(Global.BinDirectory.FullName, "platform-tools", "fastboot.exe");
            if (!File.Exists(fastbootPath))
            {
                return;
            }

            using var process = new Process();
            process.StartInfo.FileName = fastbootPath;
            process.StartInfo.Arguments = "devices";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrEmpty(output))
            {
                var lines = output.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("fastboot"))
                    {
                        var parts = line.Split('\t');
                        if (parts.Length >= 2)
                        {
                            var serialNumber = parts[0].Trim();
                            if (!string.IsNullOrEmpty(serialNumber))
                            {
                                var device = new FastbootDevice(serialNumber);
                                newDevices.Add(device);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            AddLog($"扫描Fastboot设备失败：{ex.Message}", LogLevel.Warning);
        }
    }

    /// <summary>
    /// 更新设备列表
    /// </summary>
    private void UpdateDeviceList(List<DeviceBase> newDevices)
    {
        lock (_lockObject)
        {
            var oldDevices = ConnectedDevices.ToList();

            var removedDevices = oldDevices.Where(old =>
                !newDevices.Any(n => n.SerialNumber == old.SerialNumber && n.Mode == old.Mode)).ToList();

            var addedDevices = newDevices.Where(newDev =>
                !oldDevices.Any(old => old.SerialNumber == newDev.SerialNumber && old.Mode == newDev.Mode)).ToList();

            foreach (var device in removedDevices)
            {
                ConnectedDevices.Remove(device);
                AddLog($"设备已断开：{device.SerialNumber} ({device.Mode})", LogLevel.Info);
            }

            foreach (var device in addedDevices)
            {
                ConnectedDevices.Add(device);
                AddLog($"发现新设备：{device.SerialNumber} ({device.Mode})", LogLevel.Info);
            }

            if (CurrentDevice != null)
            {
                var currentStillConnected = ConnectedDevices.Any(d =>
                    d.SerialNumber == CurrentDevice.SerialNumber && d.Mode == CurrentDevice.Mode);

                if (!currentStillConnected)
                {
                    AddLog($"当前设备已断开：{CurrentDevice.SerialNumber}", LogLevel.Warning);
                    SetCurrentDevice(null);
                }
            }

            if (CurrentDevice == null && ConnectedDevices.Count > 0)
            {
                SetCurrentDevice(ConnectedDevices[0]);
            }

            if (addedDevices.Count > 0 || removedDevices.Count > 0)
            {
                DeviceListChanged?.Invoke(this, new DeviceListChangedEventArgs(addedDevices, removedDevices));
            }
        }
    }

    /// <summary>
    /// 定时器事件处理
    /// </summary>
    private async void OnDeviceScanTimer(object? sender, ElapsedEventArgs e) => await ScanDevicesAsync();

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Stop();
        _deviceScanTimer?.Dispose();
    }
}

/// <summary>
/// 设备列表变化事件参数
/// </summary>
public class DeviceListChangedEventArgs(List<DeviceBase> addedDevices, List<DeviceBase> removedDevices) : EventArgs
{
    public List<DeviceBase> AddedDevices { get; } = addedDevices;
    public List<DeviceBase> RemovedDevices { get; } = removedDevices;
}

/// <summary>
/// 当前设备变化事件参数
/// </summary>
public class DeviceChangedEventArgs(DeviceBase? oldDevice, DeviceBase? newDevice) : EventArgs
{
    public DeviceBase? OldDevice { get; } = oldDevice;
    public DeviceBase? NewDevice { get; } = newDevice;
}
