using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UotanToolboxNT_Ursa.Models;

public class BasicflashModel
{
    // 解锁文件路径
    [JsonPropertyName("unlockFilePath")]
    public string UnlockFilePath { get; set; } = string.Empty;

    // 解锁代码
    [JsonPropertyName("unlockCode")]
    public string UnlockCode { get; set; } = string.Empty;

    // 选择的解锁代码类型
    [JsonPropertyName("selectedUnlockCodeType")]
    public string SelectedUnlockCodeType { get; set; } = string.Empty;

    // 选择的基础命令
    [JsonPropertyName("selectedBasicCommand")]
    public string SelectedBasicCommand { get; set; } = string.Empty;

    // Recovery文件路径
    [JsonPropertyName("recoveryFilePath")]
    public string RecoveryFilePath { get; set; } = string.Empty;

    // 选择的重启命令
    [JsonPropertyName("selectedRebootCommand")]
    public string SelectedRebootCommand { get; set; } = string.Empty;

    // Magisk文件路径
    [JsonPropertyName("magiskFilePath")]
    public string MagiskFilePath { get; set; } = string.Empty;

    // Boot文件路径
    [JsonPropertyName("bootFilePath")]
    public string BootFilePath { get; set; } = string.Empty;

    // 保持AVB或DM验证
    [JsonPropertyName("keepAVBOrDM")]
    public bool KeepAVBOrDM { get; set; } = true;

    // 保持强加密
    [JsonPropertyName("keepStrongEncryption")]
    public bool KeepStrongEncryption { get; set; } = true;

    // 修复Vbmeta
    [JsonPropertyName("repairVbmeta")]
    public bool RepairVbmeta { get; set; } = false;

    // 安装到Recovery
    [JsonPropertyName("installToRecovery")]
    public bool InstallToRecovery { get; set; } = false;

    // 强制Rootfs
    [JsonPropertyName("forceRootfs")]
    public bool ForceRootfs { get; set; } = true;

    // 选择的镜像架构
    [JsonPropertyName("selectedImageArchitecture")]
    public string SelectedImageArchitecture { get; set; } = string.Empty;

    // 是否选择TWRP安装
    [JsonPropertyName("isTWRPInstall")]
    public bool IsTWRPInstall { get; set; } = true;

    // 是否选择ADB Sideload
    [JsonPropertyName("isADBSideload")]
    public bool IsADBSideload { get; set; } = false;

    // 正在执行的加载状态
    [JsonPropertyName("isExecuting")]
    public bool IsExecuting { get; set; } = false;

    // 解锁代码类型列表
    public static List<string> UnlockCodeTypes { get; } =
    [
        "小米",
        "一加",
        "realme",
        "OPPO",
        "vivo",
        "华为",
        "荣耀",
        "其他"
    ];

    // 基础命令列表
    public static List<string> BasicCommands { get; } =
    [
        "fastboot flashing unlock",
        "fastboot flashing unlock_critical",
        "fastboot oem unlock",
        "fastboot oem unlock-go",
        "fastboot unlock",
        "fastboot unlock_critical"
    ];

    // 重启命令列表
    public static List<string> RebootCommands { get; } =
    [
        "fastboot reboot",
        "fastboot reboot bootloader",
        "fastboot reboot fastboot",
        "fastboot reboot recovery",
        "fastboot reboot download",
        "fastboot reboot edl",
        "fastboot reboot emergency"
    ];

    // 镜像架构列表
    public static List<string> ImageArchitectures { get; } =
    [
        "arm64-v8a",
        "armeabi-v7a",
        "x86_64",
        "x86",
        "自动检测"
    ];
}
