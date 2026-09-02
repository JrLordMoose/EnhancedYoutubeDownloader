using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnhancedYoutubeDownloader.Framework;
using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

public partial class DownloadMultipleSetupViewModel : DialogViewModelBase
{
    private readonly DialogManager _dialogManager;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _downloadDirectory = string.Empty;

    [ObservableProperty]
    private string _selectedQuality = "Best Quality";

    [ObservableProperty]
    private string _selectedFormat = "MP4";

    [ObservableProperty]
    private bool _downloadSubtitles = true;

    [ObservableProperty]
    private bool _injectTags = true;

    public ObservableCollection<PlaylistVideoSelection> VideoItems { get; } = new();

    public IReadOnlyList<string> QualityOptions { get; } =
        new[]
        {
            "Best Quality",
            "1080p (Full HD)",
            "720p (HD)",
            "480p (SD)",
            "360p",
            "Audio Only (Best)",
        };

    public IReadOnlyList<string> FormatOptions { get; } = new[] { "MP4", "WebM", "MP3" };

    public IReadOnlyList<IVideo> SelectedVideos =>
        VideoItems.Where(item => item.IsSelected).Select(item => item.Video).ToList();

    public string SelectionSummary =>
        $"{VideoItems.Count(item => item.IsSelected)} of {VideoItems.Count} videos selected";

    public bool CanDownload =>
        VideoItems.Any(item => item.IsSelected) && !string.IsNullOrWhiteSpace(DownloadDirectory);

    public DownloadMultipleSetupViewModel(DialogManager dialogManager)
    {
        _dialogManager = dialogManager;
    }

    public void SetVideos(IEnumerable<IVideo> videos)
    {
        VideoItems.Clear();
        foreach (var video in videos)
        {
            VideoItems.Add(new PlaylistVideoSelection(video, NotifySelectionChanged));
        }

        NotifySelectionChanged();
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var item in VideoItems)
        {
            item.IsSelected = true;
        }

        NotifySelectionChanged();
    }

    [RelayCommand]
    private void SelectNone()
    {
        foreach (var item in VideoItems)
        {
            item.IsSelected = false;
        }

        NotifySelectionChanged();
    }

    [RelayCommand]
    private async Task BrowseAsync()
    {
        var selectedPath = await _dialogManager.PromptFolderPathAsync(
            "Select playlist download folder",
            DownloadDirectory
        );

        if (!string.IsNullOrWhiteSpace(selectedPath))
        {
            DownloadDirectory = selectedPath;
        }
    }

    [RelayCommand]
    private void Confirm()
    {
        if (CanDownload)
        {
            Close(true);
        }
    }

    [RelayCommand]
    private void Cancel() => Close(false);

    partial void OnDownloadDirectoryChanged(string value) => OnPropertyChanged(nameof(CanDownload));

    private void NotifySelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedVideos));
        OnPropertyChanged(nameof(SelectionSummary));
        OnPropertyChanged(nameof(CanDownload));
    }
}

public partial class PlaylistVideoSelection : ObservableObject
{
    private readonly Action _selectionChanged;

    public IVideo Video { get; }

    [ObservableProperty]
    private bool _isSelected = true;

    public string DurationText => Video.Duration?.ToString(@"h\:mm\:ss") ?? "--:--";

    public PlaylistVideoSelection(IVideo video, Action selectionChanged)
    {
        Video = video;
        _selectionChanged = selectionChanged;
    }

    partial void OnIsSelectedChanged(bool value) => _selectionChanged();
}
