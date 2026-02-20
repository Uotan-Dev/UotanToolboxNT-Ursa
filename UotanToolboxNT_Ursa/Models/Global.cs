using System;
using System.IO;
using AdvancedSharpAdbClient;
using Hardware.Info;
using UotanToolboxNT_Ursa.Models.DeviceCore;

namespace UotanToolboxNT_Ursa.Models;

internal class Global
{
    private static AdbClient? _adbClient;
    public static AdbClient AdbClient => _adbClient ??= new AdbClient(); //工具箱ADB客户端实例

    private static AdbServer? _adbServer;
    public static AdbServer AdbServer => _adbServer ??= new AdbServer(); //工具箱ADB服务实例

    private static HardwareInfo? _hardwareInfo;
    public static HardwareInfo HardwareInfo
    {
        get
        {
            if (_hardwareInfo == null)
            {
                try
                {
                    _hardwareInfo = new HardwareInfo(timeoutInWMI: TimeSpan.FromMilliseconds(1000));
                    // 尝试刷新以便验证是否可用
                    _hardwareInfo.RefreshMemoryStatus();
                }
                catch (Exception ex)
                {
                    // 如果 AOT 环境下依然失败，退而求其次
                    try
                    {
                        _hardwareInfo = new HardwareInfo();
                    }
                    catch
                    {
                        // 这几乎不应发生，但作为最后手段
                        _hardwareInfo = null!; 
                    }
                }
            }
            return _hardwareInfo;
        }
    }

    private static DeviceManager? _deviceManager;
    public static DeviceManager DeviceManager => _deviceManager ??= new DeviceManager(); //设备管理器实例

    // 使用属性代替静态字段，以确保障碍能够被延迟并更容易捕获错误
    private static string _basePath = AppContext.BaseDirectory ?? AppDomain.CurrentDomain.BaseDirectory ?? ".";

    // 对于 Windows，我们尝试在程序运行目录读写（便携模式优先）
    // 对于 HarmonyOS/Android/Linux/MacOS 等系统，BaseDirectory 经常是只读的，或者是受限的，我们强制使用 LocalApplicationData
    // 增加路径分隔符判断，以防一些环境错误报告 OS 平台
    private static string _writablePath = (OperatingSystem.IsWindows() && Path.DirectorySeparatorChar == '\\') 
        ? _basePath 
        : GetBestUnixWritablePath();

    private static string GetBestUnixWritablePath()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrEmpty(path))
        {
            path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
        if (string.IsNullOrEmpty(path))
        {
            path = "/data/local/tmp";
        }
        if (!Directory.Exists(path))
        {
             path = Path.GetTempPath();
        }
        return Path.Combine(path, "UotanToolbox");
    }

    public static DirectoryInfo BaseDirectory { get; } = new(_basePath);

    public static DirectoryInfo TempDirectory { get; } = new(Path.GetTempPath());

    public static DirectoryInfo BinDirectory { get; } = new(Path.Combine(_basePath, "Bin"));

    public static DirectoryInfo ImageDirectory { get; } = new(Path.Combine(_basePath, "Image"));

    public static DirectoryInfo DriveDirectory { get; } = new(Path.Combine(_basePath, "Drive"));

    public static DirectoryInfo PushDirectory { get; } = new(Path.Combine(_basePath, "Push"));

    public static DirectoryInfo LogDirectory { get; } = new(Path.Combine(_writablePath, "Logs"));

    public static FileInfo SettingsFile { get; } = new(Path.Combine(_writablePath, "settings.json"));

    public static FileInfo LatestLogFile { get; } = new(Path.Combine(Path.Combine(_writablePath, "Logs"), "latest.log"));
}
