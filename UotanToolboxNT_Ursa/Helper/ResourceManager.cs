using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
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

            // 优先查找在 App.axaml 中静态定义的资源，这样更能适配 Native AOT
            ResourceDictionary? newLanguage = null;
            var languageKey = language == "zh-CN" || language == "简体中文" ? "zh_CN_Key" : "en_US_Key";

            if (app.TryFindResource(languageKey, out var resource) && resource is ResourceDictionary dict)
            {
                newLanguage = dict;
            }
            else
            {
                // 如果没找到，尝试动态加载 (在 AOT 下可能会失败，但作为回退保留)
                try
                {
                    var actualLang = language == "zh-CN" || language == "简体中文" ? "zh-CN" : "en-US";
                    var languageFile = $"avares://UotanToolboxNT_Ursa/Locale/{actualLang}.axaml";
                    newLanguage = (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri(languageFile, UriKind.Absolute));
                }
                catch (Exception ex)
                {
                    AddLog($"ResourceManager.ApplyLanguage: Dynamic Load Error - {ex.Message}", LogLevel.Error);
                }
            }

            if (newLanguage == null) return;

            var targetMergedDictionaries = app.Resources.MergedDictionaries;

            var existingLanguageInclude = targetMergedDictionaries
                .OfType<ResourceInclude>()
                .FirstOrDefault(r => r.Source?.ToString()?.Contains("Locale") == true);

            if (existingLanguageInclude != null)
            {
                var index = targetMergedDictionaries.IndexOf(existingLanguageInclude);
                targetMergedDictionaries[index] = newLanguage;
            }
            else
            {
                // 如果没找 ResourceInclude，查找已有的 ResourceDictionary
                ResourceDictionary? existingDict = null;
                foreach (var provider in targetMergedDictionaries)
                {
                    if (provider is ResourceDictionary d && d.GetType() == typeof(ResourceDictionary) && d.Count > 0)
                    {
                        var keys = d.Keys.Cast<object>().ToList();
                        if (keys.Any(k => k.ToString()?.Contains("Home_") == true))
                        {
                            existingDict = d;
                            break;
                        }
                    }
                }
                
                if (existingDict != null)
                {
                    var index = targetMergedDictionaries.IndexOf(existingDict);
                    targetMergedDictionaries[index] = newLanguage;
                }
                else
                {
                    // 默认 locale
                    targetMergedDictionaries.Add(newLanguage);
                }
            }

            // 更新 SemiTheme 的 Locale
            try
            {
                var langStr = language == "简体中文" || language == "zh-CN" ? "zh-CN" : "en-US";
                var culture = new CultureInfo(langStr);
                foreach (var style in app.Styles)
                {
                    if (style is Semi.Avalonia.SemiTheme s) s.Locale = culture;
                    // 使用反射适配 Ursa 的 SemiTheme
                    if (style.GetType().Name == "SemiTheme" && style.GetType().Namespace?.Contains("Ursa") == true)
                    {
                        var pi = style.GetType().GetProperty("Locale");
                        pi?.SetValue(style, culture);
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"ResourceManager.ApplyLanguage: Locale Style Update Error - {ex.Message}", LogLevel.Warning);
            }

            // 触发语言变更事件
            LanguageChanged?.Invoke(null, language);
        }
        catch (Exception ex)
        {
            AddLog($"ResourceManager.ApplyLanguage: Critical Error - {ex.Message}", LogLevel.Error);
        }
    }

    public static void ApplyTheme(string themeKey)
    {
        var app = Application.Current;
        if (app == null)
        {
            return;
        }

        // 修改 RequestedThemeVariant 这是最重要的，它可以触发应用内大部分样式的自动切换
        app.RequestedThemeVariant = themeKey switch
        {
            "LightColors" => ThemeVariant.Light,
            "DarkColors" => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };

        // 同时也尝试替换旧有的颜色资源（如果代码其他地方有用到）
        try
        {
            var themeFile = themeKey switch
            {
                "LightColors" => "avares://UotanToolboxNT_Ursa/Themes/LightColors.axaml",
                "DarkColors" => "avares://UotanToolboxNT_Ursa/Themes/DarkColors.axaml",
                _ => null
            };

            if (!string.IsNullOrEmpty(themeFile))
            {
                var newTheme = (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri(themeFile, UriKind.Absolute));
                var targetMergedDictionaries = app.Resources.MergedDictionaries;

                var existingThemeInclude = targetMergedDictionaries
                    .OfType<ResourceInclude>()
                    .FirstOrDefault(r => r.Source?.ToString()?.Contains("Themes") == true);

                if (existingThemeInclude != null)
                {
                    var index = targetMergedDictionaries.IndexOf(existingThemeInclude);
                    targetMergedDictionaries[index] = newTheme;
                }
                else
                {
                    ResourceDictionary? existingDict = null;
                    foreach (var provider in targetMergedDictionaries)
                    {
                        if (provider is ResourceDictionary d && d.GetType() == typeof(ResourceDictionary))
                        {
                            var keys = d.Keys.Cast<object>().ToList();
                            if (keys.Any(k => k.ToString() == "PageBackground"))
                            {
                                existingDict = d;
                                break;
                            }
                        }
                    }

                    if (existingDict != null)
                    {
                        var index = targetMergedDictionaries.IndexOf(existingDict);
                        targetMergedDictionaries[index] = newTheme;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            AddLog($"ResourceManager.ApplyTheme: Error - {ex.Message}", LogLevel.Warning);
        }

        // 触发主题变更事件
        ThemeChanged?.Invoke(null, themeKey);
    }
}
