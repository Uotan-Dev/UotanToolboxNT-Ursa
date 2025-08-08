using System.Collections.ObjectModel;

namespace UotanToolboxNT_Ursa.ViewModels;

public class MenuViewModel : ViewModelBase
{
    public MenuViewModel()
    {
        MenuItems =
        [
            new(){
                MenuHeader = "Home",
                Key = MenuKeys.MenuKeyHome // 必须设置Key
            },
            new(){
                MenuHeader = "Basicflash",
                Key = MenuKeys.MenuKeyBasicflash // 必须设置Key
            },
            new(){
                MenuHeader = "Appmgr",
                Key = MenuKeys.MenuKeyAppmgr // 必须设置Key
            },
            new(){
                MenuHeader = "Wiredflash",
                Key = MenuKeys.MenuKeyWiredflash // 必须设置Key
            },
            new(){
                MenuHeader = "Customizedflash",
                Key = MenuKeys.MenuKeyCustomizedflash // 必须设置Key
            },
            new(){
                MenuHeader = "Scrcpy",
                Key = MenuKeys.MenuKeyScrcpy // 必须设置Key
            },
            new(){
                MenuHeader = "FormatExtract",
                Key = MenuKeys.MenuKeyFormatExtract // 必须设置Key
            },
            new(){
                MenuHeader = "Modifypartition",
                Key = MenuKeys.MenuKeyModifypartition // 必须设置Key
            },
            new(){
                MenuHeader = "GlobalLog",
                Key = MenuKeys.MenuKeyGlobalLog // 必须设置Key
            },
            new(){
                MenuHeader = "Settings",
                Key = MenuKeys.MenuKeySettings // 必须设置Key
            }
        ];
    }

    public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }
}

public static class MenuKeys
{
    public const string MenuKeyHome = "Home";
    public const string MenuKeyGlobalLog = "GlobalLog";
    public const string MenuKeySettings = "Settings";
    public const string MenuKeyBasicflash = "Basicflash";
    public const string MenuKeyAppmgr = "Appmgr";
    public const string MenuKeyWiredflash = "Wiredflash";
    public const string MenuKeyCustomizedflash = "Customizedflash";
    public const string MenuKeyScrcpy = "Scrcpy";
    public const string MenuKeyFormatExtract = "FormatExtract";
    public const string MenuKeyModifypartition = "Modifypartition";
}