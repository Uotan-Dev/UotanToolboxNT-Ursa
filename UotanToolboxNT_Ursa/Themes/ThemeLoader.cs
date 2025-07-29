using Avalonia;
using Avalonia.Controls;

namespace UotanToolboxNT_Ursa.Themes;

internal class ThemeLoader
{
    public static void ApplyTheme(string themeKey)
    {
        var app = Application.Current;
        if (app == null)
        {
            return;
        }

        // 获取预加载的主题资源 [1]()

        if (app.Resources[themeKey] is not ResourceDictionary newTheme)
        {
            return;
        }

        // 修复：正确处理资源合并 [7]()
        app.Resources.MergedDictionaries.Clear();
        app.Resources.MergedDictionaries.Add(newTheme);

        // 修复：显式设置主题变体 [9]()
        //app.RequestedThemeVariant = themeKey.Contains("Dark")
        //    ? ThemeVariant.Dark
        //    : ThemeVariant.Light;
    }
}
