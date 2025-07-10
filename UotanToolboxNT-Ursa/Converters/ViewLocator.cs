using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace UotanToolboxNT_Ursa.Converters;

public class ViewLocator: IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null) return null;
        var name = param.GetType().Name.Replace("ViewModel", "");
        var type = Type.GetType("UotanToolboxNT_Ursa.Pages." + name);
        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return true;
    }
}