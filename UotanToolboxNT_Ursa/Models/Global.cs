using System;
using System.IO;
using AdvancedSharpAdbClient;
using Hardware.Info;
using UotanToolboxNT_Ursa.Models.DeviceCore;

namespace UotanToolboxNT_Ursa.Models;

internal class Global
{
    public static AdbClient AdbClient = new(); //工具箱ADB客户端实例

    public static AdbServer AdbServer = new(); //工具箱ADB服务实例

    public static HardwareInfo HardwareInfo = new(timeoutInWMI: TimeSpan.FromMilliseconds(1000)); //设备查询超时设置为1000ms，避免21s的WMI首次调用初始化问题

    public static DeviceManager DeviceManager = new(); //设备管理器实例

    public static DirectoryInfo? BaseDirectory = new(AppDomain.CurrentDomain.BaseDirectory);//工具箱根目录

    public static DirectoryInfo TempDirectory = new(Path.GetTempPath());//系统临时目录

    public static DirectoryInfo BinDirectory = new(Path.Join(BaseDirectory.FullName, "Bin"));//工具箱二进制目录

    public static DirectoryInfo ImageDirectory = new(Path.Join(BaseDirectory.FullName, "Image"));//工具箱镜像目录

    public static DirectoryInfo DriveDirectory = new(Path.Join(BaseDirectory.FullName, "Drive"));//工具箱驱动文件目录

    public static DirectoryInfo PushDirectory = new(Path.Join(BaseDirectory.FullName, "Push"));//工具箱驱动文件目录

    public static DirectoryInfo LogDirectory = new(Path.Join(BaseDirectory.FullName, "Logs"));//工具箱日志目录

    public static FileInfo SettingsFile = new(Path.Join(BaseDirectory.FullName, "settings.json"));//工具箱配置文件信息

    public static FileInfo LatestLogFile = new(Path.Join(LogDirectory.FullName, "latest.log"));//工具箱日志文件信息


}
