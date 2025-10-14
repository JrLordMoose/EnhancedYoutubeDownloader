using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.Core.Models;

/// <summary>
/// Generic video implementation for non-YouTube platforms (TikTok, Instagram, Twitter, etc.)
/// This is a stub that holds the URL and lets yt-dlp handle the actual download
/// </summary>
public class GenericVideo : IVideo
{
    public GenericVideo(string url, string title)
    {
        Url = url;
        Title = title;
        Id = new VideoId(url); // Use URL as ID
        Author = new Author(string.Empty, title);
        Duration = null;
        Thumbnails = Array.Empty<Thumbnail>();
        Keywords = Array.Empty<string>();
        Engagement = new Engagement(0, 0, 0);
    }

    public VideoId Id { get; }
    public string Url { get; }
    public string Title { get; }
    public Author Author { get; }
    public DateTimeOffset? UploadDate => null;
    public string Description => string.Empty;
    public TimeSpan? Duration { get; }
    public IReadOnlyList<Thumbnail> Thumbnails { get; }
    public IReadOnlyList<string> Keywords { get; }
    public Engagement Engagement { get; }
}
