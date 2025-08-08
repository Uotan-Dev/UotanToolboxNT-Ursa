using CommunityToolkit.Mvvm.ComponentModel;

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
    public ScrcpyViewModel()
    {

    }
}