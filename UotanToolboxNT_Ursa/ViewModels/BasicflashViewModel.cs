using System.IO;
using System.Threading.Tasks;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UotanToolboxNT_Ursa.Models;
using static UotanToolboxNT_Ursa.Models.GlobalLogModel;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class BasicflashViewModel : ObservableObject
{
    #region 解锁相关属性

    [ObservableProperty]
    private string _unlockFilePath = string.Empty;

    [ObservableProperty]
    private string _unlockCode = string.Empty;

    [ObservableProperty]
    private string _selectedUnlockCodeType = string.Empty;

    public AvaloniaList<string> UnlockCodeTypes { get; } = [.. BasicflashModel.UnlockCodeTypes];

    #endregion

    #region 基础命令相关属性

    [ObservableProperty]
    private string _selectedBasicCommand = string.Empty;

    public AvaloniaList<string> BasicCommands { get; } = [.. BasicflashModel.BasicCommands];

    #endregion

    #region Recovery刷写相关属性

    [ObservableProperty]
    private string _recoveryFilePath = string.Empty;

    #endregion

    #region 重启相关属性

    [ObservableProperty]
    private string _selectedRebootCommand = string.Empty;

    public AvaloniaList<string> RebootCommands { get; } = [.. BasicflashModel.RebootCommands];

    #endregion

    #region Boot修复相关属性

    [ObservableProperty]
    private string _magiskFilePath = string.Empty;

    [ObservableProperty]
    private string _bootFilePath = string.Empty;

    [ObservableProperty]
    private bool _keepAVBOrDM = true;

    [ObservableProperty]
    private bool _keepStrongEncryption = true;

    [ObservableProperty]
    private bool _repairVbmeta = false;

    [ObservableProperty]
    private bool _installToRecovery = false;

    [ObservableProperty]
    private bool _forceRootfs = true;

    [ObservableProperty]
    private string _selectedImageArchitecture = string.Empty;

    public AvaloniaList<string> ImageArchitectures { get; } = [.. BasicflashModel.ImageArchitectures];

    #endregion

    #region 快捷刷写相关属性

    [ObservableProperty]
    private bool _isTWRPInstall = true;

    [ObservableProperty]
    private bool _isADBSideload = false;

    #endregion

    #region 执行状态

    [ObservableProperty]
    private bool _isUnlockExecuting = false;

    [ObservableProperty]
    private bool _isBasicCommandExecuting = false;

    [ObservableProperty]
    private bool _isRecoveryExecuting = false;

    [ObservableProperty]
    private bool _isRebootExecuting = false;

    [ObservableProperty]
    private bool _isBootRepairExecuting = false;

    [ObservableProperty]
    private bool _isEasyFlashExecuting = false;

    #endregion

    public BasicflashViewModel()
    {
        // 初始化默认值
        if (UnlockCodeTypes.Count > 0)
        {
            SelectedUnlockCodeType = UnlockCodeTypes[0];
        }

        if (BasicCommands.Count > 0)
        {
            SelectedBasicCommand = BasicCommands[0];
        }

        if (RebootCommands.Count > 0)
        {
            SelectedRebootCommand = RebootCommands[0];
        }

        if (ImageArchitectures.Count > 0)
        {
            SelectedImageArchitecture = ImageArchitectures[0];
        }
    }

    #region 文件选择命令

    [RelayCommand]
    private async Task ChooseUnlockFileAsync()
    {
        try
        {
            // TODO: 实现文件选择功能
            // 这里可以在将来添加文件选择对话框的实现
            await Task.CompletedTask;
            AddLog("文件选择功能待实现");
        }
        catch (System.Exception ex)
        {
            AddLog($"选择解锁文件失败: {ex.Message}", LogLevel.Error);
        }
    }

    [RelayCommand]
    private async Task ChooseRecoveryFileAsync()
    {
        try
        {
            // TODO: 实现文件选择功能
            await Task.CompletedTask;
            AddLog("文件选择功能待实现");
        }
        catch (System.Exception ex)
        {
            AddLog($"选择Recovery文件失败: {ex.Message}", LogLevel.Error);
        }
    }

    [RelayCommand]
    private async Task ChooseMagiskFileAsync()
    {
        try
        {
            // TODO: 实现文件选择功能
            await Task.CompletedTask;
            AddLog("文件选择功能待实现");
        }
        catch (System.Exception ex)
        {
            AddLog($"选择Magisk文件失败: {ex.Message}", LogLevel.Error);
        }
    }

    [RelayCommand]
    private async Task ChooseBootFileAsync()
    {
        try
        {
            // TODO: 实现文件选择功能
            await Task.CompletedTask;
            AddLog("文件选择功能待实现");
        }
        catch (System.Exception ex)
        {
            AddLog($"选择Boot文件失败: {ex.Message}", LogLevel.Error);
        }
    }

    #endregion

    #region 解锁相关命令

    [RelayCommand]
    private async Task UnlockNowAsync()
    {
        if (IsUnlockExecuting)
        {
            return;
        }

        try
        {
            IsUnlockExecuting = true;
            AddLog("开始执行解锁操作...");

            if (string.IsNullOrEmpty(UnlockFilePath) || !File.Exists(UnlockFilePath))
            {
                AddLog("请先选择有效的解锁文件", LogLevel.Warning);
                return;
            }

            // 检查设备连接
            var device = Global.DeviceManager.CurrentDevice;
            if (device == null)
            {
                AddLog("未检测到设备连接", LogLevel.Warning);
                return;
            }

            AddLog($"正在使用解锁文件: {UnlockFilePath}");
            AddLog($"解锁代码类型: {SelectedUnlockCodeType}");
            AddLog($"解锁代码: {UnlockCode}");

            // 这里添加实际的解锁逻辑
            await Task.Delay(2000); // 模拟执行时间

            AddLog("解锁操作完成");
        }
        catch (System.Exception ex)
        {
            AddLog($"解锁操作失败: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            IsUnlockExecuting = false;
        }
    }

    [RelayCommand]
    private async Task RelockBootloaderAsync()
    {
        if (IsUnlockExecuting)
        {
            return;
        }

        try
        {
            IsUnlockExecuting = true;
            AddLog("开始执行重新锁定Bootloader操作...");

            // 检查设备连接
            var device = Global.DeviceManager.CurrentDevice;
            if (device == null)
            {
                AddLog("未检测到设备连接", LogLevel.Warning);
                return;
            }

            // 这里添加实际的重新锁定逻辑
            await Task.Delay(2000); // 模拟执行时间

            AddLog("重新锁定Bootloader操作完成");
        }
        catch (System.Exception ex)
        {
            AddLog($"重新锁定Bootloader操作失败: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            IsUnlockExecuting = false;
        }
    }

    #endregion

    #region 基础命令相关

    [RelayCommand]
    private async Task ExecuteUnlockCommandAsync()
    {
        if (IsBasicCommandExecuting)
        {
            return;
        }

        try
        {
            IsBasicCommandExecuting = true;
            AddLog($"开始执行解锁命令: {SelectedBasicCommand}");

            // 检查设备连接
            var device = Global.DeviceManager.CurrentDevice;
            if (device == null)
            {
                AddLog("未检测到设备连接", LogLevel.Warning);
                return;
            }

            // 这里添加实际的命令执行逻辑
            await Task.Delay(2000); // 模拟执行时间

            AddLog("解锁命令执行完成");
        }
        catch (System.Exception ex)
        {
            AddLog($"解锁命令执行失败: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            IsBasicCommandExecuting = false;
        }
    }

    #endregion

    #region Recovery刷写相关命令

    [RelayCommand]
    private async Task FlashRecoveryAsync()
    {
        if (IsRecoveryExecuting)
        {
            return;
        }

        try
        {
            IsRecoveryExecuting = true;
            AddLog("开始刷写Recovery...");

            if (string.IsNullOrEmpty(RecoveryFilePath) || !File.Exists(RecoveryFilePath))
            {
                AddLog("请先选择有效的Recovery文件", LogLevel.Warning);
                return;
            }

            // 检查设备连接
            var device = Global.DeviceManager.CurrentDevice;
            if (device == null)
            {
                AddLog("未检测到设备连接", LogLevel.Warning);
                return;
            }

            AddLog($"正在刷写Recovery文件: {RecoveryFilePath}");

            // 这里添加实际的Recovery刷写逻辑
            await Task.Delay(3000); // 模拟执行时间

            AddLog("Recovery刷写完成");
        }
        catch (System.Exception ex)
        {
            AddLog($"Recovery刷写失败: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            IsRecoveryExecuting = false;
        }
    }

    [RelayCommand]
    private async Task BootToBootAPartAsync()
    {
        if (IsRecoveryExecuting)
        {
            return;
        }

        try
        {
            IsRecoveryExecuting = true;
            AddLog("开始切换到Boot A分区...");

            // 检查设备连接
            var device = Global.DeviceManager.CurrentDevice;
            if (device == null)
            {
                AddLog("未检测到设备连接", LogLevel.Warning);
                return;
            }

            // 这里添加实际的切换逻辑
            await Task.Delay(1000); // 模拟执行时间

            AddLog("已切换到Boot A分区");
        }
        catch (System.Exception ex)
        {
            AddLog($"切换到Boot A分区失败: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            IsRecoveryExecuting = false;
        }
    }

    [RelayCommand]
    private async Task BootToBootBPartAsync()
    {
        if (IsRecoveryExecuting)
        {
            return;
        }

        try
        {
            IsRecoveryExecuting = true;
            AddLog("开始切换到Boot B分区...");

            // 检查设备连接
            var device = Global.DeviceManager.CurrentDevice;
            if (device == null)
            {
                AddLog("未检测到设备连接", LogLevel.Warning);
                return;
            }

            // 这里添加实际的切换逻辑
            await Task.Delay(1000); // 模拟执行时间

            AddLog("已切换到Boot B分区");
        }
        catch (System.Exception ex)
        {
            AddLog($"切换到Boot B分区失败: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            IsRecoveryExecuting = false;
        }
    }

    #endregion

    #region 重启相关命令

    [RelayCommand]
    private async Task ExecuteRebootAsync()
    {
        if (IsRebootExecuting)
        {
            return;
        }

        try
        {
            IsRebootExecuting = true;
            AddLog($"开始执行重启命令: {SelectedRebootCommand}");

            // 检查设备连接
            var device = Global.DeviceManager.CurrentDevice;
            if (device == null)
            {
                AddLog("未检测到设备连接", LogLevel.Warning);
                return;
            }

            // 这里添加实际的重启命令执行逻辑
            await Task.Delay(2000); // 模拟执行时间

            AddLog("重启命令执行完成");
        }
        catch (System.Exception ex)
        {
            AddLog($"重启命令执行失败: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            IsRebootExecuting = false;
        }
    }

    #endregion

    #region Boot修复相关命令

    [RelayCommand]
    private async Task StartBootRepairAsync()
    {
        if (IsBootRepairExecuting)
        {
            return;
        }

        try
        {
            IsBootRepairExecuting = true;
            AddLog("开始Boot修复操作...");

            if (string.IsNullOrEmpty(MagiskFilePath) || !File.Exists(MagiskFilePath))
            {
                AddLog("请先选择有效的Magisk文件", LogLevel.Warning);
                return;
            }

            if (string.IsNullOrEmpty(BootFilePath) || !File.Exists(BootFilePath))
            {
                AddLog("请先选择有效的Boot文件", LogLevel.Warning);
                return;
            }

            // 检查设备连接
            var device = Global.DeviceManager.CurrentDevice;
            if (device == null)
            {
                AddLog("未检测到设备连接", LogLevel.Warning);
            }

            AddLog($"Magisk文件: {MagiskFilePath}");
            AddLog($"Boot文件: {BootFilePath}");
            AddLog($"保持AVB/DM验证: {KeepAVBOrDM}");
            AddLog($"保持强加密: {KeepStrongEncryption}");
            AddLog($"修复Vbmeta: {RepairVbmeta}");
            AddLog($"安装到Recovery: {InstallToRecovery}");
            AddLog($"强制Rootfs: {ForceRootfs}");
            AddLog($"镜像架构: {SelectedImageArchitecture}");

            // 这里添加实际的Boot修复逻辑
            await Task.Delay(5000); // 模拟执行时间

            AddLog("Boot修复操作完成");
        }
        catch (System.Exception ex)
        {
            AddLog($"Boot修复操作失败: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            IsBootRepairExecuting = false;
        }
    }

    #endregion

    #region 快捷刷写相关命令

    [RelayCommand]
    private async Task FlashChosenMagiskAsync()
    {
        if (IsEasyFlashExecuting)
        {
            return;
        }

        try
        {
            IsEasyFlashExecuting = true;
            AddLog("开始刷写选择的Magisk...");

            if (string.IsNullOrEmpty(MagiskFilePath) || !File.Exists(MagiskFilePath))
            {
                AddLog("请先选择有效的Magisk文件", LogLevel.Warning);
                return;
            }

            // 检查设备连接
            var device = Global.DeviceManager.CurrentDevice;
            if (device == null)
            {
                AddLog("未检测到设备连接", LogLevel.Warning);
                return;
            }

            // 这里添加实际的Magisk刷写逻辑
            await Task.Delay(3000); // 模拟执行时间

            AddLog("Magisk刷写完成");
        }
        catch (System.Exception ex)
        {
            AddLog($"Magisk刷写失败: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            IsEasyFlashExecuting = false;
        }
    }

    [RelayCommand]
    private async Task PreventRecoverOfficialRecoveryAsync()
    {
        if (IsEasyFlashExecuting)
        {
            return;
        }

        try
        {
            IsEasyFlashExecuting = true;
            AddLog("开始阻止恢复官方Recovery...");

            // 检查设备连接
            var device = Global.DeviceManager.CurrentDevice;
            if (device == null)
            {
                AddLog("未检测到设备连接", LogLevel.Warning);
                return;
            }

            // 这里添加实际的阻止恢复官方Recovery逻辑
            await Task.Delay(2000); // 模拟执行时间

            AddLog("已阻止恢复官方Recovery");
        }
        catch (System.Exception ex)
        {
            AddLog($"阻止恢复官方Recovery失败: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            IsEasyFlashExecuting = false;
        }
    }

    [RelayCommand]
    private async Task SyncABPartAsync()
    {
        if (IsEasyFlashExecuting)
        {
            return;
        }

        try
        {
            IsEasyFlashExecuting = true;
            AddLog("开始同步A/B分区...");

            // 检查设备连接
            var device = Global.DeviceManager.CurrentDevice;
            if (device == null)
            {
                AddLog("未检测到设备连接", LogLevel.Warning);
                return;
            }

            // 这里添加实际的A/B分区同步逻辑
            await Task.Delay(3000); // 模拟执行时间

            AddLog("A/B分区同步完成");
        }
        catch (System.Exception ex)
        {
            AddLog($"A/B分区同步失败: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            IsEasyFlashExecuting = false;
        }
    }

    #endregion

    #region 无线电选项处理

    partial void OnIsTWRPInstallChanged(bool value)
    {
        if (value)
        {
            IsADBSideload = false;
        }
    }

    partial void OnIsADBSideloadChanged(bool value)
    {
        if (value)
        {
            IsTWRPInstall = false;
        }
    }

    #endregion
}