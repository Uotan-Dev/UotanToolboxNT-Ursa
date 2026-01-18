using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UotanToolboxNT_Ursa.Models.DeviceCore;

/// <summary>
/// ADB设备类
/// </summary>
public class AdbDevice : DeviceBase
{
    public AdbDevice(string serialNumber)
    {
        SerialNumber = serialNumber;
        Mode = DeviceMode.Adb;
        Status = "ADB";
        IsConnected = true;
    }

    /// <summary>
    /// 清理字符串，移除换行符和回车符
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <returns>清理后的字符串</returns>
    private static string CleanString(string? input) =>
        string.IsNullOrEmpty(input) ? "--" : input.Replace("\r", "").Replace("\n", "").Trim();

    /// <summary>
    /// 刷新完整设备信息（所有属性）
    /// </summary>
    protected override Task<bool> RefreshFullDeviceInfoAsync()
    {
        Brand = "Demo Brand";
        Model = "Demo Model";
        CodeName = "Demo";
        SystemSDK = "Android";
        CPUABI = "arm64";
        CPUCode = "Demo CPU";
        DisplayHW = "1080x1920";
        Density = "420";
        BoardID = "--";
        Platform = "Demo";
        Compile = "--";
        Kernel = "--";
        NDKVersion = "--";
        BootloaderStatus = "--";
        VABStatus = "--";
        DiskType = "--";
        BatteryLevel = "0";
        BatteryInfo = "--";
        MemoryUsage = "--";
        MemoryLevel = "0";
        DiskInfo = "--";
        DiskProgress = "0";
        PowerOnTime = "--";
        LastUpdated = DateTime.Now;
        return Task.FromResult(true);
    }

    /// <summary>
    /// 刷新动态设备信息（仅刷新变化频繁的属性）
    /// </summary>
    protected override Task<bool> RefreshDynamicDeviceInfoAsync()
    {
        LastUpdated = DateTime.Now;
        return Task.FromResult(true);
    }

    /// <summary>
    /// 获取应用列表
    /// </summary>
    /// <param name="thirdPartyOnly">是否只获取第三方应用</param>
    /// <returns></returns>
    public override Task<List<ApplicationInfo>> GetApplicationListAsync() => Task.FromResult(new List<ApplicationInfo>());
    public Task<List<ApplicationInfo>> GetApplicationListAsync(bool thirdPartyOnly) => GetApplicationListAsync();

    /// <summary>
    /// 重启到指定模式
    /// </summary>
    /// <param name="mode">目标模式</param>
    /// <returns></returns>
    public override Task<bool> RebootToModeAsync(DeviceMode mode) => Task.FromResult(true);

    /// <summary>
    /// 关机
    /// </summary>
    /// <returns></returns>
    public override Task<bool> PowerOffAsync() => Task.FromResult(true);

    /// <summary>
    /// 检查设备连接状态
    /// </summary>
    /// <returns></returns>
    public override Task<bool> CheckConnectionAsync()
    {
        IsConnected = true;
        return Task.FromResult(IsConnected);
    }
}