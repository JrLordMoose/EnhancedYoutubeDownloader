using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DialogHostAvalonia;

namespace EnhancedYoutubeDownloader.Views.Dialogs;

public partial class AuthSetupDialog : UserControl
{
    public AuthSetupDialog()
    {
        InitializeComponent();

        // Wire up Close button
        var closeButton = this.FindControl<Button>("CloseButton");
        if (closeButton != null)
        {
            closeButton.Click += OnCloseClicked;
        }

        // Wire up Clear Auth button (placeholder for now)
        var clearAuthButton = this.FindControl<Button>("ClearAuthButton");
        if (clearAuthButton != null)
        {
            clearAuthButton.Click += OnClearAuthClicked;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        // Close the dialog
        DialogHost.Close("RootDialog");
    }

    private void OnClearAuthClicked(object? sender, RoutedEventArgs e)
    {
        // Placeholder - no auth to clear yet since WebView is not implemented
        // In the future, this would clear stored cookies/tokens
    }
}
