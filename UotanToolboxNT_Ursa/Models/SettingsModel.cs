using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using UotanToolboxNT_Ursa.Helper;
using static UotanToolboxNT_Ursa.Models.GlobalLogModel;

namespace UotanToolboxNT_Ursa.Models;

public partial class SettingsModel
{
    // 想要保存的设置属性就添加到这里，然后下面的new方法也要添加对应的默认值。最后再在vm那里添加对应的属性绑定
    [JsonPropertyName("isLightTheme")]
    public bool IsLightTheme { get; set; }

    [JsonPropertyName("selectedLanguageList")]
    public string SelectedLanguageList { get; set; } = "Settings_Default";

    // 保存设置到 JSON 文件
    public static void Save(SettingsModel settings)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(settings, SettingsJsonContext.Default.SettingsModel);
            
            // 确保目录存在
            var directory = Path.GetDirectoryName(Global.SettingsFile.FullName);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(Global.SettingsFile.FullName, json);
        }
        catch (Exception ex)
        {
            AddLog($"保存设置失败: {ex.Message}", LogLevel.Error);
        }
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
        return JsonSerializer.Deserialize(json, SettingsJsonContext.Default.SettingsModel) ?? new SettingsModel();
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(SettingsModel))]
    internal partial class SettingsJsonContext : JsonSerializerContext
    {
    }

    public static void ChangeLaguage(string value)
    {
        if (value == "Settings_Default")
        {
            var systemCulture = System.Globalization.CultureInfo.CurrentUICulture.Name;
            if (systemCulture.StartsWith("zh"))
            {
                AddLog($"检测到系统语言为中文，设置为中文", LogLevel.Info);
                ResourceManager.ApplyLanguage("zh-CN");
            }
            else if (systemCulture.StartsWith("en"))
            {
                AddLog($"检测到系统语言为英文，设置为英文", LogLevel.Info);
                ResourceManager.ApplyLanguage("en-US");
            }
            else
            {
                AddLog($"系统语言 {systemCulture} 不受支持，默认设置为中文", LogLevel.Info);
                ResourceManager.ApplyLanguage("zh-CN");
            }
        }
        else if (value == "English")
        {
            ResourceManager.ApplyLanguage("en-US");
        }
        else if (value == "简体中文")
        {
            ResourceManager.ApplyLanguage("zh-CN");
        }
        else
        {
            AddLog($"未知语言 {value}，无法切换", LogLevel.Warning);
            return;
        }
    }

    internal static void ChangeTheme(string themeKey)
    {
        if (themeKey == "LightColors")
        {
            ResourceManager.ApplyTheme("LightColors");
        }
        else if (themeKey == "DarkColors")
        {
            ResourceManager.ApplyTheme("DarkColors");
        }
        else
        {
            AddLog($"未知主题 {themeKey}，无法切换", LogLevel.Warning);
            return;
        }
    }
}