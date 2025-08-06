using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;

namespace UotanToolboxNT_Ursa.Helper;

internal class ResourceManager
{
    public static void ApplyLanguage(string language)
    {
        try
        {
            var app = Application.Current;
            if (app == null) 
            {
                Console.WriteLine("ResourceManager.ApplyLanguage: Application.Current is null");
                return;
            }

            Console.WriteLine($"ResourceManager.ApplyLanguage: Switching to language '{language}'");

            // 根据语言创建新的资源引用
            var languageFile = $"avares://UotanToolboxNT_Ursa/Locale/{language}.axaml";
            Console.WriteLine($"ResourceManager.ApplyLanguage: Language file path: {languageFile}");

            var newLanguage = new ResourceInclude(new Uri(languageFile, UriKind.Absolute))
            {
                Source = new Uri(languageFile, UriKind.Absolute)
            };

            // 查找并移除现有的语言资源
            var existingLanguage = app.Resources.MergedDictionaries
                .OfType<ResourceInclude>()
                .FirstOrDefault(r => r.Source?.ToString()?.Contains("Locale") == true);

            if (existingLanguage != null)
            {
                Console.WriteLine($"ResourceManager.ApplyLanguage: Removing existing language resource: {existingLanguage.Source}");
                app.Resources.MergedDictionaries.Remove(existingLanguage);
            }
            else
            {
                Console.WriteLine("ResourceManager.ApplyLanguage: No existing language resource found");
            }

            // 将新的语言资源添加到最后，确保它有最高的优先级
            Console.WriteLine("ResourceManager.ApplyLanguage: Adding new language resource");
            app.Resources.MergedDictionaries.Add(newLanguage);
            
            // 强制刷新资源 - 通过添加和移除一个空的资源字典来触发重新加载
            var tempDict = new ResourceDictionary();
            app.Resources.MergedDictionaries.Add(tempDict);
            app.Resources.MergedDictionaries.Remove(tempDict);

            Console.WriteLine($"ResourceManager.ApplyLanguage: Language switch completed. Total merged dictionaries: {app.Resources.MergedDictionaries.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ResourceManager.ApplyLanguage: Error - {ex.Message}");
        }
    }

    public static void ApplyTheme(string themeKey)
    {
        var app = Application.Current;
        if (app == null) 
        {
            return;
        }

        // 根据主题键创建新的资源引用
        var themeFile = themeKey switch
        {
            "LightColors" => "avares://UotanToolboxNT_Ursa/Themes/LightColors.axaml",
            "DarkColors" => "avares://UotanToolboxNT_Ursa/Themes/DarkColors.axaml",
            _ => null
        };

        if (string.IsNullOrEmpty(themeFile)) 
        {
            return;
        }

        var newTheme = new ResourceInclude(new Uri(themeFile, UriKind.Absolute))
        {
            Source = new Uri(themeFile, UriKind.Absolute)
        };

        // 查找并替换现有的主题资源
        var existingTheme = app.Resources.MergedDictionaries
            .OfType<ResourceInclude>()
            .FirstOrDefault(r => r.Source?.ToString()?.Contains("Themes") == true);

        if (existingTheme != null)
        {
            var index = app.Resources.MergedDictionaries.IndexOf(existingTheme);
            app.Resources.MergedDictionaries[index] = newTheme;
        }
        else
        {
            // 如果没有找到现有主题，添加到语言资源之后
            app.Resources.MergedDictionaries.Add(newTheme);
        }
    }
}
