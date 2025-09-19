using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace UotanToolboxNT_Ursa.ViewModels;

public partial class FormatExtractViewModel : ObservableObject
{
    [ObservableProperty]
    private string _qcnFile = string.Empty, _superEmptyFile = string.Empty, _formatName = string.Empty, _extractName = string.Empty, _formatExtractLog = string.Empty;

    [ObservableProperty]
    private bool _busyQCN = false, _busyFlash = false, _busyFormat = false, _busyExtract = false, _eXT4 = true, _f2FS = false, _fAT32 = false, _eXFAT = false, _nTFS = false;

    public FormatExtractViewModel()
    {

    }

    [RelayCommand]
    public async Task OpenQcnFile()
    {

    }

    [RelayCommand]
    public async Task WriteQcn()
    {

    }

    [RelayCommand]
    public async Task BackupQcn()
    {

    }

    [RelayCommand]
    public async Task OpenBackup()
    {

    }

    [RelayCommand]
    public async Task Enable901d()
    {

    }

    [RelayCommand]
    public async Task Enable9091()
    {

    }

    [RelayCommand]
    public async Task OpenEmptyFile()
    {

    }

    [RelayCommand]
    public async Task FlashSuperEmpty()
    {

    }

    [RelayCommand]
    public async Task ADBFormat()
    {

    }

    [RelayCommand]
    public async Task FastbootFormat()
    {

    }

    [RelayCommand]
    public async Task FormatData()
    {

    }

    [RelayCommand]
    public async Task TWRPFormatData()
    {

    }

    [RelayCommand]
    public async Task ExtractPart()
    {

    }

    [RelayCommand]
    public async Task ExtractVPart()
    {

    }

    [RelayCommand]
    public async Task OpenExtractFile()
    {

    }
}