using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace EnhancedYoutubeDownloader.Converters;

/// <summary>
/// Converts boolean values to strings based on parameter format: "TrueText;FalseText"
/// </summary>
public class BoolToStringConverter : IValueConverter
{
    public static readonly BoolToStringConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue || parameter is not string paramStr)
            return value;

        var parts = paramStr.Split(';');
        if (parts.Length != 2)
            return value;

        return boolValue ? parts[0] : parts[1];
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotSupportedException();
    }
}
