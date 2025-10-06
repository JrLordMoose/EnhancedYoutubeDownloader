using EnhancedYoutubeDownloader.Shared.Models;
using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.Shared.Interfaces;

public interface IDownloadService
{
    Task<DownloadItem> CreateDownloadAsync(IVideo video, string filePath, FormatProfile? profile = null);
    Task StartDownloadAsync(DownloadItem downloadItem);
    Task PauseDownloadAsync(DownloadItem downloadItem);
    Task ResumeDownloadAsync(DownloadItem downloadItem);
    Task CancelDownloadAsync(DownloadItem downloadItem);
    Task RestartDownloadAsync(DownloadItem downloadItem);
    Task DeleteDownloadAsync(DownloadItem downloadItem);
    
    IObservable<DownloadItem> DownloadStatusChanged { get; }
    IObservable<double> OverallProgress { get; }
}