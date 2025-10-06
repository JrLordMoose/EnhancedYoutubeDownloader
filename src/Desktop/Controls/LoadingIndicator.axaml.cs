using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EnhancedYoutubeDownloader.Controls;

/// <summary>
/// A control that displays a loading indicator with a message and subtext.
/// </summary>
public partial class LoadingIndicator : UserControl
{
    /// <summary>
    /// Defines the <see cref="IsLoading"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsLoadingProperty = AvaloniaProperty.Register<
        LoadingIndicator,
        bool
    >(nameof(IsLoading), defaultValue: false);

    /// <summary>
    /// Defines the <see cref="LoadingMessage"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> LoadingMessageProperty =
        AvaloniaProperty.Register<LoadingIndicator, string?>(nameof(LoadingMessage), "Loading...");

    /// <summary>
    /// Defines the <see cref="LoadingSubtext"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> LoadingSubtextProperty =
        AvaloniaProperty.Register<LoadingIndicator, string?>(nameof(LoadingSubtext));

    /// <summary>
    /// Gets or sets a value indicating whether the loading indicator is visible.
    /// </summary>
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>
    /// Gets or sets the main loading message.
    /// </summary>
    public string? LoadingMessage
    {
        get => GetValue(LoadingMessageProperty);
        set => SetValue(LoadingMessageProperty, value);
    }

    /// <summary>
    /// Gets or sets the loading subtext.
    /// </summary>
    public string? LoadingSubtext
    {
        get => GetValue(LoadingSubtextProperty);
        set => SetValue(LoadingSubtextProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadingIndicator"/> class.
    /// </summary>
    public LoadingIndicator()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}