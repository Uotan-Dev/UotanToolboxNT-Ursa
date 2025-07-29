using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Markup.Xaml.Styling;

namespace UotanToolboxNT_Ursa.Models;

public class SettingsModel
{
    // 想要保存的设置属性就添加到这里，然后下面的new方法也要添加对应的默认值。最后再在vm那里添加对应的属性绑定
    [JsonPropertyName("isLightTheme")]
    public bool IsLightTheme { get; set; }

    [JsonPropertyName("selectedLanguageList")]
    public string SelectedLanguageList { get; set; }

    private static readonly JsonSerializerOptions CachedJsonOptions = new() { WriteIndented = true };

    // 保存设置到 JSON 文件
    public static void Save(SettingsModel settings)
    {
        var json = JsonSerializer.Serialize(settings, CachedJsonOptions);
        File.WriteAllText(Global.SettingsFile.FullName, json);
    }

    // 从 JSON 文件加载设置
    public static SettingsModel Load()
    {
        if (!File.Exists(Global.SettingsFile.FullName))
        {
            return new SettingsModel
            {
                IsLightTheme = true,
                SelectedLanguageList = "Settings_Default"
            };
        }

        var json = File.ReadAllText(Global.SettingsFile.FullName);
        return JsonSerializer.Deserialize<SettingsModel>(json, CachedJsonOptions) ?? new SettingsModel();
    }
    private static void UpdateaAxaml(string language)
    {
        var file = $"avares://UotanToolboxNT_Ursa/Locale/{language}.axaml";
        var data = new ResourceInclude(new Uri(file, UriKind.Absolute))
        {
            Source = new Uri(file, UriKind.Absolute)
        };
        Avalonia.Application.Current!.Resources.MergedDictionaries[0] = data;
    }
    public static void ChangeLaguage(string value)
    {
        if (value == "Settings_Default")
        {
            var systemCulture = System.Globalization.CultureInfo.CurrentUICulture.Name;
            if (systemCulture.StartsWith("zh"))
            {
                GlobalLogModel.AddLog($"检测到系统语言为中文，设置为中文", GlobalLogModel.LogLevel.Info);
                UpdateaAxaml("zh-CN");
            }
            else if (systemCulture.StartsWith("en"))
            {
                GlobalLogModel.AddLog($"检测到系统语言为英文，设置为英文", GlobalLogModel.LogLevel.Info);
                UpdateaAxaml("en-US");
            }
            else
            {
                GlobalLogModel.AddLog($"系统语言 {systemCulture} 不受支持，默认设置为中文", GlobalLogModel.LogLevel.Info);
                UpdateaAxaml("zh-CN");
            }
        }
        else if (value == "English")
        {
            UpdateaAxaml("en-US");
        }
        else if (value == "简体中文")
        {
            UpdateaAxaml("zh-CN");
        }
        else
        {
            GlobalLogModel.AddLog($"未知语言 {value}，无法切换", GlobalLogModel.LogLevel.Warning);
            return;
        }
    }
}