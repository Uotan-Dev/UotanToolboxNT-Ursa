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
            IResourceProvider? newLanguage = null;
            var languageKey = language == "zh-CN" || language == "简体中文" ? "zh_CN_Key" : "en_US_Key";

            if (app.TryFindResource(languageKey, out var resource) && resource is IResourceProvider provider)
            {
                newLanguage = provider;
            }
            else
            {
                // 如果没找到，尝试动态加载 (在 AOT 下可能会失败，但作为回退保留)
                try
                {
                    var actualLang = language == "zh-CN" || language == "简体中文" ? "zh-CN" : "en-US";
                    var languageFile = $"avares://UotanToolboxNT_Ursa/Locale/{actualLang}.axaml";
                    newLanguage = (IResourceProvider)AvaloniaXamlLoader.Load(new Uri(languageFile, UriKind.Absolute));
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
                // 如果没找到 ResourceInclude，查找已有的 ResourceDictionary
                IResourceProvider? existingDict = null;
                foreach (var p in targetMergedDictionaries)
                {
                    if (p is ResourceDictionary d && d.Count > 0)
                    {
                        var keys = d.Keys.Cast<object>().ToList();
                        if (keys.Any(k => k.ToString()?.Contains("Home_") == true))
                        {
                            existingDict = p;
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

            // 更新 SemiTheme 的 Locale (不使用反射)
            try
            {
                var langStr = language == "简体中文" || language == "zh-CN" ? "zh-CN" : "en-US";
                var culture = new CultureInfo(langStr);
                foreach (var style in app.Styles)
                {
                    if (style is Semi.Avalonia.SemiTheme s)
                    {
                        s.Locale = culture;
                    }
                    else if (style.GetType().Name == "SemiTheme" && style.GetType().Namespace?.Contains("Ursa") == true)
                    {
                         // Ursa 的 SemiTheme 更新
                         var prop = style.GetType().GetProperty("Locale");
                         prop?.SetValue(style, culture);
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
        if (app == null) return;

        // 修改 RequestedThemeVariant 这最重要，它可以触发应用内大部分样式的自动切换
        app.RequestedThemeVariant = themeKey switch
        {
            "Dark" or "DarkColors" => ThemeVariant.Dark,
            "Light" or "LightColors" => ThemeVariant.Light,
            _ => ThemeVariant.Default
        };

        // 也尝试更新 ResourceDictionary，以防有些地方使用了静态资源
        try
        {
            // 优先查找静态定义的资源 (在 App.axaml 中定义的 LightColors / DarkColors Key)
            if (app.TryFindResource(themeKey, out var resource) && resource is IResourceProvider provider)
            {
                var targetMergedDictionaries = app.Resources.MergedDictionaries;
                var existingThemeInclude = targetMergedDictionaries
                    .OfType<ResourceInclude>()
                    .FirstOrDefault(r => r.Source?.ToString()?.Contains("Themes") == true);

                if (existingThemeInclude != null)
                {
                    var index = targetMergedDictionaries.IndexOf(existingThemeInclude);
                    targetMergedDictionaries[index] = provider;
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
