using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.Models;
using static UotanToolboxNT_Ursa.Models.GlobalLogModel;

namespace UotanToolboxNT_Ursa.Models;

/// <summary>
/// ADB设备类
/// </summary>
public class AdbDevice : DeviceBase
{
    private readonly DeviceData _deviceData;

    public AdbDevice(DeviceData deviceData)
    {
        _deviceData = deviceData;
        SerialNumber = deviceData.Serial;
        Mode = DeviceMode.Adb;
        Status = deviceData.State.ToString();
        IsConnected = deviceData.State == DeviceState.Online;
    }

    /// <summary>
    /// 刷新设备信息
    /// </summary>
    /// <returns></returns>
    public override async Task<bool> RefreshDeviceInfoAsync()
    {
        try
        {
            if (!IsConnected)
            {
                return false;
            }

            AddLog($"正在刷新设备 {SerialNumber} 的信息...", LogLevel.Info);

            // 使用shell命令获取设备属性
            Brand = await ExecuteShellCommand("getprop ro.product.brand") ?? "--";
            Model = await ExecuteShellCommand("getprop ro.product.model") ?? "--";
            CodeName = await ExecuteShellCommand("getprop ro.product.device") ?? "--";
            SystemSDK = await ExecuteShellCommand("getprop ro.build.version.sdk") ?? "--";
            CPUABI = await ExecuteShellCommand("getprop ro.product.cpu.abi") ?? "--";
            Platform = await ExecuteShellCommand("getprop ro.board.platform") ?? "--";
            Compile = await ExecuteShellCommand("getprop ro.build.display.id") ?? "--";
            BootloaderStatus = await ExecuteShellCommand("getprop ro.bootloader") ?? "--";
            NDKVersion = await ExecuteShellCommand("getprop ro.build.version.release") ?? "--";

            // 获取电池信息
            await RefreshBatteryInfoAsync();

            // 获取内存信息  
            await RefreshMemoryInfoAsync();

            // 获取存储信息
            await RefreshStorageInfoAsync();

            // 获取运行时间
            await RefreshUptimeAsync();

            LastUpdated = DateTime.Now;
            AddLog($"设备 {SerialNumber} 信息刷新完成", LogLevel.Info);
            return true;
        }
        catch (Exception ex)
        {
            AddLog($"刷新设备 {SerialNumber} 信息时出错：{ex.Message}", LogLevel.Error);
            return false;
        }
    }

    /// <summary>
    /// 执行Shell命令
    /// </summary>
    private async Task<string?> ExecuteShellCommand(string command)
    {
        try
        {
            var receiver = new AdvancedSharpAdbClient.Receivers.ConsoleOutputReceiver();
            await Task.Run(() => Global.AdbClient.ExecuteRemoteCommand(command, _deviceData, receiver));
            return receiver.ToString().Trim();
        }
        catch (Exception ex)
        {
            AddLog($"执行命令 '{command}' 失败：{ex.Message}", LogLevel.Warning);
            return null;
        }
    }

    /// <summary>
    /// 刷新电池信息
    /// </summary>
    private async Task RefreshBatteryInfoAsync()
    {
        try
        {
            var batteryResult = await ExecuteShellCommand("dumpsys battery");

            if (!string.IsNullOrEmpty(batteryResult))
            {
                var lines = batteryResult.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("level:"))
                    {
                        var level = line.Split(':')[1].Trim();
                        BatteryLevel = level;
                    }
                    else if (line.Contains("status:"))
                    {
                        var status = line.Split(':')[1].Trim();
                        BatteryInfo = $"状态: {status}";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            AddLog($"获取电池信息失败：{ex.Message}", LogLevel.Warning);
        }
    }

    /// <summary>
    /// 刷新内存信息
    /// </summary>
    private async Task RefreshMemoryInfoAsync()
    {
        try
        {
            var memResult = await ExecuteShellCommand("cat /proc/meminfo");

            if (!string.IsNullOrEmpty(memResult))
            {
                var lines = memResult.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("MemTotal:"))
                    {
                        var total = ExtractMemoryValue(line);
                        MemoryUsage = $"总内存: {total}";
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            AddLog($"获取内存信息失败：{ex.Message}", LogLevel.Warning);
        }
    }

    /// <summary>
    /// 刷新存储信息
    /// </summary>
    private async Task RefreshStorageInfoAsync()
    {
        try
        {
            var dfResult = await ExecuteShellCommand("df /data");

            if (!string.IsNullOrEmpty(dfResult))
            {
                var lines = dfResult.Split('\n');
                if (lines.Length > 1)
                {
                    var dataLine = lines[1];
                    var parts = dataLine.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 5)
                    {
                        var total = parts[1];
                        var used = parts[2];
                        var usePercent = parts[4];

                        DiskInfo = $"已用: {used} / 总计: {total}";
                        DiskProgress = usePercent.Replace("%", "");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            AddLog($"获取存储信息失败：{ex.Message}", LogLevel.Warning);
        }
    }

    /// <summary>
    /// 刷新运行时间
    /// </summary>
    private async Task RefreshUptimeAsync()
    {
        try
        {
            var uptimeResult = await ExecuteShellCommand("cat /proc/uptime");

            if (!string.IsNullOrEmpty(uptimeResult))
            {
                var uptimeSeconds = double.Parse(uptimeResult.Split(' ')[0]);
                var uptime = TimeSpan.FromSeconds(uptimeSeconds);
                PowerOnTime = $"{uptime.Days}天 {uptime.Hours}小时 {uptime.Minutes}分钟";
            }
        }
        catch (Exception ex)
        {
            AddLog($"获取运行时间失败：{ex.Message}", LogLevel.Warning);
        }
    }

    /// <summary>
    /// 提取内存值
    /// </summary>
    private static string ExtractMemoryValue(string line)
    {
        var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            var value = long.Parse(parts[1]);
            return $"{value / 1024 / 1024} MB";
        }
        return "--";
    }

    /// <summary>
    /// 获取应用列表
    /// </summary>
    /// <returns></returns>
    public override async Task<List<string>> GetApplicationListAsync()
    {
        try
        {
            var result = await ExecuteShellCommand("pm list packages");

            if (!string.IsNullOrEmpty(result))
            {
                return [.. result.Split('\n')
                    .Where(line => line.StartsWith("package:"))
                    .Select(line => line.Replace("package:", "").Trim())];
            }
        }
        catch (Exception ex)
        {
            AddLog($"获取应用列表失败：{ex.Message}", LogLevel.Error);
        }

        return [];
    }

    /// <summary>
    /// 重启到指定模式
    /// </summary>
    /// <param name="mode">目标模式</param>
    /// <returns></returns>
    public override async Task<bool> RebootToModeAsync(DeviceMode mode)
    {
        try
        {
            var command = mode switch
            {
                DeviceMode.Fastboot => "reboot bootloader",
                DeviceMode.Recovery => "reboot recovery",
                DeviceMode.EDL => "reboot edl",
                DeviceMode.Sideload => "reboot sideload",
                DeviceMode.Adb => "reboot",
                DeviceMode.Unknown => throw new ArgumentException($"不支持的重启模式: {mode}"),
                DeviceMode.Mode9008 => throw new ArgumentException($"不支持的重启模式: {mode}"),
                _ => throw new ArgumentException($"不支持的重启模式: {mode}")
            };

            await ExecuteShellCommand(command);
            AddLog($"设备 {SerialNumber} 正在重启到 {mode} 模式", LogLevel.Info);
            return true;
        }
        catch (Exception ex)
        {
            AddLog($"重启设备到 {mode} 模式失败：{ex.Message}", LogLevel.Error);
            return false;
        }
    }

    /// <summary>
    /// 关机
    /// </summary>
    /// <returns></returns>
    public override async Task<bool> PowerOffAsync()
    {
        try
        {
            await ExecuteShellCommand("reboot -p");
            AddLog($"设备 {SerialNumber} 正在关机", LogLevel.Info);
            return true;
        }
        catch (Exception ex)
        {
            AddLog($"关机失败：{ex.Message}", LogLevel.Error);
            return false;
        }
    }

    /// <summary>
    /// 获取设备支持的操作
    /// </summary>
    /// <returns></returns>
    public override List<string> GetSupportedOperations()
    {
        return
        [
            "刷新设备信息",
            "获取应用列表",
            "重启到系统",
            "重启到Recovery",
            "重启到Bootloader",
            "重启到EDL",
            "重启到Sideload",
            "关机",
            "安装应用",
            "卸载应用",
            "执行Shell命令",
            "文件传输",
            "截屏",
            "录屏"
        ];
    }

    /// <summary>
    /// 检查设备连接状态
    /// </summary>
    /// <returns></returns>
    public override async Task<bool> CheckConnectionAsync()
    {
        try
        {
            var devices = await Task.Run(Global.AdbClient.GetDevices);
            var device = devices.FirstOrDefault(d => d.Serial == SerialNumber);

            var wasConnected = IsConnected;
            IsConnected = device != null && device.State == DeviceState.Online;

            if (wasConnected != IsConnected)
            {
                var newStatus = IsConnected ? "已连接" : "已断开";
                OnStatusChanged(Status, newStatus);
                Status = newStatus;
            }

            return IsConnected;
        }
        catch (Exception ex)
        {
            AddLog($"检查设备连接状态失败：{ex.Message}", LogLevel.Error);
            return false;
        }
    }
}
