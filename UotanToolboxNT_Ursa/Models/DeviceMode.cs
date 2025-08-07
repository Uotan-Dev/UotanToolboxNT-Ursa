namespace UotanToolboxNT_Ursa.Models;

/// <summary>
/// 设备连接模式枚举
/// </summary>
public enum DeviceMode
{
    /// <summary>
    /// 未连接或未知
    /// </summary>
    Unknown,

    /// <summary>
    /// ADB模式（Android Debug Bridge）
    /// </summary>
    Adb,

    /// <summary>
    /// Fastboot模式
    /// </summary>
    Fastboot,

    /// <summary>
    /// EDL模式（Emergency Download）
    /// </summary>
    EDL,

    /// <summary>
    /// 9008模式
    /// </summary>
    Mode9008,

    /// <summary>
    /// Recovery模式
    /// </summary>
    Recovery,

    /// <summary>
    /// Sideload模式
    /// </summary>
    Sideload
}
