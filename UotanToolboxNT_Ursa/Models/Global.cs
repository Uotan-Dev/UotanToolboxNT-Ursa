using System;
using System.IO;
using AdvancedSharpAdbClient;
using Hardware.Info;

namespace UotanToolboxNT_Ursa.Models;
internal class Global
{
    public static AdbClient AdbClient = new(); //工具箱ADB客户端实例

    public static AdbServer AdbServer = new(); //工具箱ADB服务实例

    public static HardwareInfo HardwareInfo = new(timeoutInWMI: TimeSpan.FromMilliseconds(1000)); //设备查询超时设置为1000ms，避免21s的WMI首次调用初始化问题

    public static DirectoryInfo? BaseDirectory = new(AppDomain.CurrentDomain.BaseDirectory);//工具箱根目录

    public static DirectoryInfo TempDirectory = new(Path.GetTempPath());//系统临时目录

    public static DirectoryInfo BinDirectory = new(Path.Join(BaseDirectory.FullName, "Bin"));//工具箱二进制目录

}
