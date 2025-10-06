using EnhancedYoutubeDownloader.Shared.Models;
using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.Shared.Interfaces;

/// <summary>
/// Defines the contract for a service that manages video downloads.
/// </summary>
public interface IDownloadService
{
    /// <summary>
    /// Creates a new download item.
    /// </summary>
    /// <param name="video">The video to download.</param>
    /// <param name="filePath">The path to save the downloaded file.</param>
    /// <param name="profile">The format profile to use for the download.</param>
    /// <returns>The created download item.</returns>
    Task<DownloadItem> CreateDownloadAsync(IVideo video, string filePath, FormatProfile? profile = null);

    /// <summary>
    /// Starts a download.
    /// </summary>
    /// <param name="downloadItem">The download item to start.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StartDownloadAsync(DownloadItem downloadItem);

    /// <summary>
    /// Pauses a download.
    /// </summary>
    /// <param name="downloadItem">The download item to pause.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PauseDownloadAsync(DownloadItem downloadItem);

    /// <summary>
    /// Resumes a paused download.
    /// </summary>
    /// <param name="downloadItem">The download item to resume.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ResumeDownloadAsync(DownloadItem downloadItem);

    /// <summary>
    /// Cancels a download.
    /// </summary>
    /// <param name="downloadItem">The download item to cancel.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CancelDownloadAsync(DownloadItem downloadItem);

    /// <summary>
    /// Restarts a download.
    /// </summary>
    /// <param name="downloadItem">The download item to restart.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RestartDownloadAsync(DownloadItem downloadItem);

    /// <summary>
    /// Deletes a download.
    /// </summary>
    /// <param name="downloadItem">The download item to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteDownloadAsync(DownloadItem downloadItem);

    /// <summary>
    /// Gets an observable that notifies when the status of a download changes.
    /// </summary>
    IObservable<DownloadItem> DownloadStatusChanged { get; }

    /// <summary>
    /// Gets an observable that notifies the overall download progress.
    /// </summary>
    IObservable<double> OverallProgress { get; }
}