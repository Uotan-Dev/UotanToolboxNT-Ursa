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
            if (app.TryFindResource(language, out var resource) && resource is ResourceDictionary dict)
            {
                newLanguage = dict;
            }
            else
            {
                // 如果没找到，尝试动态加载 (在 AOT 下可能会失败，但作为回退保留)
                var languageFile = $"avares://UotanToolboxNT_Ursa/Locale/{language}.axaml";
                newLanguage = (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri(languageFile, UriKind.Absolute));
            }

            if (newLanguage == null) return;

            var targetMergedDictionaries = app.Resources.MergedDictionaries;
            // 兼容目前 App.axaml 中嵌套一层 ResourceDictionary 的结构
            if (targetMergedDictionaries.Count > 0 && targetMergedDictionaries[0] is ResourceDictionary innerDict)
            {
                targetMergedDictionaries = innerDict.MergedDictionaries;
            }

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
                // 我们避开 ResourceInclude 类型本身
                ResourceDictionary? existingDict = null;
                foreach (var provider in targetMergedDictionaries)
                {
                    if (provider is ResourceDictionary d && d.GetType() != typeof(ResourceInclude) && d.Count > 0)
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
            var culture = new CultureInfo(language == "简体中文" || language == "zh-CN" ? "zh-CN" : "en-US");
            foreach (var style in app.Styles)
            {
                if (style is Semi.Avalonia.SemiTheme s) s.Locale = culture;
                if (style.GetType().Name == "SemiTheme" && style.GetType().Namespace?.Contains("Ursa") == true)
                {
                    var pi = style.GetType().GetProperty("Locale");
                    pi?.SetValue(style, culture);
                }
            }

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
                if (targetMergedDictionaries.Count > 0 && targetMergedDictionaries[0] is ResourceDictionary innerDict)
                {
                    targetMergedDictionaries = innerDict.MergedDictionaries;
                }

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
                        if (provider is ResourceDictionary d && d.GetType() != typeof(ResourceInclude))
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
        catch
        {
            // 对于 AOT，如果动态加载失败没关系，因为 RequestedThemeVariant 已经通过 ThemeDictionaries 工作了
        }

        // 触发主题变更事件
        ThemeChanged?.Invoke(null, themeKey);
    }
}
