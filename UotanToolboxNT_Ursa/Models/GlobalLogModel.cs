using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CommunityToolkit.Mvvm.Messaging;
using UotanToolboxNT_Ursa.ViewModels;

namespace UotanToolboxNT_Ursa.Models;

public partial class GlobalLogModel
{
    private static readonly Lock LockObject = new();

    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Debug
    }

    static GlobalLogModel()
    {
        if (!Global.LogDirectory.Exists)
        {
            Directory.CreateDirectory(Global.LogDirectory.FullName);
        }
        if (Global.LatestLogFile.Exists)
        {
            var firstLine = File.ReadLines(Global.LatestLogFile.FullName).FirstOrDefault();
            var timestamp = ExtractTimestampFromLog(firstLine) ?? DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var archivedLogFile = Path.Combine(Global.LogDirectory.FullName, $"log_{timestamp}.log");
            File.Move(Global.LatestLogFile.FullName, archivedLogFile);
        }
    }

    public static void AddLog(string message, LogLevel level = LogLevel.Info)
    {
        if (message != string.Empty)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var logEntry = $"[{timestamp}] [{level}] {message}";

            lock (LockObject)
            {
                File.AppendAllText(Global.LatestLogFile.FullName, logEntry + Environment.NewLine);
            }
            UpdateLogContent(logEntry);
        }
    }

    private static void UpdateLogContent(string logEntry) => WeakReferenceMessenger.Default.Send(new LogUpdateMessage(logEntry + Environment.NewLine));

    /// <summary>
    /// 更新硬件信息卡片
    /// </summary>
    public static void UpdateHardwareInfoCards()
    {
        try
        {
            // 卡片1：内存信息
            var memoryInfo = GetMemoryInfo();
            WeakReferenceMessenger.Default.Send(new HardwareInfoUpdateMessage(1, memoryInfo));

            // 卡片2：系统版本
            var systemNetworkInfo = GetSystemInfo();
            WeakReferenceMessenger.Default.Send(new HardwareInfoUpdateMessage(2, systemNetworkInfo));

            // 卡片3：GPU信息
            var gpuInfo = GetGpuInfo();
            WeakReferenceMessenger.Default.Send(new HardwareInfoUpdateMessage(3, gpuInfo));
        }
        catch (Exception ex)
        {
            AddLog($"更新硬件信息卡片失败: {ex.Message}", LogLevel.Error);
        }
    }

    /// <summary>
    /// 获取内存信息
    /// </summary>
    public static string GetMemoryInfo()
    {
        try
        {
            var hardwareInfo = Global.HardwareInfo;
            if (hardwareInfo.MemoryStatus == null)
            {
                return "内存信息\n\n暂无数据";
            }

            var totalMemoryGB = Math.Round(hardwareInfo.MemoryStatus.TotalPhysical / 1024.0 / 1024.0 / 1024.0, 1);
            var availableMemoryGB = Math.Round(hardwareInfo.MemoryStatus.AvailablePhysical / 1024.0 / 1024.0 / 1024.0, 1);
            var usedMemoryGB = Math.Round((hardwareInfo.MemoryStatus.TotalPhysical - hardwareInfo.MemoryStatus.AvailablePhysical) / 1024.0 / 1024.0 / 1024.0, 1);
            var usagePercent = Math.Round((double)(hardwareInfo.MemoryStatus.TotalPhysical - hardwareInfo.MemoryStatus.AvailablePhysical) / hardwareInfo.MemoryStatus.TotalPhysical * 100, 1);

            return $"内存信息\n\n" +
                   $"总内存: {totalMemoryGB} GB\n" +
                   $"已用: {usedMemoryGB} GB\n" +
                   $"可用: {availableMemoryGB} GB\n" +
                   $"使用率: {usagePercent}%";
        }
        catch
        {
            return "内存信息\n\n获取失败";
        }
    }

    /// <summary>
    /// 获取系统版本和网络信息
    /// </summary>
    public static string GetSystemInfo()
    {
        try
        {
            var hardwareInfo = Global.HardwareInfo;
            var systemInfo = "系统\n\n";

            // 系统版本
            if (hardwareInfo.OperatingSystem != null)
            {
                systemInfo += $"系统: {hardwareInfo.OperatingSystem.Name}\n";
                systemInfo += $"版本: {hardwareInfo.OperatingSystem.VersionString}\n\n";
            }
            else
            {
                systemInfo += "系统: 未知\n\n";
            }


            return systemInfo;
        }
        catch
        {
            return "系统 \n\n获取失败";
        }
    }

    /// <summary>
    /// 获取GPU信息
    /// </summary>
    public static string GetGpuInfo()
    {
        try
        {
            var hardwareInfo = Global.HardwareInfo;
            if (hardwareInfo.VideoControllerList?.Count > 0)
            {
                var primaryGpu = hardwareInfo.VideoControllerList[0];
                var gpuInfo = "GPU信息\n";

                gpuInfo += $"显卡: {primaryGpu.Name}\n";

                if (primaryGpu.AdapterRAM > 0)
                {
                    var vramGB = Math.Round(primaryGpu.AdapterRAM / 1024.0 / 1024.0 / 1024.0, 1);
                    gpuInfo += $"显存: {vramGB} GB\n";
                }

                if (!string.IsNullOrEmpty(primaryGpu.DriverVersion))
                {
                    gpuInfo += $"驱动: {primaryGpu.DriverVersion}";
                }

                if (hardwareInfo.VideoControllerList.Count > 1)
                {
                    gpuInfo += $"\n检测到 {hardwareInfo.VideoControllerList.Count} 个显卡";
                }

                return gpuInfo;
            }
            else
            {
                return "GPU信息\n\n未检测到显卡";
            }
        }
        catch
        {
            return "GPU信息\n\n获取失败";
        }
    }

    private static string? ExtractTimestampFromLog(string? logLine)
    {
        if (string.IsNullOrWhiteSpace(logLine))
        {
            return null;
        }
        var match = Timestamp().Match(logLine);
        if (match.Success)
        {
            if (DateTime.TryParse(match.Groups[1].Value, out var dateTime))
            {
                return dateTime.ToString("yyyyMMdd_HHmmss");
            }
        }
        return null;
    }

    [GeneratedRegex(@"\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\]")]
    private static partial Regex Timestamp();
}