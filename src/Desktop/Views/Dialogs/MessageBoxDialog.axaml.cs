using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EnhancedYoutubeDownloader.Views.Dialogs;

/// <summary>
/// The view for the message box dialog.
/// </summary>
public partial class MessageBoxDialog : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBoxDialog"/> class.
    /// </summary>
    public MessageBoxDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}