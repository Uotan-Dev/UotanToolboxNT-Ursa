using System;
using System.IO;
using AdvancedSharpAdbClient.Models;
using static UotanToolboxNT_Ursa.Models.GlobalLogModel;

namespace UotanToolboxNT_Ursa.Models;

public class SplashWindowModel
{
    public string StatusText = "工具箱正在启动...";

    public void Initialize()
    {
        AddLog("正在检查运行组件...", LogLevel.Info);
        PerformCompentCheck();
        AddLog("正在枚举设备...", LogLevel.Info);
        PerformHardwareCheck();
        AddLog("初始化完成", LogLevel.Info);
    }

    /// <summary>
    /// 检查工具箱各组件是否可用
    /// </summary>
    private void PerformCompentCheck()
    {
        try
        {
            AddLog("检查ADB Server运行状态...", LogLevel.Info);
            if (!File.Exists(Path.Combine(Global.BinDirectory.FullName, "platform-tools", "adb.exe")))
            {
                AddLog("ADB Server未找到！", LogLevel.Info);
                return;
            }
            var result = Global.AdbServer.StartServer(Path.Combine(Global.BinDirectory.FullName, "platform-tools", "adb.exe"), false);
            AddLog(result switch
            {
                StartServerResult.Started => "ADB Server启动完成",
                StartServerResult.AlreadyRunning => "ADB Server正在运行",
                StartServerResult.RestartedOutdatedDaemon => "ADB Server已过时，正在重新创建",
                StartServerResult.Starting => "ADB Server正在启动",
                _ => "未知的ADB Server状态"
            });

        }
        catch (Exception ex)
        {
            AddLog($"检查运行组件时出错！{ex.Message}", LogLevel.Error); ;
        }
    }

    /// <summary>
    /// 检查用户详细硬件信息
    /// </summary>
    private void PerformHardwareCheck()
    {
        try
        {
            AddLog("正在获取硬件信息...", LogLevel.Info);
            Global.HardwareInfo.RefreshOperatingSystem();
            Global.HardwareInfo.RefreshMemoryStatus();
            Global.HardwareInfo.RefreshBIOSList();
            Global.HardwareInfo.RefreshCPUList(false, 500, true);
            Global.HardwareInfo.RefreshMemoryList();
            Global.HardwareInfo.RefreshNetworkAdapterList(false, true);
            Global.HardwareInfo.RefreshSoundDeviceList();
            Global.HardwareInfo.RefreshVideoControllerList();
            //配置文件读取逻辑暂时在这里先放一下
            AddLog("正在读取设置信息...", LogLevel.Info);
            SettingsModel.Load();
        }
        catch (Exception ex)
        {
            AddLog($"获取硬件信息时出错！{ex.Message}", LogLevel.Error);
        }
    }
}