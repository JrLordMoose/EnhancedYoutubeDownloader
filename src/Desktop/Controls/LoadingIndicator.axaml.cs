using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EnhancedYoutubeDownloader.Controls;

public partial class LoadingIndicator : UserControl
{
    public static readonly StyledProperty<bool> IsLoadingProperty = AvaloniaProperty.Register<
        LoadingIndicator,
        bool
    >(nameof(IsLoading), defaultValue: false);

    public static readonly StyledProperty<string?> LoadingMessageProperty =
        AvaloniaProperty.Register<LoadingIndicator, string?>(nameof(LoadingMessage), "Loading...");

    public static readonly StyledProperty<string?> LoadingSubtextProperty =
        AvaloniaProperty.Register<LoadingIndicator, string?>(nameof(LoadingSubtext));

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public string? LoadingMessage
    {
        get => GetValue(LoadingMessageProperty);
        set => SetValue(LoadingMessageProperty, value);
    }

    public string? LoadingSubtext
    {
        get => GetValue(LoadingSubtextProperty);
        set => SetValue(LoadingSubtextProperty, value);
    }

    public LoadingIndicator()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
