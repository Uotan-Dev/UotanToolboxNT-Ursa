using System.Collections.Generic;
using System.Threading.Tasks;

namespace UotanToolboxNT_Ursa.Models.DeviceCore;

/// <summary>
/// Fastboot设备类
/// </summary>
public class FastbootDevice : DeviceBase
{
    public FastbootDevice(string serialNumber)
    {
        SerialNumber = serialNumber;
        Mode = DeviceMode.Fastboot;
        Status = "Fastboot";
        IsConnected = true;
    }

    /// <summary>
    /// 刷新完整设备信息（所有属性）
    /// </summary>
    /// <returns></returns>
    protected override Task<bool> RefreshFullDeviceInfoAsync()
    {
        Brand = "Demo Fastboot";
        Model = "Demo";
        CodeName = "Demo";
        BootloaderStatus = "--";
        Platform = "--";
        VABStatus = "--";
        BoardID = "--";
        SystemSDK = "--";
        Compile = "--";
        LastUpdated = System.DateTime.Now;
        return Task.FromResult(true);
    }

    /// <summary>
    /// 刷新动态设备信息
    /// </summary>
    /// <returns></returns>
    protected override Task<bool> RefreshDynamicDeviceInfoAsync()
    {
        LastUpdated = System.DateTime.Now;
        return Task.FromResult(true);
    }

    /// <summary>
    /// 获取应用列表（Fastboot模式不支持）
    /// </summary>
    /// <returns></returns>
    public override Task<List<ApplicationInfo>> GetApplicationListAsync() => Task.FromResult(new List<ApplicationInfo>());

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
