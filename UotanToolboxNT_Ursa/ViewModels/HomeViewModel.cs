using System.Threading.Tasks;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    private string _progressDisk = "0", _memLevel = "0", _status = "--", _bLStatus = "--",
    _vABStatus = "--", _codeName = "--", _vNDKVersion = "--", _cPUCode = "--",
    _powerOnTime = "--", _deviceBrand = "--", _deviceModel = "--", _systemSDK = "--",
    _cPUABI = "--", _displayHW = "--", _density = "--", _boardID = "--", _platform = "--",
    _compile = "--", _kernel = "--", _selectedSimpleContent = null, _diskType = "--",
    _batteryLevel = "0", _batteryInfo = "--", _useMem = "--", _diskInfo = "--";
    [ObservableProperty] private bool _IsConnecting;
    [ObservableProperty] private bool _commonDevicesList;
    [ObservableProperty] private static AvaloniaList<string> _simpleContent;

    public HomeViewModel()
    {

    }

    [RelayCommand]
    public async Task FreshDeviceList()
    {

    }

    [RelayCommand]
    public async Task OpenAFDI()
    {

    }

    [RelayCommand]
    public async Task Open9008DI()
    {

    }

    [RelayCommand]
    public async Task OpenUSBP()
    {

    }

    [RelayCommand]
    public async Task OpenReUSBP()
    {

    }

    [RelayCommand]
    public async Task RebootSys()
    {

    }

    [RelayCommand]
    public async Task RebootRec()
    {

    }

    [RelayCommand]
    public async Task RebootBL()
    {

    }

    [RelayCommand]
    public async Task RebootFB()
    {

    }

    [RelayCommand]
    public async Task PowerOff()
    {

    }

    [RelayCommand]
    public async Task RebootEDL()
    {

    }
}