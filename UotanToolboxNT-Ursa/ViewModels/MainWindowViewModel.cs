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

    public MenuViewModel Menus { get; set; } = new MenuViewModel();

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
            MenuKeys.MenuKeyAboutUs => new AboutUsDemoViewModel(),
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
        };
    }

    public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }
}
