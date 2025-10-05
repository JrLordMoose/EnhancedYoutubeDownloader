namespace EnhancedYoutubeDownloader.Shared.Models;

/// <summary>
/// Represents the type of YouTube query result
/// </summary>
public enum QueryResultKind
{
    /// <summary>
    /// Single video
    /// </summary>
    Video,

    /// <summary>
    /// Playlist containing multiple videos
    /// </summary>
    Playlist,

    /// <summary>
    /// Channel with multiple videos
    /// </summary>
    Channel,

    /// <summary>
    /// Search query results
    /// </summary>
    Search
}
