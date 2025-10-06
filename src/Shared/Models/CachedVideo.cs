namespace EnhancedYoutubeDownloader.Shared.Models;

/// <summary>
/// Represents a serializable version of video metadata for caching purposes.
/// </summary>
public class CachedVideo
{
    /// <summary>
    /// Gets or sets the video ID.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets or sets the video title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets or sets the video author.
    /// </summary>
    public required string Author { get; init; }

    /// <summary>
    /// Gets or sets the author's channel ID.
    /// </summary>
    public required string AuthorChannelId { get; init; }

    /// <summary>
    /// Gets or sets the video duration.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Gets or sets the video description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the list of keywords for the video.
    /// </summary>
    public List<string> Keywords { get; init; } = new();

    /// <summary>
    /// Gets or sets the list of thumbnails for the video.
    /// </summary>
    public List<CachedThumbnail> Thumbnails { get; init; } = new();

    /// <summary>
    /// Gets or sets the upload date of the video.
    /// </summary>
    public DateTimeOffset UploadDate { get; init; }
}

/// <summary>
/// Represents a cached thumbnail.
/// </summary>
public class CachedThumbnail
{
    /// <summary>
    /// Gets or sets the thumbnail URL.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Gets or sets the thumbnail width.
    /// </summary>
    public required int Width { get; init; }

    /// <summary>
    /// Gets or sets the thumbnail height.
    /// </summary>
    public required int Height { get; init; }
}