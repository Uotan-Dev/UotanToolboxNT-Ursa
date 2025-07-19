using System;
using System.IO;
using AdvancedSharpAdbClient.Models;

namespace UotanToolboxNT_Ursa.Models;

public class SplashWindowModel
{
    public string StatusText = "工具箱正在启动...";

    /// <summary>
    /// 工具箱初始化逻辑，SplashWindow 显示时调用。
    /// </summary>
    public void Initialize()
    {
        StatusText = "正在检查运行组件...";
        PerformCompentCheck();
        StatusText = "正在枚举设备...";
        PerformHardwareCheck();
        StatusText = "正在加载设置...";
        LoadConfiguration();
        StatusText = "初始化完成";
    }

    /// <summary>
    /// 检查工具箱各组件是否可用
    /// </summary>
    private void PerformCompentCheck()
    {
        try
        {
            StatusText = "检查ADB Server运行状态...";
            if (!File.Exists(Path.Combine(Global.BinDirectory.FullName, "platform-tools", "adb.exe")))
            {
                StatusText = "ADB Server未找到！";
                return;
            }
            var result = Global.AdbServer.StartServer(Path.Combine(Global.BinDirectory.FullName, "platform-tools", "adb.exe"), false);
            StatusText = result switch
            {
                StartServerResult.Started => "ADB Server启动完成",
                StartServerResult.AlreadyRunning => "ADB Server正在运行",
                StartServerResult.RestartedOutdatedDaemon => "ADB Server已过时，正在重新创建",
                StartServerResult.Starting => "ADB Server正在启动",
                _ => "未知的ADB Server状态"
            };

        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 检查用户详细硬件信息
    /// </summary>
    private void PerformHardwareCheck()
    {
        try
        {
            StatusText = "正在获取硬件信息...";
            Global.HardwareInfo.RefreshOperatingSystem();
            Global.HardwareInfo.RefreshMemoryStatus();
            Global.HardwareInfo.RefreshBIOSList();
            Global.HardwareInfo.RefreshCPUList(false, 500, true);
            Global.HardwareInfo.RefreshMemoryList();
            Global.HardwareInfo.RefreshNetworkAdapterList(false, true);
            Global.HardwareInfo.RefreshSoundDeviceList();
            Global.HardwareInfo.RefreshVideoControllerList();
        }
        catch (Exception ex)
        {
            StatusText = $"获取硬件信息时出错！{ex.Message}";
        }
    }


    /// <summary>
    /// 加载配置文件，若无则创建默认配置。
    /// </summary>
    private void LoadConfiguration()
    {

    }
}