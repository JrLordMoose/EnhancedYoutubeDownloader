using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.Shared.Models;

/// <summary>
/// Represents a single download item, including its state, progress, and associated video information.
/// </summary>
public partial class DownloadItem : ObservableObject
{
    /// <summary>
    /// Gets or sets the unique identifier for the download.
    /// </summary>
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the video associated with this download.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(Author))]
    [NotifyPropertyChangedFor(nameof(Duration))]
    [NotifyPropertyChangedFor(nameof(ThumbnailUrl))]
    private IVideo? _video;

    /// <summary>
    /// Gets or sets the final file path for the downloaded video.
    /// </summary>
    [ObservableProperty]
    private string? _filePath;

    /// <summary>
    /// Gets or sets the format profile for the download.
    /// </summary>
    [ObservableProperty]
    private FormatProfile? _formatProfile;

    /// <summary>
    /// Gets or sets the current status of the download.
    /// </summary>
    [ObservableProperty]
    private DownloadStatus _status = DownloadStatus.Queued;

    /// <summary>
    /// Gets or sets the download progress percentage (0-100).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BytesProgress))]
    [NotifyPropertyChangedFor(nameof(FormattedProgressInfo))]
    private double _progress;

    /// <summary>
    /// Gets or sets an error message if the download failed.
    /// </summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// Gets or sets the timestamp when the download was created.
    /// </summary>
    [ObservableProperty]
    private DateTime _createdAt = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when the download started.
    /// </summary>
    [ObservableProperty]
    private DateTime? _startedAt;

    /// <summary>
    /// Gets or sets the timestamp when the download completed.
    /// </summary>
    [ObservableProperty]
    private DateTime? _completedAt;

    /// <summary>
    /// Gets or sets a value indicating whether the download can be paused.
    /// </summary>
    [ObservableProperty]
    private bool _canPause;

    /// <summary>
    /// Gets or sets a value indicating whether the download can be resumed.
    /// </summary>
    [ObservableProperty]
    private bool _canResume;

    /// <summary>
    /// Gets or sets a value indicating whether the download can be canceled.
    /// </summary>
    [ObservableProperty]
    private bool _canCancel;

    /// <summary>
    /// Gets or sets a value indicating whether the download can be restarted.
    /// </summary>
    [ObservableProperty]
    private bool _canRestart;

    /// <summary>
    /// Gets or sets the number of bytes downloaded.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BytesProgress))]
    [NotifyPropertyChangedFor(nameof(FormattedBytes))]
    [NotifyPropertyChangedFor(nameof(FormattedProgressInfo))]
    private long _bytesDownloaded;

    /// <summary>
    /// Gets or sets the total number of bytes for the download.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BytesProgress))]
    [NotifyPropertyChangedFor(nameof(FormattedBytes))]
    [NotifyPropertyChangedFor(nameof(FormattedProgressInfo))]
    private long _totalBytes;

    /// <summary>
    /// Gets or sets the path to the partial download file.
    /// </summary>
    [ObservableProperty]
    private string? _partialFilePath;

    /// <summary>
    /// Gets or sets the current download speed in bytes per second.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedSpeed))]
    [NotifyPropertyChangedFor(nameof(FormattedProgressInfo))]
    private double _downloadSpeed;

    /// <summary>
    /// Gets or sets the estimated time remaining for the download.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedEta))]
    [NotifyPropertyChangedFor(nameof(FormattedProgressInfo))]
    private TimeSpan? _estimatedTimeRemaining;

    /// <summary>
    /// Gets or sets the timestamp of the last progress update.
    /// </summary>
    public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the number of bytes downloaded at the last progress update.
    /// </summary>
    public long LastBytesDownloaded { get; set; }

    /// <summary>
    /// Gets the title of the video.
    /// </summary>
    public string Title => Video?.Title ?? "Unknown";

    /// <summary>
    /// Gets the author of the video.
    /// </summary>
    public string Author => Video?.Author?.ChannelTitle ?? "Unknown";

    /// <summary>
    /// Gets the duration of the video.
    /// </summary>
    public TimeSpan? Duration => Video?.Duration;

    /// <summary>
    /// Gets the URL of the best available thumbnail for the video.
    /// </summary>
    public string? ThumbnailUrl => Video?.Thumbnails?.OrderByDescending(t => t.Resolution.Area).FirstOrDefault()?.Url;

    /// <summary>
    /// Gets the download progress based on bytes downloaded.
    /// </summary>
    public double BytesProgress => TotalBytes > 0 ? (BytesDownloaded * 100.0 / TotalBytes) : 0;

    /// <summary>
    /// Gets the formatted download speed.
    /// </summary>
    public string FormattedSpeed => DownloadSpeed > 0 ? $"{FormatBytes((long)DownloadSpeed)}/s" : "--";

    /// <summary>
    /// Gets the formatted estimated time remaining.
    /// </summary>
    public string FormattedEta => EstimatedTimeRemaining.HasValue ? FormatDuration(EstimatedTimeRemaining.Value) : "--";

    /// <summary>
    /// Gets the formatted progress in bytes (e.g., "10.5 MB / 20.0 MB").
    /// </summary>
    public string FormattedBytes => TotalBytes > 0 ? $"{FormatBytes(BytesDownloaded)} / {FormatBytes(TotalBytes)}" : "--";

    /// <summary>
    /// Gets a formatted string containing all progress information.
    /// </summary>
    public string FormattedProgressInfo => $"{Progress:F1}% • {FormattedSpeed} • {FormattedEta} • {FormattedBytes}";

    /// <summary>
    /// Formats a byte count into a human-readable string (e.g., KB, MB, GB).
    /// </summary>
    /// <param name="bytes">The number of bytes.</param>
    /// <returns>A formatted string.</returns>
    public static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// Formats a TimeSpan into a human-readable string.
    /// </summary>
    /// <param name="duration">The duration to format.</param>
    /// <returns>A formatted string.</returns>
    public static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours}h {duration.Minutes}m";
        else if (duration.TotalMinutes >= 1)
            return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";
        else
            return $"{duration.Seconds}s";
    }

    /// <summary>
    /// Updates the tracking information for calculating download speed.
    /// </summary>
    /// <param name="currentBytes">The current number of bytes downloaded.</param>
    /// <param name="currentTime">The current time.</param>
    public void UpdateTracking(long currentBytes, DateTime currentTime)
    {
        LastBytesDownloaded = currentBytes;
        LastUpdateTime = currentTime;
    }

    /// <summary>
    /// Resets the download speed and ETA tracking information.
    /// </summary>
    public void ResetTracking()
    {
        DownloadSpeed = 0;
        EstimatedTimeRemaining = null;
        LastUpdateTime = DateTime.UtcNow;
        LastBytesDownloaded = 0;
    }
}