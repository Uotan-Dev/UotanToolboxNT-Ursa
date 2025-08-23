using CommunityToolkit.Mvvm.ComponentModel;

namespace UotanToolboxNT_Ursa.Models;

/// <summary>
/// 应用程序信息模型
/// </summary>
public partial class ApplicationInfo : ObservableObject
{
    /// <summary>
    /// 是否被选中
    /// </summary>
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// 应用包名
    /// </summary>
    [ObservableProperty]
    private string _name = string.Empty;

    /// <summary>
    /// 应用显示名称
    /// </summary>
    [ObservableProperty]
    private string? _displayName;

    /// <summary>
    /// 应用大小
    /// </summary>
    [ObservableProperty]
    private string _size = string.Empty;

    /// <summary>
    /// 其他信息（版本、安装日期等）
    /// </summary>
    [ObservableProperty]
    private string _otherInfo = string.Empty;
}
