using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.Shared.Models;

/// <summary>
/// Represents the result of resolving a YouTube query.
/// </summary>
public class QueryResult
{
    /// <summary>
    /// Gets or sets the kind of query result (e.g., video, playlist).
    /// </summary>
    public required QueryResultKind Kind { get; init; }

    /// <summary>
    /// Gets or sets the single video if the result kind is <see cref="QueryResultKind.Video"/>.
    /// </summary>
    public IVideo? Video { get; init; }

    /// <summary>
    /// Gets or sets the collection of videos if the result kind is <see cref="QueryResultKind.Playlist"/>,
    /// <see cref="QueryResultKind.Channel"/>, or <see cref="QueryResultKind.Search"/>.
    /// </summary>
    public IReadOnlyList<IVideo>? Videos { get; init; }

    /// <summary>
    /// Gets or sets the title of the playlist or channel.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets or sets the description of the playlist or channel.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the author or channel information.
    /// </summary>
    public string? Author { get; init; }
}