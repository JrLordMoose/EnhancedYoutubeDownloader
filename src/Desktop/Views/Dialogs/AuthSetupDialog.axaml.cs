using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EnhancedYoutubeDownloader.Views.Dialogs;

/// <summary>
/// The view for the authentication setup dialog.
/// </summary>
public partial class AuthSetupDialog : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthSetupDialog"/> class.
    /// </summary>
    public AuthSetupDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}