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

/// <summary>
/// A control that displays a queue of snackbar messages.
/// </summary>
public partial class SnackbarHost : UserControl
{
    /// <summary>
    /// Defines the <see cref="SnackbarQueue"/> property.
    /// </summary>
    public static readonly StyledProperty<ObservableCollection<SnackbarItem>?> SnackbarQueueProperty =
        AvaloniaProperty.Register<SnackbarHost, ObservableCollection<SnackbarItem>?>(
            nameof(SnackbarQueue)
        );

    /// <summary>
    /// Gets or sets the queue of snackbar items to display.
    /// </summary>
    public ObservableCollection<SnackbarItem>? SnackbarQueue
    {
        get => GetValue(SnackbarQueueProperty);
        set => SetValue(SnackbarQueueProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SnackbarHost"/> class.
    /// </summary>
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
/// Converts <see cref="SnackbarSeverity"/> to a color brush.
/// </summary>
public class SeverityToBrushConverter : IValueConverter
{
    /// <summary>
    /// Converts a <see cref="SnackbarSeverity"/> to a corresponding <see cref="IBrush"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The type of the target property.</param>
    /// <param name="parameter">An optional parameter.</param>
    /// <param name="culture">The culture to use for the conversion.</param>
    /// <returns>An <see cref="IBrush"/> representing the severity color.</returns>
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

    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <param name="value">The value to convert back.</param>
    /// <param name="targetType">The type of the target property.</param>
    /// <param name="parameter">An optional parameter.</param>
    /// <param name="culture">The culture to use for the conversion.</param>
    /// <returns>Throws <see cref="NotImplementedException"/>.</returns>
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
/// Converts <see cref="SnackbarSeverity"/> to a Material Icon.
/// </summary>
public class SeverityToIconConverter : IValueConverter
{
    /// <summary>
    /// Converts a <see cref="SnackbarSeverity"/> to a corresponding <see cref="MaterialIconKind"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The type of the target property.</param>
    /// <param name="parameter">An optional parameter.</param>
    /// <param name="culture">The culture to use for the conversion.</param>
    /// <returns>A <see cref="MaterialIconKind"/> representing the severity.</returns>
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

    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <param name="value">The value to convert back.</param>
    /// <param name="targetType">The type of the target property.</param>
    /// <param name="parameter">An optional parameter.</param>
    /// <param name="culture">The culture to use for the conversion.</param>
    /// <returns>Throws <see cref="NotImplementedException"/>.</returns>
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