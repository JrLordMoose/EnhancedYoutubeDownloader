using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnhancedYoutubeDownloader.Framework;
using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

public partial class DownloadSingleSetupViewModel : DialogViewModelBase
{
    private readonly DialogManager _dialogManager;

    [ObservableProperty]
    private IVideo? _video;

    [ObservableProperty]
    private List<object> _downloadOptions = new();

    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private bool _downloadSubtitles = true;

    [ObservableProperty]
    private bool _injectTags = true;

    [ObservableProperty]
    private string _selectedQuality = "Best Quality";

    [ObservableProperty]
    private string _selectedFormat = "MP4";

    // Quality options for ComboBox
    public List<string> QualityOptions { get; } =
        new()
        {
            "Best Quality",
            "1080p (Full HD)",
            "720p (HD)",
            "480p (SD)",
            "360p",
            "Audio Only (Best)",
        };

    // Format options for ComboBox
    public List<string> FormatOptions { get; } = new() { "MP4", "WebM", "MP3" };

    public DownloadSingleSetupViewModel(DialogManager dialogManager)
    {
        _dialogManager = dialogManager;
    }

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

    /// <summary>
    /// Generates a unique file path by appending (1), (2), etc. if the file already exists
    /// </summary>
    public static string GetUniqueFilePath(string filePath)
    {
        if (!File.Exists(filePath))
            return filePath;

        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);

        var counter = 1;
        string newFilePath;

        do
        {
            var newFileName = $"{fileNameWithoutExtension} ({counter}){extension}";
            newFilePath = Path.Combine(directory, newFileName);
            counter++;
        } while (File.Exists(newFilePath));

        return newFilePath;
    }
}
