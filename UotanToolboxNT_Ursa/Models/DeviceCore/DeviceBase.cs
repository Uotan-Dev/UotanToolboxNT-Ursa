using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UotanToolboxNT_Ursa.Models.DeviceCore;

public abstract class DeviceBase
{
    /// <summary>
    /// 设备序列号
    /// </summary>
    public string SerialNumber { get; protected set; } = string.Empty;

    /// <summary>
    /// 设备模式
    /// </summary>
    public DeviceMode Mode { get; protected set; } = DeviceMode.Unknown;

    /// <summary>
    /// 设备是否连接
    /// </summary>
    public bool IsConnected { get; protected set; }

    /// <summary>
    /// 设备状态
    /// </summary>
    public string Status { get; protected set; } = "--";

    /// <summary>
    /// 设备品牌
    /// </summary>
    public string Brand { get; protected set; } = "--";

    /// <summary>
    /// 设备型号
    /// </summary>
    public string Model { get; protected set; } = "--";

    /// <summary>
    /// 设备代号
    /// </summary>
    public string CodeName { get; protected set; } = "--";

    /// <summary>
    /// 系统SDK版本
    /// </summary>
    public string SystemSDK { get; protected set; } = "--";

    /// <summary>
    /// CPU架构
    /// </summary>
    public string CPUABI { get; protected set; } = "--";

    /// <summary>
    /// CPU代码
    /// </summary>
    public string CPUCode { get; protected set; } = "--";

    /// <summary>
    /// 显示硬件信息
    /// </summary>
    public string DisplayHW { get; protected set; } = "--";

    /// <summary>
    /// 屏幕密度
    /// </summary>
    public string Density { get; protected set; } = "--";

    /// <summary>
    /// 主板ID
    /// </summary>
    public string BoardID { get; protected set; } = "--";

    /// <summary>
    /// 平台信息
    /// </summary>
    public string Platform { get; protected set; } = "--";

    /// <summary>
    /// 编译信息
    /// </summary>
    public string Compile { get; protected set; } = "--";

    /// <summary>
    /// 内核版本
    /// </summary>
    public string Kernel { get; protected set; } = "--";

    /// <summary>
    /// NDK版本
    /// </summary>
    public string NDKVersion { get; protected set; } = "--";

    /// <summary>
    /// Bootloader状态
    /// </summary>
    public string BootloaderStatus { get; protected set; } = "--";

    /// <summary>
    /// VAB状态
    /// </summary>
    public string VABStatus { get; protected set; } = "--";

    /// <summary>
    /// 电池电量百分比
    /// </summary>
    public string BatteryLevel { get; protected set; } = "0";

    /// <summary>
    /// 电池信息
    /// </summary>
    public string BatteryInfo { get; protected set; } = "--";

    /// <summary>
    /// 内存使用情况
    /// </summary>
    public string MemoryUsage { get; protected set; } = "--";

    /// <summary>
    /// 内存级别
    /// </summary>
    public string MemoryLevel { get; protected set; } = "0";

    /// <summary>
    /// 磁盘信息
    /// </summary>
    public string DiskInfo { get; protected set; } = "--";

    /// <summary>
    /// 磁盘类型
    /// </summary>
    public string DiskType { get; protected set; } = "--";

    /// <summary>
    /// 磁盘使用进度
    /// </summary>
    public string DiskProgress { get; protected set; } = "0";

    /// <summary>
    /// 开机时间
    /// </summary>
    public string PowerOnTime { get; protected set; } = "--";

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdated { get; protected set; } = DateTime.MinValue;

    /// <summary>
    /// 是否已缓存完整设备信息
    /// </summary>
    protected bool _isFullInfoCached = false;

    /// <summary>
    /// 缓存状态
    /// </summary>
    public bool IsFullInfoCached => _isFullInfoCached;

    /// <summary>
    /// 清除缓存状态（用于强制重新获取完整信息）
    /// </summary>
    public void ClearCache() => _isFullInfoCached = false;

    /// <summary>
    /// 刷新设备信息（智能缓存模式）
    /// </summary>
    /// <returns>是否刷新成功</returns>
    public virtual async Task<bool> RefreshDeviceInfoAsync()
    {
        if (!_isFullInfoCached)
        {
            var result = await RefreshFullDeviceInfoAsync();
            if (result)
            {
                _isFullInfoCached = true;
            }
            return result;
        }

        return await RefreshDynamicDeviceInfoAsync();
    }

    /// <summary>
    /// 强制完整刷新设备信息（忽略缓存）
    /// </summary>
    public virtual async Task<bool> ForceRefreshFullDeviceInfoAsync()
    {
        _isFullInfoCached = false;
        var result = await RefreshFullDeviceInfoAsync();
        if (result)
        {
            _isFullInfoCached = true;
        }
        return result;
    }

    /// <summary>
    /// 刷新完整设备信息（所有属性）
    /// </summary>
    protected virtual Task<bool> RefreshFullDeviceInfoAsync()
    {
        LastUpdated = DateTime.Now;
        return Task.FromResult(true);
    }

    /// <summary>
    /// 刷新动态设备信息（仅刷新变化频繁的属性）
    /// </summary>
    protected virtual Task<bool> RefreshDynamicDeviceInfoAsync()
    {
        LastUpdated = DateTime.Now;
        return Task.FromResult(true);
    }

    /// <summary>
    /// 获取应用列表
    /// </summary>
    public virtual Task<List<ApplicationInfo>> GetApplicationListAsync() => Task.FromResult(new List<ApplicationInfo>());

    /// <summary>
    /// 重启到指定模式
    /// </summary>
    /// <param name="mode">目标模式</param>
    public virtual Task<bool> RebootToModeAsync(DeviceMode mode) => Task.FromResult(true);

    /// <summary>
    /// 关机
    /// </summary>
    public virtual Task<bool> PowerOffAsync() => Task.FromResult(true);

    /// <summary>
    /// 检查设备连接状态
    /// </summary>
    public virtual Task<bool> CheckConnectionAsync() => Task.FromResult(IsConnected);

    /// <summary>
    /// 设备状态变化事件
    /// </summary>
    public event EventHandler<DeviceStatusChangedEventArgs>? StatusChanged;

    /// <summary>
    /// 触发设备状态变化事件
    /// </summary>
    /// <param name="oldStatus"></param>
    /// <param name="newStatus"></param>
    protected virtual void OnStatusChanged(string oldStatus, string newStatus) =>
        StatusChanged?.Invoke(this, new DeviceStatusChangedEventArgs(oldStatus, newStatus));
}

/// <summary>
/// 设备状态变化事件参数
/// </summary>
public class DeviceStatusChangedEventArgs(string oldStatus, string newStatus) : EventArgs
{
    public string OldStatus { get; } = oldStatus;
    public string NewStatus { get; } = newStatus;
}
