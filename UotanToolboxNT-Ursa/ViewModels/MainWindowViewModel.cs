using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Messaging;
using Ursa.Controls;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia!";
    public string? Header { get; set; }

    private object? _content;

    public object? Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }
    public MainWindowViewModel()
    {
        WeakReferenceMessenger.Default.Register<MainWindowViewModel, string>(this, OnNavigation);
    }

    private void OnNavigation(MainWindowViewModel vm, string s)
    {
        Content = s switch
        {
            "Code of Conduct" => new AboutUsDemoViewModel(),
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
        };
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
