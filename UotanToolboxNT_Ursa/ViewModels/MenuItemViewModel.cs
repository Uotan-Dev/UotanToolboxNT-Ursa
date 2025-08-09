using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using UotanToolboxNT_Ursa.Helper;

namespace UotanToolboxNT_Ursa.ViewModels;

public enum ControlStatus
{
    New,
    Beta,
    Stable,
}

public partial class MenuItemViewModel : ViewModelBase
{
    private string? _resourceKey;

    [ObservableProperty]
    private string? _menuHeader;

    public string? ResourceKey
    {
        get => _resourceKey;
        set
        {
            _resourceKey = value;
            if (!string.IsNullOrEmpty(value))
            {
                MenuHeader = LanguageResourceHelper.GetLanguageResource<string>(value);
            }
        }
    }

    public string? MenuIconName { get; set; }
    public string? Key { get; set; }
    public string? Status { get; set; }

    public bool IsSeparator { get; set; }
    public ObservableCollection<MenuItemViewModel> Children { get; set; } = [];

    public ICommand ActivateCommand { get; set; }

    public MenuItemViewModel()
    {
        ActivateCommand = new RelayCommand(OnActivate);
    }

    private void OnActivate()
    {
        if (IsSeparator || Key is null)
        {
            return;
        }
        _ = WeakReferenceMessenger.Default.Send(Key);
    }

    public void RefreshMenuHeader()
    {
        if (!string.IsNullOrEmpty(_resourceKey))
        {
            MenuHeader = LanguageResourceHelper.GetLanguageResource<string>(_resourceKey);
        }
    }
}