using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using UotanToolboxNT_Ursa.Pages;
using UotanToolboxNT_Ursa.ViewModels;

namespace UotanToolboxNT_Ursa.Converters;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
        {
            return null;
        }

        return param switch
        {
            HomeViewModel => new Home(),
            BasicflashViewModel => new Basicflash(),
            AppmgrViewModel => new Appmgr(),
            WiredflashViewModel => new Wiredflash(),
            CustomizedflashViewModel => new Customizedflash(),
            ScrcpyViewModel => new Scrcpy(),
            FormatExtractViewModel => new FormatExtract(),
            ModifypartitionViewModel => new Modifypartition(),
            GlobalLogViewModel => new GlobalLog(),
            SettingsViewModel => new Settings(),
            _ => new TextBlock { Text = "Not Found: " + param.GetType().Name }
        };
    }

    public bool Match(object? data) => data is ObservableObject;
}
