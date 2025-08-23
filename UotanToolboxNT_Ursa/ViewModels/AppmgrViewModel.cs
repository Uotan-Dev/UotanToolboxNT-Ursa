using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UotanToolboxNT_Ursa.Models;
using UotanToolboxNT_Ursa.Helper;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class AppmgrViewModel : ObservableObject
{
    #region 属性

    /// <summary>
    /// 应用程序列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ApplicationInfo> _applications = [];

    /// <summary>
    /// 是否正在忙碌（加载中）
    /// </summary>
    [ObservableProperty]
    private bool _isBusy = false;

    /// <summary>
    /// 是否有应用项目
    /// </summary>
    [ObservableProperty]
    private bool _hasItems = false;

    /// <summary>
    /// 搜索框是否启用
    /// </summary>
    [ObservableProperty]
    private bool _sBoxEnabled = true;

    /// <summary>
    /// 是否显示系统应用
    /// </summary>
    [ObservableProperty]
    private bool _isSystemAppDisplayed = false;

    /// <summary>
    /// 是否正在安装
    /// </summary>
    [ObservableProperty]
    private bool _isInstalling = false;

    /// <summary>
    /// APK文件路径
    /// </summary>
    [ObservableProperty]
    private string _apkFile = string.Empty;

    /// <summary>
    /// 搜索关键词
    /// </summary>
    [ObservableProperty]
    private string _search = string.Empty;

    /// <summary>
    /// 搜索框水印文字
    /// </summary>
    [ObservableProperty]
    private string _sBoxWater = string.Empty;

    /// <summary>
    /// 当前选中的应用信息
    /// </summary>
    [ObservableProperty]
    private ApplicationInfo? _selectedApplication;

    #endregion

    #region 私有字段

    private ApplicationInfo[]? _allApplicationInfos;
    private List<ApplicationInfo>? _applicationInfos;

    #endregion

    #region 构造函数

    public AppmgrViewModel()
    {
        // 设置默认搜索框水印
        SBoxWater = GetTranslation("Appmgr_SearchApp");

        // 监听搜索关键词变化
        PropertyChanged += OnPropertyChanged;
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 获取翻译文本
    /// </summary>
    /// <param name="key">翻译键</param>
    /// <returns>翻译后的文本</returns>
    private static string GetTranslation(string key) => LanguageResourceHelper.GetLanguageResource<string>(key) ?? key;

    /// <summary>
    /// 属性变化事件处理
    /// </summary>
    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Search))
        {
            FilterApplications();
        }
        else if (e.PropertyName == nameof(IsSystemAppDisplayed))
        {
            // 重新获取应用列表
            _ = ConnectAsync();
        }
    }

    /// <summary>
    /// 过滤应用程序列表
    /// </summary>
    private void FilterApplications()
    {
        if (_applicationInfos != null && _allApplicationInfos != null)
        {
            if (!string.IsNullOrEmpty(Search))
            {
                var filteredApps = _allApplicationInfos
                    .Where(app => app.DisplayName?.Contains(Search, StringComparison.OrdinalIgnoreCase) == true ||
                                  app.Name.Contains(Search, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(app => app.Name)
                    .ToList();

                Applications = new ObservableCollection<ApplicationInfo>(filteredApps);
            }
            else
            {
                var sortedApps = _allApplicationInfos
                    .Where(info => info != null)
                    .OrderBy(app => app.Name)
                    .ToList();

                Applications = new ObservableCollection<ApplicationInfo>(sortedApps);
            }
        }
    }

    #endregion

    #region 命令

    /// <summary>
    /// 连接设备并获取应用列表
    /// </summary>
    [RelayCommand]
    public async Task ConnectAsync()
    {
        IsBusy = true;
        SBoxEnabled = false;
        SBoxWater = GetTranslation("Appmgr_SearchWait");

        try
        {
            // 这里需要实现具体的设备连接和应用获取逻辑
            // 模拟获取应用列表的过程
            await Task.Delay(1000);

            // 示例数据，实际实现需要调用设备API
            var sampleApps = new List<ApplicationInfo>
            {
                new() { Name = "com.example.app1", DisplayName = "示例应用1", Size = "10MB", OtherInfo = "v1.0.0" },
                new() { Name = "com.example.app2", DisplayName = "示例应用2", Size = "5MB", OtherInfo = "v2.1.0" }
            };

            _allApplicationInfos = [.. sampleApps];
            _applicationInfos = sampleApps;
            Applications = new ObservableCollection<ApplicationInfo>(sampleApps);
            HasItems = sampleApps.Count > 0;
        }
        catch
        {
            // 处理异常
        }
        finally
        {
            SBoxEnabled = true;
            SBoxWater = GetTranslation("Appmgr_SearchApp");
            IsBusy = false;
        }
    }

    /// <summary>
    /// 安装APK文件
    /// </summary>
    [RelayCommand]
    public async Task InstallApkAsync()
    {
        if (string.IsNullOrEmpty(ApkFile))
        {
            return;
        }

        IsInstalling = true;

        try
        {
            // 这里需要实现具体的APK安装逻辑
            await Task.Delay(2000); // 模拟安装过程

            // 安装完成后刷新应用列表
            await ConnectAsync();
        }
        catch
        {
            // 处理安装异常
        }
        finally
        {
            IsInstalling = false;
        }
    }

    /// <summary>
    /// 选择APK文件
    /// </summary>
    [RelayCommand]
    public Task SelectApkFileAsync() => Task.CompletedTask;

    /// <summary>
    /// 运行应用
    /// </summary>
    [RelayCommand]
    public async Task RunAppAsync()
    {
        if (SelectedApplication == null)
        {
            return;
        }

        try
        {
            // 实现运行应用的逻辑
            await Task.Delay(500); // 模拟执行过程
        }
        catch
        {
            // 处理异常
        }
    }

    /// <summary>
    /// 强制停止应用
    /// </summary>
    [RelayCommand]
    public async Task ForceStopAppAsync()
    {
        if (SelectedApplication == null)
        {
            return;
        }

        try
        {
            // 实现强制停止应用的逻辑
            await Task.Delay(500); // 模拟执行过程
        }
        catch
        {
            // 处理异常
        }
    }

    /// <summary>
    /// 启用/禁用应用
    /// </summary>
    [RelayCommand]
    public async Task ToggleAppStatusAsync()
    {
        if (SelectedApplication == null)
        {
            return;
        }

        try
        {
            // 实现启用/禁用应用的逻辑
            await Task.Delay(500); // 模拟执行过程
        }
        catch
        {
            // 处理异常
        }
    }

    /// <summary>
    /// 禁用应用
    /// </summary>
    [RelayCommand]
    public async Task DisableAppAsync()
    {
        if (SelectedApplication == null)
        {
            return;
        }

        try
        {
            // 实现禁用应用的逻辑
            await Task.Delay(500); // 模拟执行过程
        }
        catch
        {
            // 处理异常
        }
    }

    /// <summary>
    /// 启用应用
    /// </summary>
    [RelayCommand]
    public async Task EnableAppAsync()
    {
        if (SelectedApplication == null)
        {
            return;
        }

        try
        {
            // 实现启用应用的逻辑
            await Task.Delay(500); // 模拟执行过程
        }
        catch
        {
            // 处理异常
        }
    }

    /// <summary>
    /// 提取APK
    /// </summary>
    [RelayCommand]
    public async Task ExtractApkAsync()
    {
        if (SelectedApplication == null)
        {
            return;
        }

        try
        {
            // 实现提取APK的逻辑
            await Task.Delay(1000); // 模拟提取过程
        }
        catch
        {
            // 处理异常
        }
    }

    /// <summary>
    /// 卸载应用
    /// </summary>
    [RelayCommand]
    public async Task UninstallAppAsync()
    {
        if (SelectedApplication == null)
        {
            return;
        }

        try
        {
            // 实现卸载应用的逻辑
            await Task.Delay(1000); // 模拟卸载过程
            // 卸载成功后从列表中移除
            Applications.Remove(SelectedApplication);
            SelectedApplication = null;
        }
        catch
        {
            // 处理异常
        }
    }

    /// <summary>
    /// 保留数据卸载应用
    /// </summary>
    [RelayCommand]
    public async Task KeepDataUninstallAppAsync()
    {
        if (SelectedApplication == null)
        {
            return;
        }

        try
        {
            // 实现保留数据卸载应用的逻辑
            await Task.Delay(1000); // 模拟卸载过程
            // 卸载成功后从列表中移除
            Applications.Remove(SelectedApplication);
            SelectedApplication = null;
        }
        catch
        {
            // 处理异常
        }
    }

    /// <summary>
    /// 清除应用数据
    /// </summary>
    [RelayCommand]
    public async Task ClearAppDataAsync()
    {
        if (SelectedApplication == null)
        {
            return;
        }

        try
        {
            // 实现清除应用数据的逻辑
            await Task.Delay(500); // 模拟清除过程
        }
        catch
        {
            // 处理异常
        }
    }

    #endregion
}