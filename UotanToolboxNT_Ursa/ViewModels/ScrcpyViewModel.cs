using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class ScrcpyViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _recordScreen = false, _windowFixed = false, _computerControl = true, _fullScreen = false, _showBorder = true,
                        _showTouch = true, _closeScreen = false, _screenAwake = false, _screenAwakeStatus = true, _clipboardSync = true,
                        _cameraMirror = false, _enableVirtualScreen = false, _mirrorRotation = false, _lockAngle = false,
                        _rotation0 = true, _rotation1 = false, _rotation2 = false, _rotation3 = false, _rotation4 = false,
                        _forwardAudio = true;
    [ObservableProperty] private int _angle = 0;
    [ObservableProperty] private bool _IsConnecting;
    [ObservableProperty] private string _windowTitle, _recordFolder, _virtualScreenPackage, _virtualScreenDisplaySize;

    [ObservableProperty] private double _bitRate = 8;
    [ObservableProperty] private double _frameRate = 60;
    [ObservableProperty] private double _sizeResolution = 0;
    public ScrcpyViewModel()
    {

    }

    [RelayCommand]
    public async Task Connect()
    {

    }

    [RelayCommand]
    public async Task OpenFolderBut()
    {

    }

    [RelayCommand]
    public async Task Back()
    {

    }

    [RelayCommand]
    public async Task Home()
    {

    }

    [RelayCommand]
    public async Task Mul()
    {

    }

    [RelayCommand]
    public async Task Lock()
    {

    }

    [RelayCommand]
    public async Task VolU()
    {

    }

    [RelayCommand]
    public async Task VolD()
    {

    }

    [RelayCommand]
    public async Task Mute()
    {

    }

    [RelayCommand]
    public async Task SC()
    {

    }
}