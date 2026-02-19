using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using UotanToolboxNT_Ursa.Helper;
using UotanToolboxNT_Ursa.Models;
using UotanToolboxNT_Ursa.Models.DeviceCore;
using Ursa.Controls;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class MainViewViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty] private string _status = "--", _codeName = "--", _blStatus = "--", _vabStatus = "--";
    public WindowNotificationManager? NotificationManager { get; set; }
    public MenuViewModel Menus { get; set; } = new MenuViewModel();

    private object? _content;

    public object? Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    public MainViewViewModel()
    {
        //Content = new FormatExtractViewModel();
        WeakReferenceMessenger.Default.Register<MainViewViewModel, string>(this, OnNavigation);
        //// 订阅资源变更事件以刷新菜单项
        //ResourceManager.LanguageChanged += OnLanguageChanged;
        //ResourceManager.ThemeChanged += OnThemeChanged;

        //// 订阅设备变化事件以更新设备状态信息
        //Global.DeviceManager.CurrentDeviceChanged += OnCurrentDeviceChanged;

        //// 初始化设备状态信息
        //// 不立即更新当前设备状态来避免重复获取设备信息，此为调试用
        //_ = UpdateDeviceStatusInfoAsync();
    }

    /// <summary>
    /// 语言变更事件处理
    /// </summary>
    //private void OnLanguageChanged(object? sender, string language) => Menus.RefreshMenuItems();

    /// <summary>
    /// 主题变更事件处理
    /// 怎么说呢，虽然理论上主题变更不需要刷新菜单项，但为了保持一致性，这里还是刷新一下。天知道哪里会出奇怪的毛病
    /// </summary>
    //private void OnThemeChanged(object? sender, string theme) => Menus.RefreshMenuItems();

    /// <summary>
    /// 当前设备变化事件处理
    /// </summary>
    //private async void OnCurrentDeviceChanged(object? sender, DeviceChangedEventArgs e) =>
     //   await UpdateDeviceStatusInfoAsync();

    /// <summary>
    /// 更新设备状态信息
    /// </summary>
    private async Task UpdateDeviceStatusInfoAsync()
    {
        //var currentDevice = Global.DeviceManager.CurrentDevice;

        //if (currentDevice != null)
        //{
        //    // 强制刷新设备信息以获取最新数据，否则无法获得除Status外的其他信息。但会导致二次刷新，暂时先这样吧

        //    await currentDevice.RefreshDeviceInfoAsync();

        //    Status = currentDevice.Status;
        //    CodeName = currentDevice.CodeName;
        //    BlStatus = currentDevice.BootloaderStatus;
        //    VabStatus = currentDevice.VABStatus;
        //}
        //else
        //{
        //    Status = "--";
        //    CodeName = "--";
        //    BlStatus = "--";
        //    VabStatus = "--";
        //}
    }

    private void OnNavigation(MainViewViewModel vm, string s) =>
        Content = s switch
        {
            MenuKeys.MenuKeyHome => new HomeViewModel(),
            MenuKeys.MenuKeyBasicflash => new BasicflashViewModel(),
            MenuKeys.MenuKeyAppmgr => new AppmgrViewModel(),
            MenuKeys.MenuKeyWiredflash => new WiredflashViewModel(),
            MenuKeys.MenuKeyCustomizedflash => new CustomizedflashViewModel(),
            MenuKeys.MenuKeyScrcpy => new ScrcpyViewModel(),
            MenuKeys.MenuKeyFormatExtract => new FormatExtractViewModel(),
            MenuKeys.MenuKeyModifypartition => new ModifypartitionViewModel(),
            MenuKeys.MenuKeyGlobalLog => new GlobalLogViewModel(),
            MenuKeys.MenuKeySettings => new SettingsViewModel(),
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
        };

    //public ObservableCollection<MenuItemViewModel> MenuItems { get; set; } = [];

    /// <summary>
    /// 释放资源，取消事件订阅
    /// </summary>
    public void Dispose()
    {
        //ResourceManager.LanguageChanged -= OnLanguageChanged;
        //ResourceManager.ThemeChanged -= OnThemeChanged;
        //Global.DeviceManager.CurrentDeviceChanged -= OnCurrentDeviceChanged;
        //GC.SuppressFinalize(this);
    }
}