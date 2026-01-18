using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static UotanToolboxNT_Ursa.Models.GlobalLogModel;

namespace UotanToolboxNT_Ursa.Models.DeviceCore;

public class DeviceManager : IDisposable
{
    /// <summary>
    /// 当前连接的设备列表
    /// </summary>
    public ObservableCollection<DeviceBase> ConnectedDevices { get; } = [];

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

    /// <summary>
    /// 启动设备管理器
    /// </summary>
    public void Start()
    {
        AddLog("设备管理器已启动 (精简模式)", LogLevel.Info);
    }

    /// <summary>
    /// 停止设备管理器
    /// </summary>
    public void Stop()
    {
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
    }

    /// <summary>
    /// 根据序列号获取设备
    /// </summary>
    /// <param name="serialNumber">设备序列号</param>
    /// <returns></returns>
    public DeviceBase? GetDeviceBySerial(string serialNumber) => ConnectedDevices.FirstOrDefault(d => d.SerialNumber == serialNumber);

    /// <summary>
    /// 获取指定模式的设备列表
    /// </summary>
    /// <param name="mode">设备模式</param>
    /// <returns></returns>
    public List<DeviceBase> GetDevicesByMode(DeviceMode mode) => [.. ConnectedDevices.Where(d => d.Mode == mode)];

    /// <summary>
    /// 刷新当前设备信息（使用缓存）
    /// </summary>
    public Task<bool> RefreshCurrentDeviceAsync() => CurrentDevice?.RefreshDeviceInfoAsync() ?? Task.FromResult(false);

    /// <summary>
    /// 强制完整刷新当前设备信息（忽略缓存）
    /// </summary>
    public Task<bool> ForceRefreshCurrentDeviceAsync() => CurrentDevice?.ForceRefreshFullDeviceInfoAsync() ?? Task.FromResult(false);

    /// <summary>
    /// 扫描设备
    /// </summary>
    public Task ScanDevicesAsync()
    {
        var removed = ConnectedDevices.ToList();
        ConnectedDevices.Clear();
        SetCurrentDevice(null);
        if (removed.Count > 0)
        {
            DeviceListChanged?.Invoke(this, new DeviceListChangedEventArgs([], removed));
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose() => Stop();
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
