using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using static UotanToolboxNT_Ursa.Models.GlobalLogModel;

namespace UotanToolboxNT_Ursa.Helper;

internal class ResourceManager
{
    /// <summary>
    /// 语言变更事件
    /// </summary>
    public static event EventHandler<string>? LanguageChanged;

    /// <summary>
    /// 主题变更事件
    /// </summary>
    public static event EventHandler<string>? ThemeChanged;

    public static void ApplyLanguage(string language)
    {
        try
        {
            var app = Application.Current;
            if (app == null)
            {
                AddLog("ResourceManager.ApplyLanguage: Application.Current is null");
                return;
            }

            var languageFile = $"avares://UotanToolboxNT_Ursa/Locale/{language}.axaml";

            var newLanguage = new ResourceInclude(new Uri(languageFile, UriKind.Absolute))
            {
                Source = new Uri(languageFile, UriKind.Absolute)
            };

            var existingLanguage = app.Resources.MergedDictionaries
                .OfType<ResourceInclude>()
                .FirstOrDefault(r => r.Source?.ToString()?.Contains("Locale") == true);

            if (existingLanguage != null)
            {
                app.Resources.MergedDictionaries.Remove(existingLanguage);
            }
            app.Resources.MergedDictionaries.Add(newLanguage);

            var tempDict = new ResourceDictionary();
            app.Resources.MergedDictionaries.Add(tempDict);
            app.Resources.MergedDictionaries.Remove(tempDict);

            // 触发语言变更事件
            LanguageChanged?.Invoke(null, language);
        }
        catch (Exception ex)
        {
            AddLog($"ResourceManager.ApplyLanguage: Error - {ex.Message}", LogLevel.Error);
        }
    }

    public static void ApplyTheme(string themeKey)
    {
        var app = Application.Current;
        if (app == null)
        {
            return;
        }

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
            app.Resources.MergedDictionaries.Add(newTheme);
        }

        // 触发主题变更事件
        ThemeChanged?.Invoke(null, themeKey);
    }
}
