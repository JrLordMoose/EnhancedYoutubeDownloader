using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DialogHostAvalonia;

namespace EnhancedYoutubeDownloader.Views.Dialogs;

public partial class DownloadMultipleSetupDialog : UserControl
{
    public DownloadMultipleSetupDialog()
    {
        InitializeComponent();

        // Wire up Cancel button
        var cancelButton = this.FindControl<Button>("CancelButton");
        if (cancelButton != null)
        {
            cancelButton.Click += OnCancelClicked;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        // Close the dialog with false result
        DialogHost.Close("RootDialog", false);
    }
}
