using EnhancedYoutubeDownloader.Shared.Models;

namespace EnhancedYoutubeDownloader.Shared.Interfaces;

/// <summary>
/// Defines the contract for a cache service that stores and retrieves video metadata.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets cached video metadata by video ID.
    /// </summary>
    /// <param name="videoId">The ID of the video.</param>
    /// <returns>The cached video metadata, or null if not found or expired.</returns>
    Task<CachedVideo?> GetVideoMetadataAsync(string videoId);

    /// <summary>
    /// Sets cached video metadata.
    /// </summary>
    /// <param name="videoId">The ID of the video.</param>
    /// <param name="video">The video metadata to cache.</param>
    /// <param name="expiration">The cache expiration timespan. If null, the cache does not expire.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetVideoMetadataAsync(string videoId, CachedVideo video, TimeSpan? expiration = null);

    /// <summary>
    /// Checks if video metadata is cached.
    /// </summary>
    /// <param name="videoId">The ID of the video.</param>
    /// <returns>True if the metadata is cached and not expired, false otherwise.</returns>
    Task<bool> IsVideoMetadataCachedAsync(string videoId);

    /// <summary>
    /// Clears the entire cache.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ClearCacheAsync();

    /// <summary>
    /// Clears expired cache entries.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ClearExpiredCacheAsync();
}