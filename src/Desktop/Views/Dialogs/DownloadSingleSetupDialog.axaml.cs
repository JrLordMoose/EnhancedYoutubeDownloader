using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using EnhancedYoutubeDownloader.ViewModels.Dialogs;

namespace EnhancedYoutubeDownloader.Views.Dialogs;

/// <summary>
/// The view for the download single setup dialog.
/// </summary>
public partial class DownloadSingleSetupDialog : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadSingleSetupDialog"/> class.
    /// </summary>
    public DownloadSingleSetupDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        // Wire up buttons
        var browseButton = this.FindControl<Button>("BrowseButton");
        var downloadButton = this.FindControl<Button>("DownloadButton");
        var cancelButton = this.FindControl<Button>("CancelButton");

        if (browseButton != null)
        {
            browseButton.Click += BrowseButton_Click;
        }

        if (downloadButton != null)
        {
            downloadButton.Click += DownloadButton_Click;
        }

        if (cancelButton != null)
        {
            cancelButton.Click += CancelButton_Click;
        }
    }

    private async void BrowseButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is DownloadSingleSetupViewModel viewModel)
        {
            await viewModel.BrowseCommand.ExecuteAsync(null);
        }
    }

    private void DownloadButton_Click(object? sender, RoutedEventArgs e)
    {
        // Return true to indicate user confirmed download
        if (DataContext is DownloadSingleSetupViewModel viewModel)
        {
            Console.WriteLine("[DIALOG] Download button clicked");
            Console.WriteLine($"[DIALOG] FilePath: {viewModel.FilePath}");
            Console.WriteLine($"[DIALOG] Video: {viewModel.Video?.Title}");

            if (string.IsNullOrWhiteSpace(viewModel.FilePath))
            {
                Console.WriteLine("[DIALOG] ERROR: FilePath is empty!");
            }

            viewModel.CloseCommand.Execute(true);
            Console.WriteLine("[DIALOG] CloseCommand executed with true");
        }
        else
        {
            Console.WriteLine("[DIALOG] ERROR: DataContext is not DownloadSingleSetupViewModel!");
        }
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        // Return false to indicate user cancelled
        if (DataContext is DownloadSingleSetupViewModel viewModel)
        {
            viewModel.CloseCommand.Execute(false);
        }
    }
}