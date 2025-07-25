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
                MenuHeader = "GlobalLog",
                Key = MenuKeys.MenuKeyGlobalLog // 必须设置Key
            }
        ];
    }

    public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }
}

public static class MenuKeys
{
    public const string MenuKeyHome = "Home";
    public const string MenuKeyGlobalLog = "GlobalLog";
}