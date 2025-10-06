using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnhancedYoutubeDownloader.Framework;
using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

/// <summary>
/// The view model for the download single setup dialog.
/// </summary>
public partial class DownloadSingleSetupViewModel : DialogViewModelBase
{
    private readonly DialogManager _dialogManager;

    /// <summary>
    /// Gets or sets the video to download.
    /// </summary>
    [ObservableProperty]
    private IVideo? _video;

    /// <summary>
    /// Gets or sets the download options.
    /// </summary>
    [ObservableProperty]
    private List<object> _downloadOptions = new();

    /// <summary>
    /// Gets or sets the file path to save the download to.
    /// </summary>
    [ObservableProperty]
    private string _filePath = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to download subtitles.
    /// </summary>
    [ObservableProperty]
    private bool _downloadSubtitles = true;

    /// <summary>
    /// Gets or sets a value indicating whether to inject tags.
    /// </summary>
    [ObservableProperty]
    private bool _injectTags = true;

    /// <summary>
    /// Gets or sets the selected quality.
    /// </summary>
    [ObservableProperty]
    private string _selectedQuality = "Best Quality";

    /// <summary>
    /// Gets or sets the selected format.
    /// </summary>
    [ObservableProperty]
    private string _selectedFormat = "MP4";

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadSingleSetupViewModel"/> class.
    /// </summary>
    /// <param name="dialogManager">The dialog manager.</param>
    public DownloadSingleSetupViewModel(DialogManager dialogManager)
    {
        _dialogManager = dialogManager;
    }

    /// <summary>
    /// Opens a file browser to select a download location.
    /// </summary>
    [RelayCommand]
    private async Task BrowseAsync()
    {
        if (Video == null)
            return;

        // Sanitize filename
        var sanitizedTitle = SanitizeFileName(Video.Title);
        var defaultFileName = $"{sanitizedTitle}.{SelectedFormat.ToLower()}";

        // Show file picker
        var selectedPath = await _dialogManager.PromptSaveFilePathAsync(
            defaultFileName,
            new Dictionary<string, string[]>
            {
                { $"{SelectedFormat} Video", new[] { $".{SelectedFormat.ToLower()}" } },
                { "All Files", new[] { "*" } },
            }
        );

        if (!string.IsNullOrWhiteSpace(selectedPath))
        {
            FilePath = selectedPath;
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var c in invalidChars)
        {
            fileName = fileName.Replace(c, '_');
        }

        fileName = fileName
            .Replace(":", "_")
            .Replace("/", "_")
            .Replace("\\", "_")
            .Replace("|", "_")
            .Replace("?", "_")
            .Replace("*", "_")
            .Replace("\"", "_")
            .Replace("<", "_")
            .Replace(">", "_");

        fileName = fileName.TrimEnd('.', ' ');

        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = "video";
        }

        const int maxLength = 240;
        if (fileName.Length > maxLength)
        {
            fileName = fileName.Substring(0, maxLength);
        }

        return fileName;
    }
}