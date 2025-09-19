using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class CustomizedflashViewModel : ObservableObject
{
    [ObservableProperty]
    private string _systemFile = string.Empty, _productFile = string.Empty, _venderFile = string.Empty, _bootFile = string.Empty, _systemextFile = string.Empty,
        _odmFile = string.Empty, _venderbootFile = string.Empty, _initbootFile = string.Empty, _imageFile = string.Empty, _part = string.Empty, _customizedflashLog = string.Empty;

    public CustomizedflashViewModel()
    {

    }

    [RelayCommand]
    public async Task OpenSystemFileBut()
    {

    }

    [RelayCommand]
    public async Task FlashSystemFileBut()
    {

    }

    [RelayCommand]
    public async Task OpenProductFileBut()
    {

    }

    [RelayCommand]
    public async Task FlashProductFileBut()
    {

    }

    [RelayCommand]
    public async Task OpenVenderFileBut()
    {

    }

    [RelayCommand]
    public async Task FlashVenderFileBut()
    {

    }

    [RelayCommand]
    public async Task OpenBootFileBut()
    {

    }

    [RelayCommand]
    public async Task FlashBootFileBut()
    {

    }

    [RelayCommand]
    public async Task OpenSystemextFileBut()
    {

    }

    [RelayCommand]
    public async Task FlashSystemextFileBut()
    {

    }

    [RelayCommand]
    public async Task OpenOdmFileBut()
    {

    }

    [RelayCommand]
    public async Task FlashOdmFileBut()
    {

    }

    [RelayCommand]
    public async Task OpenVenderbootFileBut()
    {

    }

    [RelayCommand]
    public async Task FlashVenderbootFileBut()
    {

    }

    [RelayCommand]
    public async Task OpenInitbootFileBut()
    {

    }

    [RelayCommand]
    public async Task FlashInitbootFileBut()
    {

    }

    [RelayCommand]
    public async Task OpenImageFileBut()
    {

    }

    [RelayCommand]
    public async Task FlashImageFileBut()
    {

    }

    [RelayCommand]
    public async Task DisableVbmeta()
    {

    }

    [RelayCommand]
    public async Task SetOther()
    {

    }
}