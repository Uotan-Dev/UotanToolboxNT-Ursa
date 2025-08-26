using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.Models;
using UotanToolboxNT_Ursa.Helper;
using static UotanToolboxNT_Ursa.Models.GlobalLogModel;

namespace UotanToolboxNT_Ursa.Models;

/// <summary>
/// ADB设备类
/// </summary>
public class AdbDevice : DeviceBase
{
    private readonly DeviceData _deviceData;
    private static readonly char[] SeparatorArray = ['\r', '\n'];
    internal static readonly char[] Separator = ['\r', '\n'];
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
    /// 获取翻译文本
    /// </summary>
    /// <param name="key">翻译键</param>
    /// <returns>翻译后的文本</returns>
    private static string GetTranslation(string key) => LanguageResourceHelper.GetLanguageResource<string>(key) ?? key;

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
    private async Task<string?> ExecuteShellCommand(string command, bool cleanString = true)
    {
        try
        {
            var receiver = new AdvancedSharpAdbClient.Receivers.ConsoleOutputReceiver();
            await Task.Run(() => Global.AdbClient.ExecuteRemoteCommand(command, _deviceData, receiver));
            var result = receiver.ToString().Trim();
            return cleanString ? CleanString(result) : result;
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
            var batteryResult = await ExecuteShellCommand("dumpsys battery", false);
            if (!string.IsNullOrEmpty(batteryResult))
            {
                var infos = new string[100];
                var lines = batteryResult.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < lines.Length; i++)
                {
                    if (!lines[i].Contains("Max charging voltage") && !lines[i].Contains("Charger voltage"))
                    {
                        if (lines[i].Contains("level") || lines[i].Contains("voltage") || lines[i].Contains("temperature"))
                        {
                            var device = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            infos[i] = device[^1];
                        }
                    }
                }
                infos = [.. infos.Where(s => !string.IsNullOrEmpty(s))];

                BatteryLevel = infos[0];
                BatteryInfo = string.Format($"{double.Parse(infos[1]) / 1000.0}V {double.Parse(infos[2]) / 10.0}℃");
                //AddLog($"电池信息：{BatteryInfo}，电量：{BatteryLevel}%", LogLevel.Info);
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
            var memResult = await ExecuteShellCommand("cat /proc/meminfo | grep Mem", false);
            if (!string.IsNullOrEmpty(memResult))
            {
                var infos = new string[20];
                var lines = memResult.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("MemTotal") || lines[i].Contains("MemAvailable"))
                    {
                        var device = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        infos[i] = device[^2];
                    }
                }
                infos = [.. infos.Where(s => !string.IsNullOrEmpty(s))];
                var use = double.Parse(infos[0]) - double.Parse(infos[1]);
                MemoryLevel = Math.Round(Math.Round(use * 1.024 / 1000000, 1) / Math.Round(double.Parse(infos[0]) * 1.024 / 1000000) * 100).ToString();
                MemoryUsage = string.Format($"{Math.Round(use * 1.024 / 1000000, 1)}GB/{Math.Round(double.Parse(infos[0]) * 1.024 / 1000000)}GB");
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
            var diskinfos1 = await ExecuteShellCommand("df /storage/emulated", false);
            var diskinfos2 = await ExecuteShellCommand("df /data", false);

            string[]? columns = null;

            if (!string.IsNullOrEmpty(diskinfos1) && diskinfos1.Contains("/storage/emulated"))
            {
                columns = ParseDiskInfo(diskinfos1, "/storage/emulated");
            }
            else if (!string.IsNullOrEmpty(diskinfos2) && diskinfos2.Contains("/sdcard"))
            {
                columns = ParseDiskInfo(diskinfos2, "/sdcard");
            }
            else if (!string.IsNullOrEmpty(diskinfos2) && diskinfos2.Contains("/data"))
            {
                columns = ParseDiskInfo(diskinfos2, "/data");
            }

            if (columns != null && columns.Length >= 5)
            {
                try
                {
                    var used = double.Parse(columns[2]);
                    var total = double.Parse(columns[1]);
                    var progressPercent = columns[4].TrimEnd('%');

                    DiskInfo = $"{used / 1024 / 1024:0.00}GB/{total / 1024 / 1024:0.00}GB";
                    DiskProgress = progressPercent;
                }
                catch (Exception ex)
                {
                    AddLog($"解析存储信息失败：{ex.Message}", LogLevel.Warning);
                }
            }
        }
        catch (Exception ex)
        {
            AddLog($"获取存储信息失败：{ex.Message}", LogLevel.Warning);
        }
    }

    /// <summary>
    /// 解析磁盘信息
    /// </summary>
    /// <param name="info">df命令输出</param>
    /// <param name="find">要查找的挂载点</param>
    /// <returns>解析后的列数组</returns>
    private static string[] ParseDiskInfo(string info, string find)
    {
        var lines = info.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var targetLine = lines.FirstOrDefault(line => line.Contains(find));
        if (targetLine != null)
        {
            var columns = targetLine.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            return [.. columns.Where(s => !string.IsNullOrEmpty(s))];
        }

        return [];
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
                var intptime = int.Parse(uptimeResult.Split('.')[0].Trim());
                var timeSpan = TimeSpan.FromSeconds(intptime);
                PowerOnTime = $"{timeSpan.Days}d{timeSpan.Hours}h{timeSpan.Minutes}m{timeSpan.Seconds}s";
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
    public override async Task<List<ApplicationInfo>> GetApplicationListAsync() => await GetApplicationListAsync(false);

    /// <summary>
    /// 获取应用列表
    /// </summary>
    /// <param name="thirdPartyOnly">是否只获取第三方应用</param>
    /// <returns></returns>
    public async Task<List<ApplicationInfo>> GetApplicationListAsync(bool thirdPartyOnly)
    {
        try
        {
            await PushFileAsync(Path.Combine(Global.PushDirectory.FullName, "list_apps"), "/data/local/tmp/list_apps");
            AddLog($"推送 list_apps 到设备 {SerialNumber} 的 /data/local/tmp/ 目录");
            await ExecuteShellCommand("chmod 0777 /data/local/tmp/list_apps");
            var full_lists = await ExecuteShellCommand("/data/local/tmp/list_apps");
            AddLog(full_lists);
            var applicationInfos = new List<ApplicationInfo>();
            var lines = full_lists.Split(['\n'], StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split([' '], 3, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 3)
                {
                    applicationInfos.Add(new ApplicationInfo
                    {
                        Name = parts[1],
                        DisplayName = parts[2],
                    });
                }
            }

            var command = thirdPartyOnly ? "pm list packages -3" : "pm list packages";
            var result = await ExecuteShellCommand(command);
            return await ProcessAndroidApplications(applicationInfos, result);
        }
        catch (Exception ex)
        {
            AddLog($"获取应用列表失败：{ex.Message}", LogLevel.Error);
        }

        return [];
    }

    private async Task<List<ApplicationInfo>> ProcessAndroidApplications(List<ApplicationInfo> fullapplications, string fullApplicationsList)
    {
        var lines = fullApplicationsList.Split(SeparatorArray, StringSplitOptions.RemoveEmptyEntries);
        var applicationInfosTasks = lines.Select(async line =>
        {
            var displayName = string.Empty;
            var packageName = ExtractPackageName(line);
            foreach (var app in fullapplications)
            {
                if (app.Name == packageName)
                {
                    displayName = app.DisplayName;
                    break;
                }
            }

            if (string.IsNullOrEmpty(packageName))
            {
                return null;
            }
            var combinedOutput = await ExecuteShellCommand($"dumpsys package {packageName}");
            var splitOutput = combinedOutput.Split('\n', ' ');
            var otherInfo = GetVersionName(splitOutput) + " | " + GetInstalledDate(splitOutput) + " | " + GetSdkVersion(splitOutput);

            return new ApplicationInfo
            {
                Name = packageName,
                DisplayName = RemoveLineFeed(displayName),
                OtherInfo = otherInfo
            };
        });
        var allApplicationInfos = await Task.WhenAll(applicationInfosTasks);
        var processedApplications = allApplicationInfos.Where(info => info != null)
                                                       .OrderByDescending(app => app.Size)
                                                       .ThenBy(app => app.Name)
                                                       .ToList();

        return processedApplications;
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
                DeviceMode.Fastbootd => "reboot fastboot",
                DeviceMode.Unknown => throw new ArgumentException($"不支持的重启模式: {mode}"),
                _ => throw new ArgumentException($"未知的重启模式: {mode}")
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
            if (!string.IsNullOrEmpty(cpuInfo))
            {
                var lines = cpuInfo.Split('\n');
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith("model name"))
                    {
                        var parts = trimmedLine.Split(':', 2);
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
    /// 获取主板ID
    /// </summary>
    private async Task<string> GetBoardIdAsync()
    {
        try
        {
            var serialNumber = await ExecuteShellCommand("cat /sys/devices/soc0/serial_number");
            if (!string.IsNullOrEmpty(serialNumber) && serialNumber != "--")
            {
                return serialNumber;
            }

            var boardProp = CleanString(await Global.AdbClient.GetPropertyAsync(_deviceData, "ro.product.board"));
            return boardProp;
        }
        catch
        {
            return "--";
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
    /// 将本地文件推送到设备
    /// </summary>
    /// <param name="localFilePath">本地文件路径</param>
    /// <param name="remoteFilePath">设备目标路径</param>
    /// <returns>操作是否成功</returns>
    public async Task<bool> PushFileAsync(string localFilePath, string remoteFilePath)
    {
        try
        {
            if (!IsConnected)
            {
                AddLog($"设备 {SerialNumber} 未连接，无法推送文件", LogLevel.Error);
                return false;
            }

            if (!File.Exists(localFilePath))
            {
                AddLog($"本地文件不存在：{localFilePath}", LogLevel.Error);
                return false;
            }

            AddLog($"正在将文件推送到设备 {SerialNumber}：{localFilePath} -> {remoteFilePath}", LogLevel.Info);

            using var fileStream = File.OpenRead(localFilePath);
            var syncService = new SyncService(Global.AdbClient, _deviceData);
            await Task.Run(() => syncService.Push(fileStream, remoteFilePath,
                UnixFileStatus.UserRead | UnixFileStatus.UserWrite | UnixFileStatus.GroupRead | UnixFileStatus.OtherRead,
                DateTime.Now, null));

            AddLog($"文件推送成功：{localFilePath} -> {remoteFilePath}", LogLevel.Info);
            return true;
        }
        catch (Exception ex)
        {
            AddLog($"推送文件失败：{ex.Message}", LogLevel.Error);
            return false;
        }
    }

    /// <summary>
    /// 将设备文件拉取到本地
    /// </summary>
    /// <param name="remoteFilePath">设备文件路径</param>
    /// <param name="localFilePath">本地目标路径</param>
    /// <returns>操作是否成功</returns>
    public async Task<bool> PullFileAsync(string remoteFilePath, string localFilePath)
    {
        try
        {
            if (!IsConnected)
            {
                AddLog($"设备 {SerialNumber} 未连接，无法拉取文件", LogLevel.Error);
                return false;
            }

            // 确保本地目录存在
            var localDirectory = Path.GetDirectoryName(localFilePath);
            if (!string.IsNullOrEmpty(localDirectory) && !Directory.Exists(localDirectory))
            {
                Directory.CreateDirectory(localDirectory);
            }

            AddLog($"正在从设备 {SerialNumber} 拉取文件：{remoteFilePath} -> {localFilePath}", LogLevel.Info);

            using var fileStream = File.Create(localFilePath);
            var syncService = new SyncService(Global.AdbClient, _deviceData);
            await Task.Run(() => syncService.Pull(remoteFilePath, fileStream, null));

            AddLog($"文件拉取成功：{remoteFilePath} -> {localFilePath}", LogLevel.Info);
            return true;
        }
        catch (Exception ex)
        {
            AddLog($"拉取文件失败：{ex.Message}", LogLevel.Error);

            // 如果拉取失败，删除可能创建的空文件
            try
            {
                if (File.Exists(localFilePath))
                {
                    File.Delete(localFilePath);
                }
            }
            catch
            {
                // 忽略删除文件时的异常
            }

            return false;
        }
    }    /// <summary>
         /// 推送多个文件到设备
         /// </summary>
         /// <param name="fileMap">文件映射字典 (本地路径 -> 设备路径)</param>
         /// <returns>成功推送的文件数量</returns>
    public async Task<int> PushMultipleFilesAsync(Dictionary<string, string> fileMap)
    {
        if (!IsConnected)
        {
            AddLog($"设备 {SerialNumber} 未连接，无法推送文件", LogLevel.Error);
            return 0;
        }

        var successCount = 0;
        AddLog($"开始批量推送 {fileMap.Count} 个文件到设备 {SerialNumber}", LogLevel.Info);

        foreach (var kvp in fileMap)
        {
            if (await PushFileAsync(kvp.Key, kvp.Value))
            {
                successCount++;
            }
        }

        AddLog($"批量推送完成，成功推送 {successCount}/{fileMap.Count} 个文件", LogLevel.Info);
        return successCount;
    }

    /// <summary>
    /// 拉取多个文件到本地
    /// </summary>
    /// <param name="fileMap">文件映射字典 (设备路径 -> 本地路径)</param>
    /// <returns>成功拉取的文件数量</returns>
    public async Task<int> PullMultipleFilesAsync(Dictionary<string, string> fileMap)
    {
        if (!IsConnected)
        {
            AddLog($"设备 {SerialNumber} 未连接，无法拉取文件", LogLevel.Error);
            return 0;
        }

        var successCount = 0;
        AddLog($"开始批量拉取 {fileMap.Count} 个文件从设备 {SerialNumber}", LogLevel.Info);

        foreach (var kvp in fileMap)
        {
            if (await PullFileAsync(kvp.Key, kvp.Value))
            {
                successCount++;
            }
        }

        AddLog($"批量拉取完成，成功拉取 {successCount}/{fileMap.Count} 个文件", LogLevel.Info);
        return successCount;
    }

    /// <summary>
    /// 检查设备上文件是否存在
    /// </summary>
    /// <param name="remoteFilePath">设备文件路径</param>
    /// <returns>文件是否存在</returns>
    public async Task<bool> FileExistsAsync(string remoteFilePath)
    {
        try
        {
            if (!IsConnected)
            {
                return false;
            }

            var result = await ExecuteShellCommand($"test -f '{remoteFilePath}' && echo 'exists' || echo 'not_exists'");
            return result?.Trim() == "exists";
        }
        catch (Exception ex)
        {
            AddLog($"检查文件是否存在失败：{ex.Message}", LogLevel.Warning);
            return false;
        }
    }

    /// <summary>
    /// 获取设备文件大小
    /// </summary>
    /// <param name="remoteFilePath">设备文件路径</param>
    /// <returns>文件大小（字节），如果文件不存在或获取失败返回-1</returns>
    public async Task<long> GetFileSizeAsync(string remoteFilePath)
    {
        try
        {
            if (!IsConnected)
            {
                return -1;
            }

            var result = await ExecuteShellCommand($"stat -c %s '{remoteFilePath}'");
            return long.TryParse(result?.Trim(), out var size) ? size : -1;
        }
        catch (Exception ex)
        {
            AddLog($"获取文件大小失败：{ex.Message}", LogLevel.Warning);
            return -1;
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
            19 => "(KitKat)",
            20 => "(KitKat Wear)",
            21 or 22 => "(Lollipop)",
            23 => "(Marshmallow)",
            24 or 25 => "(Nougat)",
            26 or 27 => "(Oreo)",
            28 => "(Pie)",
            29 => "(Q)",
            30 => "(R)",
            31 => "(S)",
            32 => "(Sv2)",
            33 => "(Tiramisu)",
            34 => "(UpsideDownCake)",
            35 => "(Vanilla Ice Cream)",
            36 => "(W)",
            _ when apiLevel > 36 => $"(API {apiLevel})",
            _ => $"(API {apiLevel})"
        };
    }
    private static string GetInstalledDate(string[] lines)
    {
        var installedDateLine = lines.FirstOrDefault(x => x.Contains("lastUpdateTime"));
        if (installedDateLine != null)
        {
            var installedDate = installedDateLine[(installedDateLine.IndexOf('=') + 1)..].Trim();
            return installedDate;
        }
        return GetTranslation("Appmgr_UnknownTime");
    }

    private static string GetSdkVersion(string[] lines)
    {
        var sdkVersion = lines.FirstOrDefault(x => x.Contains("targetSdk"));
        if (sdkVersion != null)
        {
            var installedDate = "SDK" + sdkVersion[(sdkVersion.IndexOf('=') + 1)..].Trim();
            return installedDate;
        }
        return GetTranslation("Appmgr_UnknownSDKVersion");
    }

    private static string GetVersionName(string[] lines)
    {
        var versionName = lines.FirstOrDefault(x => x.Contains("versionName"));
        if (versionName != null)
        {
            var installedDate = versionName[(versionName.IndexOf('=') + 1)..].Trim();
            return installedDate;
        }
        return GetTranslation("Appmgr_UnknownAppVersion");
    }
    private static string RemoveLineFeed(string str)
    {
        var lines = str.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var result = string.Concat(lines);
        if (result.Contains("FreeChannelContinue"))
        {
            result = lines[0];
        }
        return string.IsNullOrEmpty(result) || result.Contains("not found") || result.Contains("dialog on your device") || result.Contains("device offline") || result.Contains("closed") || result.Contains("fail!") || result.Contains("Fail") || result.Contains("unauthorized")
                ? "--"
                : result;
    }
    private static string? ExtractPackageName(string line)
    {
        var parts = line.Split(':');
        if (parts.Length < 2)
        {
            return null;
        }

        var packageNamePart = parts[1];
        var packageNameStartIndex = packageNamePart.LastIndexOf('/') + 1;
        return packageNameStartIndex < packageNamePart.Length
            ? packageNamePart[packageNameStartIndex..]
            : null;
    }
}