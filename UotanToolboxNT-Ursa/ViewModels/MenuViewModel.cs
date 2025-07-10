using System.Collections.ObjectModel;

namespace UotanToolboxNT_Ursa.ViewModels;

public class MenuViewModel : ViewModelBase
{
    public MenuViewModel()
    {
        MenuItems = new ObservableCollection<MenuItemViewModel>
        {
            new() { MenuHeader = "Introduction", IsSeparator = false },
            new() { MenuHeader = "About Us", Key = MenuKeys.MenuKeyAboutUs, IsSeparator = false },
            new() {
                MenuHeader = "Introduction",
                Children = new ObservableCollection<MenuItemViewModel>{
                    new() {
                        MenuHeader =  "Getting Started",
                        Children = new ObservableCollection<MenuItemViewModel>
                        {
                            new() { MenuHeader = "Code of Conduct" },
                            new() { MenuHeader = "How to Contribute" },
                            new() { MenuHeader = "Development Workflow" },
                        }
                    },
                },
            },
        };
    }

    public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }
}

public static class MenuKeys
{
    public const string MenuKeyAboutUs = "About Us";
}