using EnhancedYoutubeDownloader.Shared.Models;
using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.Shared.Interfaces;

public interface IDownloadService
{
    Task<DownloadItem> CreateDownloadAsync(IVideo video, string filePath, FormatProfile? profile = null, PlatformType platform = PlatformType.YouTube);
    Task StartDownloadAsync(DownloadItem downloadItem);
    Task PauseDownloadAsync(DownloadItem downloadItem);
    Task ResumeDownloadAsync(DownloadItem downloadItem);
    Task CancelDownloadAsync(DownloadItem downloadItem);
    Task RestartDownloadAsync(DownloadItem downloadItem);
    Task DeleteDownloadAsync(DownloadItem downloadItem);

    /// <summary>
    /// Looks up a video's real title/author/thumbnail without downloading it.
    /// Returns null when the lookup is unsupported or fails.
    /// </summary>
    Task<IVideo?> TryFetchVideoInfoAsync(string url, CancellationToken cancellationToken = default) =>
        Task.FromResult<IVideo?>(null);

    IObservable<DownloadItem> DownloadStatusChanged { get; }
    IObservable<double> OverallProgress { get; }
}