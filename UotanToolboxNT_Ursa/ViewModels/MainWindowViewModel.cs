using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string _status, _codeName, _bLStatus, _vABStatus;
    public MenuViewModel Menus { get; set; } = new MenuViewModel();

    private object? _content;

    public object? Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    public MainWindowViewModel()
    {
        Content = new AboutUsDemoViewModel();
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

    public ObservableCollection<MenuItemViewModel> MenuItems { get; set; } = [];
}
