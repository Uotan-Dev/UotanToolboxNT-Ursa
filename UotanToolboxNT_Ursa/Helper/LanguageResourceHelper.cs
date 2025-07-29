using Avalonia.Controls;

namespace UotanToolboxNT_Ursa.Helper;
internal class LanguageResourceHelper
{
    public static T GetLanguageResource<T>(string name)
    {
        try
        {
            Avalonia.Application.Current!.TryFindResource(name, out var value);
            return (T)value;
        }
        catch
        {
            return typeof(T) == typeof(string) ? (T)(object)"??" : default!;
        }
    }
}
