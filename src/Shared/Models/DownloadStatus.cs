namespace EnhancedYoutubeDownloader.Shared.Models;

/// <summary>
/// Represents the status of a download.
/// </summary>
public enum DownloadStatus
{
    /// <summary>
    /// The download is queued and waiting to start.
    /// </summary>
    Queued,

    /// <summary>
    /// The download has started.
    /// </summary>
    Started,

    /// <summary>
    /// The download is paused.
    /// </summary>
    Paused,

    /// <summary>
    /// The download completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// The download failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The download was canceled by the user.
    /// </summary>
    Canceled
}