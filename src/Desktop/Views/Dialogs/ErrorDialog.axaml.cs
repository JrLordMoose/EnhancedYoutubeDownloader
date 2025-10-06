using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EnhancedYoutubeDownloader.Views.Dialogs;

/// <summary>
/// The view for the error dialog.
/// </summary>
public partial class ErrorDialog : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorDialog"/> class.
    /// </summary>
    public ErrorDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}