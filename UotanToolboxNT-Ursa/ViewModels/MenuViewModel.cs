using System.Collections.ObjectModel;

namespace UotanToolboxNT_Ursa.ViewModels;

public class MenuViewModel : ViewModelBase
{
    public MenuViewModel()
    {
        MenuItems =
        [
            new(){
                MenuHeader = "Introduction",
                Children =
                [
                    new() { MenuHeader = "Code of Conduct" },
                    new() { MenuHeader = "How to Contribute" },
                    new() { MenuHeader = "Development Workflow" },
                ]
            },
            new(){
                MenuHeader = "About Us",
                Key = MenuKeys.MenuKeyAboutUs // ±ÿ–Î…Ë÷√Key
            }
        ];
    }

    public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }
}

public static class MenuKeys
{
    public const string MenuKeyAboutUs = "About Us";
}