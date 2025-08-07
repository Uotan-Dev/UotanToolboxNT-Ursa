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
    /// 刷新设备信息
    /// </summary>
    /// <returns></returns>
    public override async Task<bool> RefreshDeviceInfoAsync()
    {
        try
        {
            AddLog($"正在刷新Fastboot设备 {SerialNumber} 的信息...", LogLevel.Info);

            // 获取设备变量
            var variables = await GetFastbootVariables();

            // 更新设备信息
            Brand = variables.GetValueOrDefault("product", "--");
            Model = variables.GetValueOrDefault("product", "--");
            CodeName = variables.GetValueOrDefault("product", "--");
            BootloaderStatus = variables.GetValueOrDefault("unlocked", "--");
            Platform = variables.GetValueOrDefault("platform", "--");

            // Fastboot模式下的特殊信息
            VABStatus = variables.GetValueOrDefault("slot-count", "--");
            BoardID = variables.GetValueOrDefault("hw-revision", "--");

            // 获取当前槽位信息
            var currentSlot = variables.GetValueOrDefault("current-slot", "--");
            if (currentSlot != "--")
            {
                Status = $"Fastboot (Slot: {currentSlot})";
            }

            LastUpdated = DateTime.Now;
            AddLog($"Fastboot设备 {SerialNumber} 信息刷新完成", LogLevel.Info);
            return true;
        }
        catch (Exception ex)
        {
            AddLog($"刷新Fastboot设备 {SerialNumber} 信息时出错：{ex.Message}", LogLevel.Error);
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
            // 获取基本变量列表
            var variableNames = new[]
            {
                "product", "version-bootloader", "version-baseband", "serialno",
                "unlocked", "secure", "slot-count", "current-slot", "platform",
                "hw-revision", "max-download-size", "partition-type", "has-slot"
            };

            foreach (var varName in variableNames)
            {
                var value = await ExecuteFastbootCommand($"getvar {varName}");
                if (!string.IsNullOrEmpty(value))
                {
                    // Fastboot输出格式通常是 "varname: value"
                    var lines = value.Split('\n');
                    foreach (var line in lines)
                    {
                        if (line.Contains($"{varName}:"))
                        {
                            var parts = line.Split(':');
                            if (parts.Length >= 2)
                            {
                                variables[varName] = parts[1].Trim();
                            }
                            break;
                        }
                    }
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
    public override async Task<List<string>> GetApplicationListAsync()
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
        return false;
        //等待重写具体逻辑
        /*
        try
        {
            var command = mode switch
            {
                DeviceMode.Adb => "reboot",
                DeviceMode.Recovery => "reboot recovery",
                DeviceMode.Fastboot => "reboot bootloader",
                DeviceMode.EDL => "reboot edl",
                DeviceMode.Unknown => throw new ArgumentException($"不支持的重启模式: {mode}"),
                DeviceMode.Sideload => throw new ArgumentException($"不支持的重启模式: {mode}"),
                _ => throw new ArgumentException($"不支持的重启模式: {mode}")
            };

            var result = await ExecuteFastbootCommand(command);
            AddLog($"Fastboot设备 {SerialNumber} 正在重启到 {mode} 模式", LogLevel.Info);
            return true;
        }
        catch (Exception ex)
        {
            AddLog($"Fastboot重启到 {mode} 模式失败：{ex.Message}", LogLevel.Error);
            return false;
        }*/
    }

    /// <summary>
    /// 关机（Fastboot模式下重启到系统）
    /// </summary>
    /// <returns></returns>
    public override async Task<bool> PowerOffAsync()
    {
        try
        {
            await ExecuteFastbootCommand("reboot");
            AddLog($"Fastboot设备 {SerialNumber} 正在重启到系统", LogLevel.Info);
            return true;
        }
        catch (Exception ex)
        {
            AddLog($"Fastboot重启失败：{ex.Message}", LogLevel.Error);
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
            "重启到系统",
            "重启到Recovery",
            "重启到Bootloader",
            "重启到EDL",
            "解锁Bootloader",
            "锁定Bootloader",
            "刷写分区",
            "擦除分区",
            "格式化分区",
            "获取变量",
            "设置变量",
            "启动镜像"
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
