using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using Ursa.Controls;

namespace UotanToolbox;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    public string? Header { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this; // 添加这一行
    }

    public ObservableCollection<NavMenuItem> MenuItems { get; set; } = new ObservableCollection<NavMenuItem>
    {
        new NavMenuItem
        {
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
    };

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}