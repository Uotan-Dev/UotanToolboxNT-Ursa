using UotanToolboxNT_Ursa.Helper;

namespace UotanToolboxNT_Ursa.Themes;

internal class ThemeLoader
{
    public static void ApplyTheme(string themeKey)
    {
        ResourceManager.ApplyTheme(themeKey);
    }
}
