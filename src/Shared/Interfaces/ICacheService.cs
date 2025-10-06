using EnhancedYoutubeDownloader.Shared.Models;

namespace EnhancedYoutubeDownloader.Shared.Interfaces;

public interface ICacheService
{
    Task<CachedVideo?> GetVideoMetadataAsync(string videoId);
    Task SetVideoMetadataAsync(string videoId, CachedVideo video, TimeSpan? expiration = null);
    Task<bool> IsVideoMetadataCachedAsync(string videoId);
    Task ClearCacheAsync();
    Task ClearExpiredCacheAsync();
}