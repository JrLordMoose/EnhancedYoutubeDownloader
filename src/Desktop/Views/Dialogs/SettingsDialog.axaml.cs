using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EnhancedYoutubeDownloader.Views.Dialogs;

/// <summary>
/// The view for the settings dialog.
/// </summary>
public partial class SettingsDialog : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsDialog"/> class.
    /// </summary>
    public SettingsDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}