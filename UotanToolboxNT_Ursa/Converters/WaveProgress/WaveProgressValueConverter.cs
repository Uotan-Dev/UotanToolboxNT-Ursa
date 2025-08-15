using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace UotanToolboxNT_Ursa.Converters.WaveProgress;

public class WaveProgressValueConverter : IValueConverter
{
    public static readonly WaveProgressValueConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not double i ? 0 : (object)(155 - i * 2.1);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}