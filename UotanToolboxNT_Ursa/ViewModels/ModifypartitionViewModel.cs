using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class ModifypartitionViewModel : ObservableObject
{
    [ObservableProperty]
    private string _searchBox = string.Empty, _idNumber = string.Empty, _newPartitionName = string.Empty, _newPartitionFormat = string.Empty, _newPartitionStartpoint = string.Empty, _newPartitionEndpoint = string.Empty;

    [ObservableProperty]
    private bool _showAllPartEnabled = false, _showAllPartChecked = false, _busyPart = false, _sda = false, _sdb = false, _sdc = false, _eMMC = false, _sdd = false, _sde = false, _sdf = false, _fastboot = false;

    public ModifypartitionViewModel()
    {

    }

    [RelayCommand]
    public async Task RMPart()
    {

    }

    [RelayCommand]
    public async Task ESPON()
    {

    }

    [RelayCommand]
    public async Task MKPart()
    {

    }

    [RelayCommand]
    public async Task ReadPart()
    {

    }

    [RelayCommand]
    public async Task RemoveLimit()
    {

    }
}