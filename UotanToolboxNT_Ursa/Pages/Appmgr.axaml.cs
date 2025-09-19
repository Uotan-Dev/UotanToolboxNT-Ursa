using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using UotanToolboxNT_Ursa.Models;
using UotanToolboxNT_Ursa.ViewModels;
using static UotanToolboxNT_Ursa.Models.GlobalLogModel;

namespace UotanToolboxNT_Ursa.Pages;

public partial class Appmgr : UserControl
{
    private readonly AppmgrViewModel _viewModel;

    /// <summary>
    /// APK文件类型选择器
    /// </summary>
    private static readonly FilePickerFileType ApkPicker = new("APP File")
    {
        Patterns = ["*.apk", "*.hap"],
        AppleUniformTypeIdentifiers = ["*.apk", "*.hap"]
    };

    public Appmgr()
    {
        InitializeComponent();
        _viewModel = new AppmgrViewModel();
        DataContext = _viewModel;
        _viewModel.FileSelectionHandler = SelectApkFileAsync;
    }

    /// <summary>
    /// 选择APK文件的具体实现
    /// </summary>
    private async Task SelectApkFileAsync()
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.StorageProvider != null)
            {
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "选择APK文件",
                    AllowMultiple = true,
                    FileTypeFilter = [ApkPicker, FilePickerFileTypes.All]
                });

                if (files.Count >= 1)
                {
                    var selectedFiles = string.Empty;
                    for (var i = 0; i < files.Count; i++)
                    {
                        var filePath = files[i].TryGetLocalPath();
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            selectedFiles += filePath;
                            if (i < files.Count - 1)
                            {
                                selectedFiles += "|||";
                            }
                        }
                    }
                    AddLog($"选择应用：{selectedFiles}");
                    _viewModel.SetSelectedApkFile(selectedFiles);
                }
            }
        }
        catch (Exception ex)
        {
            AddLog($"选择应用时发生异常：{ex.Message}", LogLevel.Error);
        }
    }

    private async void UninstallButton_Click(object sender, RoutedEventArgs e)
    {
        Button button = (Button)sender;
        ApplicationInfo applicationInfo = (ApplicationInfo)button.DataContext;
        await UninstallApplication(applicationInfo.Name);
    }

    private async Task UninstallApplication(string packageName)
    {

    }

    public async void CopyButton_OnClick(object sender, RoutedEventArgs args)
    {

    }
}