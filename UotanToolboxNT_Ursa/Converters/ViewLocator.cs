using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using UotanToolboxNT_Ursa.Pages;
using UotanToolboxNT_Ursa.ViewModels;

namespace UotanToolboxNT_Ursa.Converters;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null) return null;
        
        var view = GetViewByVM(param);
        if (view != null) return view;

        var name = param.GetType().Name.Replace("ViewModel", "");
        return new TextBlock { Text = "Not Found : " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }

    private Control? GetViewByVM(object vm)
    {
        return vm switch
        {
            AppmgrViewModel => new Appmgr(),
            BasicflashViewModel => new Basicflash(),
            CustomizedflashViewModel => new Customizedflash(),
            FormatExtractViewModel => new FormatExtract(),
            GlobalLogViewModel => new GlobalLog(),
            HomeViewModel => new Home(),
            ModifypartitionViewModel => new Modifypartition(),
            ScrcpyViewModel => new Scrcpy(),
            SettingsViewModel => new Settings(),
            WiredflashViewModel => new Wiredflash(),
            _ => null
        };
    }
}
}
