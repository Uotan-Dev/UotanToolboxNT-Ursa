using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Ursa.Controls;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class MainViewViewModel : ViewModelBase
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
    }

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
}