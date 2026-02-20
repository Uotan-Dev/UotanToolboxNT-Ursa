using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using UotanToolboxNT_Ursa.Pages;
using UotanToolboxNT_Ursa.ViewModels;

namespace UotanToolboxNT_Ursa.Converters;

public class ViewLocator : IDataTemplate
{
    bool _isfirst = false;
    public Control? Build(object? param)
    {
        if (param is null) return null;
        var name = param.GetType().Name.Replace("ViewModel", "");
        try
        {
            var type = Type.GetType("UotanToolboxNT_Ursa.Pages." + name);
            if (type != null)
            {
                return Activator.CreateInstance(type) as Control;
            }

            return GetViewByVM(param) ?? new TextBlock { Text = "Not Found : " + name };
        }
        catch (Exception e)
        {
            var reslut = _isfirst
                ? new TextBlock
                {
                    Text = $"Activator.CreateInstance failure ::{e}",
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                }
                : GetViewByVM(param) ?? new TextBlock { Text = "Not Found : " + name };
            _isfirst = false;
            return reslut;
        }
    }

    public bool Match(object? data)
    {
        return true;
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
