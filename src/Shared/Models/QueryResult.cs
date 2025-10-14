using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.Shared.Models;

/// <summary>
/// Represents the result of resolving a video download query
/// </summary>
public class QueryResult
{
    /// <summary>
    /// The kind of query result (video, playlist, channel, or search)
    /// </summary>
    public required QueryResultKind Kind { get; init; }

    /// <summary>
    /// The platform/service from which content is being downloaded
    /// </summary>
    public PlatformType Platform { get; init; } = PlatformType.YouTube;

    /// <summary>
    /// Single video (when Kind is Video)
    /// </summary>
    public IVideo? Video { get; init; }

    /// <summary>
    /// Collection of videos (when Kind is Playlist, Channel, or Search)
    /// </summary>
    public IReadOnlyList<IVideo>? Videos { get; init; }

    /// <summary>
    /// Title of the playlist or channel
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Description of the playlist or channel
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Author/channel information
    /// </summary>
    public string? Author { get; init; }
}
