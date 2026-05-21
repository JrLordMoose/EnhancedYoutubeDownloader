using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using EnhancedYoutubeDownloader.ViewModels.Dialogs;

namespace EnhancedYoutubeDownloader.Views.Dialogs;

using EnhancedYoutubeDownloader.Utils;

public partial class DownloadSingleSetupDialog : UserControl
{
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
            TraceLog.Write("[DIALOG] Download button clicked");
            TraceLog.Write($"[DIALOG] FilePath: {viewModel.FilePath}");
            TraceLog.Write($"[DIALOG] Video: {viewModel.Video?.Title}");

            if (string.IsNullOrWhiteSpace(viewModel.FilePath))
            {
                TraceLog.Write("[DIALOG] ERROR: FilePath is empty!");
            }

            viewModel.CloseCommand.Execute(true);
            TraceLog.Write("[DIALOG] CloseCommand executed with true");
        }
        else
        {
            TraceLog.Write("[DIALOG] ERROR: DataContext is not DownloadSingleSetupViewModel!");
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
