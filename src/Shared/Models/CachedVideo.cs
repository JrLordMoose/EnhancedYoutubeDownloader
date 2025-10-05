namespace EnhancedYoutubeDownloader.Shared.Models;

/// <summary>
/// Serializable representation of video metadata for caching
/// </summary>
public class CachedVideo
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Author { get; init; }
    public required string AuthorChannelId { get; init; }
    public TimeSpan Duration { get; init; }
    public string? Description { get; init; }
    public List<string> Keywords { get; init; } = new();
    public List<CachedThumbnail> Thumbnails { get; init; } = new();
    public DateTimeOffset UploadDate { get; init; }
}

public class CachedThumbnail
{
    public required string Url { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
}
