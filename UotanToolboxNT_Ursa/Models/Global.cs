using System;
using System.IO;
using Hardware.Info;

namespace UotanToolboxNT_Ursa.Models;
internal class Global
{
    public static HardwareInfo HardwareInfo = new(timeoutInWMI: TimeSpan.FromMilliseconds(1000)); //设备查询超时设置为1000ms，避免21s的WMI首次调用初始化问题

    public static DirectoryInfo? BaseDirectory = new(AppDomain.CurrentDomain.BaseDirectory);//工具箱根目录

    public static DirectoryInfo TempDirectory = new(Path.GetTempPath());//系统临时目录

    public static DirectoryInfo BinDirectory = new(Path.Join(BaseDirectory.FullName, "bin"));//工具箱二进制目录

}
