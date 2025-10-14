using System;
using System.Globalization;
using Avalonia.Data.Converters;
using EnhancedYoutubeDownloader.Shared.Models;

namespace EnhancedYoutubeDownloader.Converters;

/// <summary>
/// Converts PlatformType enum to platform-specific emoji icon for thumbnail placeholders
/// </summary>
public class PlatformToIconConverter : IMultiValueConverter
{
    public object? Convert(
        IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if (values.Count > 0 && values[0] is PlatformType platform)
        {
            return platform switch
            {
                PlatformType.YouTube => "📺",
                PlatformType.TikTok => "🎵",
                PlatformType.Instagram => "📷",
                PlatformType.Twitter => "🐦",
                PlatformType.Generic => "🎬",
                _ => "🎬",
            };
        }

        return "🎬"; // Default fallback
    }

    public object[] ConvertBack(
        object value,
        Type[] targetTypes,
        object parameter,
        CultureInfo culture
    )
    {
        throw new NotSupportedException();
    }
}
