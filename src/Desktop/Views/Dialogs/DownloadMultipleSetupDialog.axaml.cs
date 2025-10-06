using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EnhancedYoutubeDownloader.Views.Dialogs;

/// <summary>
/// The view for the download multiple setup dialog.
/// </summary>
public partial class DownloadMultipleSetupDialog : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadMultipleSetupDialog"/> class.
    /// </summary>
    public DownloadMultipleSetupDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}