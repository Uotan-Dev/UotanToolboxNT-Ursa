using System;
using System.Threading.Tasks;
using static UotanToolboxNT_Ursa.Models.GlobalLogModel;

namespace UotanToolboxNT_Ursa.Models;

/// <summary>
/// 设备缓存机制使用示例
/// </summary>
public static class DeviceCacheExample
{
    /// <summary>
    /// 演示设备缓存机制的使用方法
    /// </summary>
    /// <param name="device">设备实例</param>
    public static async Task DemonstrateCachingMechanism(DeviceBase device)
    {
        AddLog("=== 设备缓存机制演示 ===", LogLevel.Info);

        // 第一次刷新 - 将获取完整信息并缓存
        AddLog($"首次刷新设备信息，缓存状态：{device.IsFullInfoCached}", LogLevel.Info);
        var startTime = DateTime.Now;
        await device.RefreshDeviceInfoAsync();
        var duration1 = DateTime.Now - startTime;
        AddLog($"首次刷新完成，耗时：{duration1.TotalMilliseconds}ms，缓存状态：{device.IsFullInfoCached}", LogLevel.Info);

        // 等待一段时间
        await Task.Delay(1000);

        // 第二次刷新 - 将仅刷新动态信息（更快）
        AddLog("第二次刷新设备信息（增量刷新）", LogLevel.Info);
        startTime = DateTime.Now;
        await device.RefreshDeviceInfoAsync();
        var duration2 = DateTime.Now - startTime;
        AddLog($"增量刷新完成，耗时：{duration2.TotalMilliseconds}ms，缓存状态：{device.IsFullInfoCached}", LogLevel.Info);

        // 强制完整刷新
        AddLog("强制完整刷新设备信息", LogLevel.Info);
        startTime = DateTime.Now;
        await device.ForceRefreshFullDeviceInfoAsync();
        var duration3 = DateTime.Now - startTime;
        AddLog($"强制完整刷新完成，耗时：{duration3.TotalMilliseconds}ms，缓存状态：{device.IsFullInfoCached}", LogLevel.Info);

        // 清除缓存状态
        device.ClearCache();
        AddLog($"缓存已清除，缓存状态：{device.IsFullInfoCached}", LogLevel.Info);

        AddLog("=== 缓存机制演示完成 ===", LogLevel.Info);
        AddLog($"性能对比：首次刷新 {duration1.TotalMilliseconds}ms，增量刷新 {duration2.TotalMilliseconds}ms，强制刷新 {duration3.TotalMilliseconds}ms", LogLevel.Info);
    }

    /// <summary>
    /// 演示最佳实践的设备信息获取流程
    /// </summary>
    /// <param name="deviceManager">设备管理器</param>
    public static async Task DemonstrateBestPractices(DeviceManager deviceManager)
    {
        AddLog("=== 设备信息获取最佳实践演示 ===", LogLevel.Info);

        // 选择设备后立即获取完整信息
        if (deviceManager.CurrentDevice != null)
        {
            AddLog("设备首次选中，获取完整设备信息...", LogLevel.Info);
            await deviceManager.RefreshCurrentDeviceAsync(); // 首次调用会自动获取完整信息

            // 模拟用户在界面上多次刷新
            for (var i = 1; i <= 3; i++)
            {
                await Task.Delay(2000); // 模拟用户操作间隔
                AddLog($"用户第{i}次手动刷新设备信息", LogLevel.Info);
                await deviceManager.RefreshCurrentDeviceAsync(); // 后续调用只会刷新动态信息
            }

            // 如果需要强制获取最新的完整信息（比如设备可能有系统更新）
            AddLog("需要获取最新完整信息（例如系统更新后）", LogLevel.Info);
            await deviceManager.ForceRefreshCurrentDeviceAsync();
        }

        AddLog("=== 最佳实践演示完成 ===", LogLevel.Info);
    }
}

/// <summary>
/// 缓存配置
/// </summary>
public static class DeviceCacheConfig
{
    /// <summary>
    /// 静态属性列表（首次获取后缓存）
    /// </summary>
    public static readonly string[] StaticProperties =
    [
        nameof(DeviceBase.Brand),
        nameof(DeviceBase.Model),
        nameof(DeviceBase.CodeName),
        nameof(DeviceBase.SystemSDK),
        nameof(DeviceBase.CPUABI),
        nameof(DeviceBase.CPUCode),
        nameof(DeviceBase.DisplayHW),
        nameof(DeviceBase.Density),
        nameof(DeviceBase.BoardID),
        nameof(DeviceBase.Platform),
        nameof(DeviceBase.Compile),
        nameof(DeviceBase.Kernel),
        nameof(DeviceBase.NDKVersion),
        nameof(DeviceBase.BootloaderStatus),
        nameof(DeviceBase.VABStatus),
        nameof(DeviceBase.DiskType),
    ];

    /// <summary>
    /// 动态属性列表（每次刷新都更新）
    /// </summary>
    public static readonly string[] DynamicProperties =
    [
        nameof(DeviceBase.PowerOnTime),
        nameof(DeviceBase.BatteryLevel),
        nameof(DeviceBase.BatteryInfo),
        nameof(DeviceBase.MemoryUsage),
        nameof(DeviceBase.MemoryLevel),
        nameof(DeviceBase.DiskInfo),
        nameof(DeviceBase.DiskProgress),
    ];
}
