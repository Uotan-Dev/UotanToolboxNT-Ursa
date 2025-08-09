using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Ursa.Controls;
using UotanToolboxNT_Ursa.Helper;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class MainViewViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty] private string _status = "--", _codeName = "--", _bLStatus = "--", _vABStatus = "--";
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
        Content = new HomeViewModel();
        WeakReferenceMessenger.Default.Register<MainViewViewModel, string>(this, OnNavigation);
        // 订阅资源变更事件以刷新菜单项
        ResourceManager.LanguageChanged += OnLanguageChanged;
        ResourceManager.ThemeChanged += OnThemeChanged;
    }

    /// <summary>
    /// 语言变更事件处理
    /// </summary>
    private void OnLanguageChanged(object? sender, string language) => Menus.RefreshMenuItems();

    /// <summary>
    /// 主题变更事件处理
    /// 怎么说呢，虽然理论上主题变更不需要刷新菜单项，但为了保持一致性，这里还是刷新一下。天知道哪里会出奇怪的毛病
    /// </summary>
    private void OnThemeChanged(object? sender, string theme) => Menus.RefreshMenuItems();

    private void OnNavigation(MainViewViewModel vm, string s)
    {
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
    }

    public ObservableCollection<MenuItemViewModel> MenuItems { get; set; } = [];

    /// <summary>
    /// 释放资源，取消事件订阅
    /// </summary>
    public void Dispose()
    {
        ResourceManager.LanguageChanged -= OnLanguageChanged;
        ResourceManager.ThemeChanged -= OnThemeChanged;
        GC.SuppressFinalize(this);
    }
}