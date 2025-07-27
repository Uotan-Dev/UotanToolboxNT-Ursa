using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UotanToolboxNT_Ursa.Models;

public class SettingsModel
{
    //想要保存的设置属性就添加到这里，然后下面的new方法也要添加对应的默认值。最后再在vm那里添加对应的属性绑定
    [JsonPropertyName("isLightTheme")]
    public bool IsLightTheme { get; set; }

    [JsonPropertyName("selectedLanguageList")]
    public string SelectedLanguageList { get; set; }

    // 保存设置到 JSON 文件
    public static void Save(SettingsModel settings)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(settings, options);
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
        return JsonSerializer.Deserialize<SettingsModel>(json) ?? new SettingsModel();
    }
}