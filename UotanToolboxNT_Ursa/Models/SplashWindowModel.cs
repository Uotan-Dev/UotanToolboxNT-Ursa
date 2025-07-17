namespace UotanToolboxNT_Ursa.Models;

public class SplashModel
{
    public string StatusText = "工具箱正在启动...";

    /// <summary>
    /// 工具箱初始化逻辑，SplashWindow 显示时调用。
    /// </summary>
    public void Initialize()
    {
        StatusText = "正在检查ADB Server...";
        PerformADBCheck();
        StatusText = "正在枚举设备...";
        PerformHardwareCheck();
        StatusText = "正在加载设置...";
        LoadConfiguration();
        StatusText = "初始化完成";
    }

    /// <summary>
    /// 检查ADB Server是否可用，不可用则创建ADB Server进程。
    /// </summary>
    private void PerformADBCheck()
    {
        try
        {

        }
        catch
        {

        }
    }

    /// <summary>
    /// 检查用户详细硬件信息
    /// </summary>
    private void PerformHardwareCheck()
    {

    }


    /// <summary>
    /// 加载配置文件，若无则创建默认配置。
    /// </summary>
    private void LoadConfiguration()
    {

    }
}