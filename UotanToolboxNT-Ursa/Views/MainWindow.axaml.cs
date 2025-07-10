using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Ursa.Controls;

namespace UotanToolboxNT_Ursa.Views;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        //DataContext = this; // 添加这一行
    }

    public ObservableCollection<NavMenuItem> MenuItems { get; set; } =
    [
        new() {
            Header = "Introduction",
            Items =
            {
                new NavMenuItem
                {
                    Header = "Getting Started",
                    Items =
                    {
                        new NavMenuItem { Header = "Code of Conduct" },
                        new NavMenuItem { Header = "How to Contribute" },
                        new NavMenuItem { Header = "Development Workflow" },
                    }
                },
                new NavMenuItem { Header = "Design Principles" },
                new NavMenuItem
                {
                    Header = "Contributing",
                    Items =
                    {
                        new NavMenuItem { Header = "Code of Conduct" },
                        new NavMenuItem { Header = "How to Contribute" },
                        new NavMenuItem { Header = "Development Workflow" },
                    }
                },
            }
        }
    ];

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}