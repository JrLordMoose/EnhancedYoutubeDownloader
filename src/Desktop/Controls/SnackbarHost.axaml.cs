using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using EnhancedYoutubeDownloader.Framework;
using Material.Icons;

namespace EnhancedYoutubeDownloader.Controls;

public partial class SnackbarHost : UserControl
{
    public static readonly StyledProperty<ObservableCollection<SnackbarItem>?> SnackbarQueueProperty =
        AvaloniaProperty.Register<SnackbarHost, ObservableCollection<SnackbarItem>?>(
            nameof(SnackbarQueue)
        );

    public ObservableCollection<SnackbarItem>? SnackbarQueue
    {
        get => GetValue(SnackbarQueueProperty);
        set => SetValue(SnackbarQueueProperty, value);
    }

    public SnackbarHost()
    {
        InitializeComponent();

        // Register converters
        Resources["SeverityToBrushConverter"] = new SeverityToBrushConverter();
        Resources["SeverityToIconConverter"] = new SeverityToIconConverter();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

/// <summary>
/// Converts SnackbarSeverity to a color brush
/// </summary>
public class SeverityToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SnackbarSeverity severity)
        {
            return severity switch
            {
                SnackbarSeverity.Info => new SolidColorBrush(Color.Parse("#2196F3")),
                SnackbarSeverity.Success => new SolidColorBrush(Color.Parse("#4CAF50")),
                SnackbarSeverity.Warning => new SolidColorBrush(Color.Parse("#FF9800")),
                SnackbarSeverity.Error => new SolidColorBrush(Color.Parse("#F44336")),
                _ => new SolidColorBrush(Color.Parse("#2196F3")),
            };
        }
        return new SolidColorBrush(Color.Parse("#2196F3"));
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts SnackbarSeverity to a Material Icon
/// </summary>
public class SeverityToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SnackbarSeverity severity)
        {
            return severity switch
            {
                SnackbarSeverity.Info => MaterialIconKind.Information,
                SnackbarSeverity.Success => MaterialIconKind.CheckCircle,
                SnackbarSeverity.Warning => MaterialIconKind.Alert,
                SnackbarSeverity.Error => MaterialIconKind.AlertCircle,
                _ => MaterialIconKind.Information,
            };
        }
        return MaterialIconKind.Information;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }
}
