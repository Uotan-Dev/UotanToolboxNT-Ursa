using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static UotanToolboxNT_Ursa.Models.GlobalLogModel;

namespace UotanToolboxNT_Ursa.Models;

/// <summary>
/// Fastboot设备类
/// </summary>
public class FastbootDevice : DeviceBase
{
    private readonly string _fastbootPath;

    public FastbootDevice(string serialNumber)
    {
        SerialNumber = serialNumber;
        Mode = DeviceMode.Fastboot;
        Status = "Fastboot";
        IsConnected = true;
        _fastbootPath = Path.Combine(Global.BinDirectory.FullName, "platform-tools", "fastboot.exe");
    }

    /// <summary>
    /// 刷新完整设备信息（所有属性）
    /// </summary>
    /// <returns></returns>
    protected override async Task<bool> RefreshFullDeviceInfoAsync()
    {
        try
        {
            AddLog($"正在完整刷新Fastboot设备 {SerialNumber} 的信息...", LogLevel.Info);

            var variables = await GetFastbootVariables();
            var isUserspace = variables.GetValueOrDefault("is-userspace", "no");
            if (string.Equals(isUserspace, "yes", StringComparison.OrdinalIgnoreCase))
            {
                Mode = DeviceMode.Fastbootd;
                Status = "Fastbootd";
            }
            Brand = variables.GetValueOrDefault("product", "--");
            Model = variables.GetValueOrDefault("product", "--");
            CodeName = variables.GetValueOrDefault("product", "--");
            BootloaderStatus = variables.GetValueOrDefault("unlocked", "--");
            Platform = variables.GetValueOrDefault("platform", "--");
            VABStatus = variables.GetValueOrDefault("slot-count", "--");
            BoardID = variables.GetValueOrDefault("hw-revision", "--");
            SystemSDK = variables.GetValueOrDefault("version-baseband", "--");
            Compile = variables.GetValueOrDefault("version-bootloader", "--");
            var currentSlot = variables.GetValueOrDefault("current-slot", "--");
            if (currentSlot != "--")
            {
                Status = $"Fastboot (Slot: {currentSlot})";
            }

            BatteryLevel = "--";
            BatteryInfo = "--";
            MemoryUsage = "--";
            MemoryLevel = "--";
            DiskInfo = "--";
            DiskProgress = "--";
            PowerOnTime = "--";

            LastUpdated = DateTime.Now;
            AddLog($"Fastboot设备 {SerialNumber} 完整信息刷新完成", LogLevel.Info);
            return true;
        }
        catch (Exception ex)
        {
            AddLog($"完整刷新Fastboot设备 {SerialNumber} 信息时出错：{ex.Message}", LogLevel.Error);
            return false;
        }
    }

    /// <summary>
    /// 刷新动态设备信息（Fastboot模式下大部分动态信息不可用）
    /// </summary>
    /// <returns></returns>
    protected override async Task<bool> RefreshDynamicDeviceInfoAsync()
    {
        try
        {
            AddLog($"正在增量刷新Fastboot设备 {SerialNumber} 的动态信息...", LogLevel.Debug);

            var variables = await GetFastbootVariables();
            var isUserspace = variables.GetValueOrDefault("is-userspace", "no");
            if (string.Equals(isUserspace, "yes", StringComparison.OrdinalIgnoreCase))
            {
                Mode = DeviceMode.Fastbootd;
                Status = "Fastbootd";
            }
            var currentSlot = variables.GetValueOrDefault("current-slot", "--");
            if (currentSlot != "--")
            {
                Status = $"Fastboot (Slot: {currentSlot})";
            }

            LastUpdated = DateTime.Now;
            AddLog($"Fastboot设备 {SerialNumber} 动态信息刷新完成", LogLevel.Debug);
            return true;
        }
        catch (Exception ex)
        {
            AddLog($"增量刷新Fastboot设备 {SerialNumber} 信息时出错：{ex.Message}", LogLevel.Error);
            return false;
        }
    }

    /// <summary>
    /// 获取Fastboot变量
    /// </summary>
    private async Task<Dictionary<string, string>> GetFastbootVariables()
    {
        var variables = new Dictionary<string, string>();

        try
        {
            var variableNames = new[]
            {
                "product", "version-bootloader", "version-baseband", "serialno",
                "unlocked", "secure", "slot-count", "current-slot", "platform",
                "hw-revision", "max-download-size", "partition-type", "has-slot",
                "is-userspace"
            };

            foreach (var varName in variableNames)
            {
                var value = await ExecuteFastbootCommand($"getvar {varName}");
                if (string.IsNullOrEmpty(value))
                {
                    if (varName == "is-userspace")
                    {
                        variables[varName] = "no";
                    }
                    continue;
                }

                var lines = value.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
                var found = false;
                foreach (var line in lines)
                {
                    if (line.Contains($"{varName}:", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = line.Split(':');
                        if (parts.Length >= 2)
                        {
                            var parsed = parts[1].Trim();
                            variables[varName] = varName == "is-userspace" ? string.Equals(parsed, "yes", StringComparison.OrdinalIgnoreCase) ? "yes" : "no" : parsed;
                        }
                        found = true;
                        break;
                    }
                }

                if (!found && varName == "is-userspace")
                {
                    var trimmed = value.Trim();
                    variables[varName] = string.Equals(trimmed, "yes", StringComparison.OrdinalIgnoreCase) ? "yes" : "no";
                }
            }
        }
        catch (Exception ex)
        {
            AddLog($"获取Fastboot变量失败：{ex.Message}", LogLevel.Warning);
        }

        return variables;
    }

    /// <summary>
    /// 执行Fastboot命令
    /// </summary>
    private async Task<string> ExecuteFastbootCommand(string arguments)
    {
        try
        {
            using var process = new Process();
            process.StartInfo.FileName = _fastbootPath;
            process.StartInfo.Arguments = $"-s {SerialNumber} {arguments}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            // Fastboot输出通常在stderr中
            return !string.IsNullOrEmpty(error) ? error : output;
        }
        catch (Exception ex)
        {
            AddLog($"执行Fastboot命令失败：{ex.Message}", LogLevel.Warning);
            return string.Empty;
        }
    }

    /// <summary>
    /// 获取应用列表（Fastboot模式不支持）
    /// </summary>
    /// <returns></returns>
    public override async Task<List<ApplicationInfo>> GetApplicationListAsync()
    {
        await Task.CompletedTask;
        AddLog("Fastboot模式不支持获取应用列表", LogLevel.Warning);
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
            if (mode is DeviceMode.Adb)
            {
                await ExecuteFastbootCommand("reboot");
            }
            else if (mode is DeviceMode.Recovery)
            {

                var output = await ExecuteFastbootCommand($"oem reboot-recovery");
                if (output.Contains("unknown command"))
                {
                    await ExecuteFastbootCommand($"flash misc \"{Path.Combine(Global.ImageDirectory.FullName, "misc.img")}\"");
                    await ExecuteFastbootCommand("reboot");
                }
                else
                {
                    await ExecuteFastbootCommand("reboot recovery");
                }
            }
            else if (mode is DeviceMode.Fastboot or DeviceMode.Fastbootd)
            {
                await ExecuteFastbootCommand("reboot-fastboot");
            }
            else if (mode is DeviceMode.EDL)
            {
                await ExecuteFastbootCommand("oem edl");
            }
            else if (mode is DeviceMode.Sideload)
            {
                throw new ArgumentException($"不支持的重启模式: {mode}");
            }

            AddLog($"已向设备 {SerialNumber} 发送重启命令：{mode}", LogLevel.Info);
            return true;
        }
        catch (Exception ex)
        {
            AddLog($"重启设备到模式 {mode} 失败：{ex.Message}", LogLevel.Error);
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
            var output = await ExecuteFastbootCommand("oem poweroff");
            if (output.Contains("unknown command"))
            {
                AddLog($"不支持在Fastboot模式下关机", LogLevel.Error);
                return false;
            }
            AddLog($"Fastboot设备 {SerialNumber} 正在关闭电源", LogLevel.Info);
            return true;
        }
        catch (Exception ex)
        {
            AddLog($"Fastboot关机失败：{ex.Message}", LogLevel.Error);
            return false;
        }
    }

    /// <summary>
    /// 检查设备连接状态
    /// </summary>
    /// <returns></returns>
    public override async Task<bool> CheckConnectionAsync()
    {
        try
        {
            var result = await ExecuteFastbootCommand("devices");
            var wasConnected = IsConnected;

            IsConnected = !string.IsNullOrEmpty(result) && result.Contains(SerialNumber);

            if (wasConnected != IsConnected)
            {
                var newStatus = IsConnected ? "Fastboot已连接" : "Fastboot已断开";
                OnStatusChanged(Status, newStatus);
                Status = newStatus;
            }

            return IsConnected;
        }
        catch (Exception ex)
        {
            AddLog($"检查Fastboot设备连接状态失败：{ex.Message}", LogLevel.Error);
            return false;
        }
    }

    /// <summary>
    /// 解锁Bootloader
    /// </summary>
    /// <returns></returns>
    public async Task<bool> UnlockBootloaderAsync()
    {
        try
        {
            var result = await ExecuteFastbootCommand("oem unlock");
            AddLog($"Fastboot设备 {SerialNumber} Bootloader解锁命令已执行", LogLevel.Info);
            return true;
        }
        catch (Exception ex)
        {
            AddLog($"解锁Bootloader失败：{ex.Message}", LogLevel.Error);
            return false;
        }
    }

    /// <summary>
    /// 锁定Bootloader
    /// </summary>
    /// <returns></returns>
    public async Task<bool> LockBootloaderAsync()
    {
        try
        {
            var result = await ExecuteFastbootCommand("oem lock");
            AddLog($"Fastboot设备 {SerialNumber} Bootloader锁定命令已执行", LogLevel.Info);
            return true;
        }
        catch (Exception ex)
        {
            AddLog($"锁定Bootloader失败：{ex.Message}", LogLevel.Error);
            return false;
        }
    }

    /// <summary>
    /// 刷写分区
    /// </summary>
    /// <param name="partition">分区名称</param>
    /// <param name="imagePath">镜像文件路径</param>
    /// <returns></returns>
    public async Task<bool> FlashPartitionAsync(string partition, string imagePath)
    {
        try
        {
            if (!File.Exists(imagePath))
            {
                AddLog($"镜像文件不存在：{imagePath}", LogLevel.Error);
                return false;
            }

            var result = await ExecuteFastbootCommand($"flash {partition} \"{imagePath}\"");
            AddLog($"已向分区 {partition} 刷写镜像：{imagePath}", LogLevel.Info);
            return true;
        }
        catch (Exception ex)
        {
            AddLog($"刷写分区 {partition} 失败：{ex.Message}", LogLevel.Error);
            return false;
        }
    }
}
