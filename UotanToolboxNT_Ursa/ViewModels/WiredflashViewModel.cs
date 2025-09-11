using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using UotanToolboxNT_Ursa.Models;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class WiredflashViewModel : ObservableObject
{
    [ObservableProperty]
    private string _fastbootFile = string.Empty, _fastbootdFile = string.Empty, _adbSideloadFile = string.Empty, _fastbootUpdatedFile = string.Empty, _batFile = string.Empty, _wiredflashLog = string.Empty;

    [ObservableProperty]
    private bool _addRoot = false, _erasData = false;

    public WiredflashViewModel()
    {

    }

    [RelayCommand]
    public async Task OpenFastbootFile()
    {
        
    }

    [RelayCommand]
    public async Task OpenFastbootdFile()
    {

    }

    [RelayCommand]
    public async Task StartTXTFlash()
    {

    }

    [RelayCommand]
    public async Task OpenSideloadFile()
    {

    }

    [RelayCommand]
    public async Task OpenUpdatedFile()
    {

    }

    [RelayCommand]
    public async Task OpenBatFile()
    {

    }

    [RelayCommand]
    public async Task SetToA()
    {

    }

    [RelayCommand]
    public async Task SetToB()
    {

    }

    [RelayCommand]
    public async Task StartFlash()
    {

    }
}