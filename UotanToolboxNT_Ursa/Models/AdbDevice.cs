using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
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
    /// 清理字符串，移除换行符和回车符
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <returns>清理后的字符串</returns>
    private static string CleanString(string? input) =>
        string.IsNullOrEmpty(input) ? "--" : input.Replace("\r", "").Replace("\n", "").Trim();

    /// <summary>
    /// 刷新完整设备信息（所有属性）
    /// </summary>
    protected override async Task<bool> RefreshFullDeviceInfoAsync()
    {
        try
        {
            if (!IsConnected)
            {
                return false;
            }

            AddLog($"正在完整刷新设备 {SerialNumber} 的信息...", LogLevel.Info);
            //foreach (var i in Global.AdbClient.GetDevices())
            //{
            //    foreach (var item in Global.AdbClient.GetProperties(i))
            //   {
            //       AddLog(item.Key + ":" + item.Value);
            //   }
            //
            //}
            // 使用shell命令获取设备属性（静态信息）
            Brand = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.product.brand"));
            Model = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.product.model"));
            CodeName = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.product.device"));
            SystemSDK = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.build.version.sdk"));
            CPUABI = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.product.cpu.abi"));
            Platform = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.board.platform"));
            Compile = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.system.build.version.incremental")) ??
                     CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.build.display.id"));

            // Bootloader状态获取
            BootloaderStatus = await GetBootloaderStatusAsync();

            // Android版本获取
            var androidVersion = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.build.version.release"));
            var apiLevel = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.build.version.sdk"));
            if (apiLevel != "--" && int.TryParse(apiLevel, out var api))
            {
                var versionName = GetAndroidVersionName(api);
                SystemSDK = $"Android {androidVersion}{versionName}";
            }
            else
            {
                SystemSDK = $"Android {androidVersion}";
            }

            CPUCode = GetCpuModelName();
            // 获取屏幕分辨率
            DisplayHW = await GetScreenResolutionAsync();

            // 获取屏幕密度
            Density = await GetScreenDensityAsync();

            // 获取主板ID  
            BoardID = await GetBoardIdAsync();

            // 内核版本
            Kernel = CleanString(await ExecuteShellCommand("uname -r"));

            // 获取VAB状态和启动槽位信息
            await RefreshVABAndSlotInfoAsync();

            // 获取磁盘类型（相对静态）
            DiskType = await GetDiskTypeAsync();

            // 获取VNDK版本
            NDKVersion = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.vndk.version"));

            // 获取动态信息
            await RefreshDynamicInfoAsync();

            LastUpdated = DateTime.Now;
            AddLog($"设备 {SerialNumber} 完整信息刷新完成", LogLevel.Info);
            return true;
        }
        catch (Exception ex)
        {
            AddLog($"完整刷新设备 {SerialNumber} 信息时出错：{ex.Message}", LogLevel.Error);
            return false;
        }
    }

    /// <summary>
    /// 刷新动态设备信息（仅刷新变化频繁的属性）
    /// </summary>
    protected override async Task<bool> RefreshDynamicDeviceInfoAsync()
    {
        try
        {
            if (!IsConnected)
            {
                return false;
            }

            AddLog($"正在增量刷新设备 {SerialNumber} 的动态信息...", LogLevel.Debug);

            // 仅刷新动态信息
            await RefreshDynamicInfoAsync();

            LastUpdated = DateTime.Now;
            AddLog($"设备 {SerialNumber} 动态信息刷新完成", LogLevel.Debug);
            return true;
        }
        catch (Exception ex)
        {
            AddLog($"增量刷新设备 {SerialNumber} 信息时出错：{ex.Message}", LogLevel.Error);
            return false;
        }
    }

    /// <summary>
    /// 刷新动态信息的公共方法
    /// </summary>
    private async Task RefreshDynamicInfoAsync()
    {
        // 获取电池信息
        await RefreshBatteryInfoAsync();

        // 获取内存使用情况
        await RefreshMemoryInfoAsync();

        // 获取存储信息
        await RefreshStorageInfoAsync();

        // 获取运行时间
        await RefreshUptimeAsync();
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
            var result = receiver.ToString().Trim();
            return CleanString(result);
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
                string level = "0", status = "--", health = "--", temp = "--";

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith("level:"))
                    {
                        level = trimmedLine.Split(':')[1].Trim();
                    }
                    else if (trimmedLine.StartsWith("status:"))
                    {
                        var statusCode = trimmedLine.Split(':')[1].Trim();
                        status = ConvertBatteryStatus(statusCode);
                    }
                    else if (trimmedLine.StartsWith("health:"))
                    {
                        var healthCode = trimmedLine.Split(':')[1].Trim();
                        health = ConvertBatteryHealth(healthCode);
                    }
                    else if (trimmedLine.StartsWith("temperature:"))
                    {
                        var tempValue = trimmedLine.Split(':')[1].Trim();
                        if (int.TryParse(tempValue, out var tempInt))
                        {
                            temp = $"{tempInt / 10.0:F1}°C";
                        }
                    }
                }

                BatteryLevel = level;
                BatteryInfo = $"状态: {status}" + Environment.NewLine +
                             $"温度: {temp}";
            }
        }
        catch (Exception ex)
        {
            AddLog($"获取电池信息失败：{ex.Message}", LogLevel.Warning);
        }
    }

    /// <summary>
    /// 刷新内存信息（包含使用率等动态信息）
    /// </summary>
    private async Task RefreshMemoryInfoAsync()
    {
        try
        {
            var memResult = await ExecuteShellCommand("cat /proc/meminfo");
            if (!string.IsNullOrEmpty(memResult))
            {
                var memoryValues = ParseMemoryInfo(memResult);
                // 确保有MemTotal值且能正确解析
                if (memoryValues.Length >= 1 && !string.IsNullOrEmpty(memoryValues[0]) && 
                    long.TryParse(memoryValues[0], out var memTotal) && memTotal > 0)
                {
                    long memAvailable = 0;
                    // 尝试获取MemAvailable，如果没有则设为0
                    if (memoryValues.Length >= 2 && !string.IsNullOrEmpty(memoryValues[1]) && 
                        long.TryParse(memoryValues[1], out var available))
                    {
                        memAvailable = available;
                    }

                    var memUsed = memTotal - memAvailable;
                    var usagePercent = (int)(memUsed * 100 / memTotal);

                    MemoryUsage = $"{memUsed / 1024.0 / 1024.0:F1} GB/{memTotal / 1024.0 / 1024.0:F1} GB";
                    MemoryLevel = usagePercent.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            AddLog($"获取内存信息失败：{ex.Message}", LogLevel.Warning);
        }
    }

    /// <summary>
    /// 解析内存信息，提取MemTotal和MemAvailable的数值
    /// </summary>
    /// <param name="info">内存信息字符串</param>
    /// <returns>包含内存数值的数组，[0]为MemTotal，[1]为MemAvailable</returns>
    private static string[] ParseMemoryInfo(string info)
    {
        var memTotal = "";
        var memAvailable = "";
        var lines = info.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var line in lines)
        {
            if (line.Contains("MemTotal:"))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    // 获取倒数第二个元素（数值部分）
                    memTotal = parts[^2];
                }
            }
            else if (line.Contains("MemAvailable:"))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    // 获取倒数第二个元素（数值部分）
                    memAvailable = parts[^2];
                }
            }
        }
        
        // 确保返回的数组顺序固定：[0]为MemTotal，[1]为MemAvailable
        return [memTotal, memAvailable];
    }

    /// <summary>
    /// 刷新存储信息
    /// </summary>
    private async Task RefreshStorageInfoAsync()
    {
        try
        {
            var dfResult = await ExecuteShellCommand("df /data");

            if (string.IsNullOrEmpty(dfResult) || !dfResult.Contains("/data"))
            {
                dfResult = await ExecuteShellCommand("df /storage/emulated");
                if (string.IsNullOrEmpty(dfResult) || !dfResult.Contains("/storage"))
                {
                    dfResult = await ExecuteShellCommand("df /sdcard");
                }
            }

            if (!string.IsNullOrEmpty(dfResult))
            {
                var lines = dfResult.Split('\n');
                if (lines.Length > 1)
                {
                    for (var i = 1; i < lines.Length; i++)
                    {
                        var dataLine = lines[i].Trim();
                        if (string.IsNullOrEmpty(dataLine))
                        {
                            continue;
                        }

                        var parts = dataLine.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 5)
                        {
                            try
                            {
                                var total = long.Parse(parts[1]);
                                var used = long.Parse(parts[2]);
                                var usePercent = parts[4].Replace("%", "");

                                var (totalFormatted, totalUnit) = FormatStorageSize(total * 1024);
                                var (usedFormatted, usedUnit) = FormatStorageSize(used * 1024);

                                DiskInfo = $"{usedFormatted:F1} {usedUnit}/{totalFormatted:F1} {totalUnit}";
                                DiskProgress = usePercent;
                                break;
                            }
                            catch
                            {
                                continue;
                            }
                        }
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
    /// 获取应用列表
    /// </summary>
    /// <param name="thirdPartyOnly">是否只获取第三方应用</param>
    /// <returns></returns>
    public override async Task<List<string>> GetApplicationListAsync() => await GetApplicationListAsync(false);

    /// <summary>
    /// 获取应用列表
    /// </summary>
    /// <param name="thirdPartyOnly">是否只获取第三方应用</param>
    /// <returns></returns>
    public async Task<List<string>> GetApplicationListAsync(bool thirdPartyOnly)
    {
        try
        {
            // 根据MD文档，使用不同的命令获取不同类型的应用列表
            var command = thirdPartyOnly ? "pm list packages -3" : "pm list packages";
            var result = await ExecuteShellCommand(command);

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

    /// <summary>
    /// 获取Bootloader状态（支持多种检测方式）
    /// </summary>
    private async Task<string> GetBootloaderStatusAsync()
    {
        try
        {
            // 优先使用ro.secureboot.lockstate
            var lockState = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.secureboot.lockstate"));
            if (!string.IsNullOrEmpty(lockState) && lockState != "--")
            {
                return lockState;
            }

            // 备用方式：检查vbmeta设备状态
            var vbmetaState = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.boot.vbmeta.device_state"));
            if (!string.IsNullOrEmpty(vbmetaState) && vbmetaState != "--")
            {
                return vbmetaState;
            }

            // 传统方式：获取bootloader信息
            var bootloader = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.bootloader"));
            return bootloader;
        }
        catch
        {
            return "--";
        }
    }

    /// <summary>
    /// 获取屏幕密度（支持多种获取方式）
    /// </summary>
    private async Task<string> GetScreenDensityAsync()
    {
        try
        {
            var wmDensity = await ExecuteShellCommand("wm density");
            if (!string.IsNullOrEmpty(wmDensity) && wmDensity.Contains("dpi"))
            {
                return wmDensity.Replace("Physical density: ", "").Trim();
            }

            var densityProp = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.sf.lcd_density"));
            return !string.IsNullOrEmpty(densityProp) && densityProp != "--" ? densityProp + " dpi" : "--";
        }
        catch
        {
            return "--";
        }
    }

    /// <summary>
    /// 获取CPU型号名称
    /// </summary>
    private string GetCpuModelName()
    {
        try
        {
            var receiver = new AdvancedSharpAdbClient.Receivers.ConsoleOutputReceiver();
            Global.AdbClient.ExecuteRemoteCommand("cat /proc/cpuinfo", _deviceData, receiver);
            var cpuInfo = receiver.ToString().Trim();
            AddLog(cpuInfo);
            if (!string.IsNullOrEmpty(cpuInfo))
            {
                var lines = cpuInfo.Split('\n');
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    AddLog(trimmedLine);
                    if (trimmedLine.StartsWith("model name"))
                    {
                        var parts = trimmedLine.Split(':', 2);
                        AddLog(parts[1]);
                        if (parts.Length == 2)
                        {
                            return parts[1].Trim();
                        }
                    }
                }
            }
            return "--";
        }
        catch
        {
            return "--";
        }
    }

    /// <summary>
    /// 获取主板ID（支持多种获取方式）
    /// </summary>
    private async Task<string> GetBoardIdAsync()
    {
        try
        {
            // 方式1：获取设备序列号（Board ID）
            var serialNumber = await ExecuteShellCommand("cat /sys/devices/soc0/serial_number");
            if (!string.IsNullOrEmpty(serialNumber) && serialNumber != "--")
            {
                return serialNumber;
            }

            // 方式2：通过系统属性获取主板信息
            var boardProp = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.product.board"));
            return boardProp;
        }
        catch
        {
            return "--";
        }
    }

    /// <summary>
    /// 转换电池状态代码为可读文本
    /// </summary>
    private static string ConvertBatteryStatus(string statusCode)
    {
        return statusCode switch
        {
            "1" => "未知",
            "2" => "充电中",
            "3" => "放电中",
            "4" => "未充电",
            "5" => "已满",
            _ => statusCode
        };
    }

    /// <summary>
    /// 转换电池健康状态代码为可读文本
    /// </summary>
    private static string ConvertBatteryHealth(string healthCode)
    {
        return healthCode switch
        {
            "1" => "未知",
            "2" => "良好",
            "3" => "过热",
            "4" => "报废",
            "5" => "过压",
            "6" => "故障",
            "7" => "低温",
            _ => healthCode
        };
    }

    /// <summary>
    /// 格式化存储大小，自动选择合适的单位
    /// </summary>
    /// <param name="bytes">字节数</param>
    /// <returns>格式化后的大小和单位</returns>
    private static (double size, string unit) FormatStorageSize(long bytes)
    {
        const int kb = 1024;
        const int mb = kb * 1024;
        const int gb = mb * 1024;
        const long tb = (long)gb * 1024;

        if (bytes >= tb)
        {
            return ((double)bytes / tb, "TB");
        }
        else if (bytes >= gb)
        {
            return ((double)bytes / gb, "GB");
        }
        else if (bytes >= mb)
        {
            return ((double)bytes / mb, "MB");
        }
        else if (bytes >= kb)
        {
            return ((double)bytes / kb, "KB");
        }
        else
        {
            return (bytes, "B");
        }
    }

    /// <summary>
    /// 获取VAB状态和启动槽位信息
    /// </summary>
    private async Task RefreshVABAndSlotInfoAsync()
    {
        try
        {
            // 获取VAB状态
            var vabInfo = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.virtual_ab.enabled"));
            var isVAB = vabInfo == "true";

            // 获取当前启动槽位
            var currentSlot = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.boot.slot_suffix"));

            VABStatus = isVAB && !string.IsNullOrEmpty(currentSlot) && currentSlot != "--" ? $"支持 (当前槽位: {currentSlot})" : isVAB ? "支持" : "不支持";
        }
        catch
        {
            VABStatus = "--";
        }
    }

    /// <summary>
    /// 获取磁盘类型信息
    /// </summary>
    private async Task<string> GetDiskTypeAsync()
    {
        try
        {
            var emmcInfo = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.hardware.emmc"));
            if (!string.IsNullOrEmpty(emmcInfo) && emmcInfo != "--")
            {
                return $"eMMC ({emmcInfo})";
            }

            var ufsInfo = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.hardware.ufs"));
            if (!string.IsNullOrEmpty(ufsInfo) && ufsInfo != "--")
            {
                return $"UFS ({ufsInfo})";
            }
            var storageInfo = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.boot.bootdevice"));
            if (!string.IsNullOrEmpty(storageInfo) && storageInfo != "--")
            {
                if (storageInfo.Contains("ufs"))
                {
                    return "UFS";
                }
                else if (storageInfo.Contains("emmc") || storageInfo.Contains("mmc"))
                {
                    return "eMMC";
                }
            }

            return "--";
        }
        catch
        {
            return "--";
        }
    }

    /// <summary>
    /// 获取屏幕分辨率信息
    /// </summary>
    public async Task<string> GetScreenResolutionAsync()
    {
        try
        {
            var sizeResult = await ExecuteShellCommand("wm size");
            return !string.IsNullOrEmpty(sizeResult) ? sizeResult.Replace("Physical size: ", "").Trim() : "--";
        }
        catch
        {
            return "--";
        }
    }

    /// <summary>
    /// 获取设备IP地址
    /// </summary>
    public async Task<string> GetDeviceIpAsync()
    {
        try
        {
            var ipResult = await ExecuteShellCommand("ip addr show to 0.0.0.0/0 scope global");
            if (!string.IsNullOrEmpty(ipResult))
            {
                var lines = ipResult.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("inet ") && !line.Contains("127.0.0.1"))
                    {
                        var parts = line.Trim().Split(' ');
                        if (parts.Length > 1 && parts[0] == "inet")
                        {
                            return parts[1].Split('/')[0];
                        }
                    }
                }
            }
            return "--";
        }
        catch
        {
            return "--";
        }
    }

    /// <summary>
    /// 截取屏幕截图并保存到设备
    /// </summary>
    /// <param name="fileName">文件名</param>
    public async Task<bool> TakeScreenshotAsync(string fileName)
    {
        try
        {
            var result = await ExecuteShellCommand($"/system/bin/screencap -p /sdcard/{fileName}.png");
            return string.IsNullOrEmpty(result); // 成功时通常没有输出
        }
        catch (Exception ex)
        {
            AddLog($"截图失败：{ex.Message}", LogLevel.Error);
            return false;
        }
    }

    /// <summary>
    /// 根据API级别获取Android版本代号
    /// </summary>
    /// <param name="apiLevel">API级别</param>
    /// <returns>Android版本代号</returns>
    private static string GetAndroidVersionName(int apiLevel)
    {
        return apiLevel switch
        {
            1 => "(Alpha)",
            2 => "(Beta)",
            3 => "(Cupcake)",
            4 => "(Donut)",
            5 or 6 or 7 => "(Eclair)",
            8 => "(Froyo)",
            9 or 10 => "(Gingerbread)",
            11 or 12 or 13 => "(Honeycomb)",
            14 or 15 => "(Ice Cream Sandwich)",
            16 or 17 or 18 => "(Jelly Bean)",
            19 or 20 => "(KitKat)",
            21 or 22 => "(Lollipop)",
            23 => "(Marshmallow)",
            24 or 25 => "(Nougat)",
            26 or 27 => "(Oreo)",
            28 => "(Pie)",
            29 => "(10)",
            30 => "(11)",
            31 or 32 => "(12)",
            33 => "(13)",
            34 => "(14)",
            35 => "(15)",
            _ when apiLevel >= 36 => $"(API {apiLevel})",
            _ => $"(API {apiLevel})"
        };
    }
}
