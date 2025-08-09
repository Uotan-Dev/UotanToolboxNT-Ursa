using System.Collections.ObjectModel;

namespace UotanToolboxNT_Ursa.ViewModels;

public class MenuViewModel : ViewModelBase
{
    public MenuViewModel()
    {
        MenuItems =
        [
            new(){
                ResourceKey = "Sidebar_HomePage",
                Key = MenuKeys.MenuKeyHome // 必须设置Key
            },
            new(){
                ResourceKey = "Sidebar_Basicflash",
                Key = MenuKeys.MenuKeyBasicflash // 必须设置Key
            },
            new(){
                ResourceKey = "Sidebar_Appmgr",
                Key = MenuKeys.MenuKeyAppmgr // 必须设置Key
            },
            new(){
                ResourceKey = "Sidebar_WiredFlash",
                Key = MenuKeys.MenuKeyWiredflash // 必须设置Key
            },
            new(){
                ResourceKey = "Sidebar_Customizedflash",
                Key = MenuKeys.MenuKeyCustomizedflash // 必须设置Key
            },
            new(){
                ResourceKey = "Sidebar_Scrcpy",
                Key = MenuKeys.MenuKeyScrcpy // 必须设置Key
            },
            new(){
                ResourceKey = "Sidebar_FormatExtract",
                Key = MenuKeys.MenuKeyFormatExtract // 必须设置Key
            },
            new(){
                ResourceKey = "Sidebar_ModifyPartition",
                Key = MenuKeys.MenuKeyModifypartition // 必须设置Key
            },
            new(){
                ResourceKey = "Sidebar_GlobalLog",
                Key = MenuKeys.MenuKeyGlobalLog // 必须设置Key
            },
            new(){
                ResourceKey = "Sidebar_Settings",
                Key = MenuKeys.MenuKeySettings // 必须设置Key
            }
        ];
    }

    public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }
    public void RefreshMenuItems()
    {
        foreach (var item in MenuItems)
        {
            item.RefreshMenuHeader();
        }
    }
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