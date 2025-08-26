using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;

namespace UotanToolboxNT_Ursa.Converters.WaveProgress;

public class WaveProgressValueColorConverter : IValueConverter
{
    public static readonly WaveProgressValueColorConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not double d
            ? Brushes.Black
            : d > 50
            ? Brushes.GhostWhite
            : Application.Current?.ActualThemeVariant == ThemeVariant.Dark ? Brushes.GhostWhite : Brushes.Black;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}