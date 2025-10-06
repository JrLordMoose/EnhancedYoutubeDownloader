namespace EnhancedYoutubeDownloader.Shared.Models;

/// <summary>
/// Represents the type of YouTube query result.
/// </summary>
public enum QueryResultKind
{
    /// <summary>
    /// The result is a single video.
    /// </summary>
    Video,

    /// <summary>
    /// The result is a playlist containing multiple videos.
    /// </summary>
    Playlist,

    /// <summary>
    /// The result is a channel containing multiple videos.
    /// </summary>
    Channel,

    /// <summary>
    /// The result is a list of videos from a search query.
    /// </summary>
    Search
}