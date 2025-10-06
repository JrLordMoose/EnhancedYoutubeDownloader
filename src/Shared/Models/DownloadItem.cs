using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.Shared.Models;

public partial class DownloadItem : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(Author))]
    [NotifyPropertyChangedFor(nameof(Duration))]
    [NotifyPropertyChangedFor(nameof(ThumbnailUrl))]
    private IVideo? _video;

    [ObservableProperty]
    private string? _filePath;

    [ObservableProperty]
    private FormatProfile? _formatProfile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanStart))]
    [NotifyPropertyChangedFor(nameof(CanPause))]
    [NotifyPropertyChangedFor(nameof(CanResume))]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    [NotifyPropertyChangedFor(nameof(CanRestart))]
    [NotifyPropertyChangedFor(nameof(CanOpen))]
    [NotifyPropertyChangedFor(nameof(IsCompleted))]
    [NotifyPropertyChangedFor(nameof(FormattedProgressInfo))]
    private DownloadStatus _status = DownloadStatus.Queued;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BytesProgress))]
    [NotifyPropertyChangedFor(nameof(FormattedProgressInfo))]
    private double _progress;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private DateTime _createdAt = DateTime.UtcNow;

    [ObservableProperty]
    private DateTime? _startedAt;

    [ObservableProperty]
    private DateTime? _completedAt;

    [ObservableProperty]
    private bool _canStart;

    [ObservableProperty]
    private bool _canPause;

    [ObservableProperty]
    private bool _canResume;

    [ObservableProperty]
    private bool _canCancel;

    [ObservableProperty]
    private bool _canRestart;

    [ObservableProperty]
    private bool _canOpen;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BytesProgress))]
    [NotifyPropertyChangedFor(nameof(FormattedBytes))]
    [NotifyPropertyChangedFor(nameof(FormattedProgressInfo))]
    private long _bytesDownloaded;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BytesProgress))]
    [NotifyPropertyChangedFor(nameof(FormattedBytes))]
    [NotifyPropertyChangedFor(nameof(FormattedProgressInfo))]
    private long _totalBytes;

    [ObservableProperty]
    private string? _partialFilePath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedSpeed))]
    [NotifyPropertyChangedFor(nameof(FormattedProgressInfo))]
    private double _downloadSpeed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedEta))]
    [NotifyPropertyChangedFor(nameof(FormattedProgressInfo))]
    private TimeSpan? _estimatedTimeRemaining;

    // Tracking fields for speed calculation (public for service access)
    public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;
    public long LastBytesDownloaded { get; set; }

    public string Title => Video?.Title ?? "Unknown";
    public string Author => Video?.Author?.ChannelTitle ?? "Unknown";
    public TimeSpan? Duration => Video?.Duration;
    public string? ThumbnailUrl => Video?.Thumbnails?.OrderByDescending(t => t.Resolution.Area).FirstOrDefault()?.Url;
    public double BytesProgress => TotalBytes > 0 ? (BytesDownloaded * 100.0 / TotalBytes) : 0;
    public bool IsCompleted => Status == DownloadStatus.Completed;

    // Formatted properties for display
    public string FormattedSpeed => DownloadSpeed > 0 ? $"{FormatBytes((long)DownloadSpeed)}/s" : "--";
    public string FormattedEta => EstimatedTimeRemaining.HasValue ? FormatDuration(EstimatedTimeRemaining.Value) : "--";
    public string FormattedBytes => TotalBytes > 0 ? $"{FormatBytes(BytesDownloaded)} / {FormatBytes(TotalBytes)}" : "--";
    public string FormattedProgressInfo =>
        Status == DownloadStatus.Failed
            ? "Failed"
            : Status == DownloadStatus.Paused
                ? "Paused"
                : Status == DownloadStatus.Started && Progress <= 0 && TotalBytes == 0
                    ? "Loading..."
                    : $"{Progress:F1}% • {FormattedSpeed} • {FormattedEta} • {FormattedBytes}";

    // Helper methods
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

    public static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours}h {duration.Minutes}m";
        else if (duration.TotalMinutes >= 1)
            return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";
        else
            return $"{duration.Seconds}s";
    }

    public void UpdateTracking(long currentBytes, DateTime currentTime)
    {
        LastBytesDownloaded = currentBytes;
        LastUpdateTime = currentTime;
    }

    public void ResetTracking()
    {
        DownloadSpeed = 0;
        EstimatedTimeRemaining = null;
        LastUpdateTime = DateTime.UtcNow;
        LastBytesDownloaded = 0;
    }
}