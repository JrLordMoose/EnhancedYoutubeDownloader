using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.Core.Models;

/// <summary>
/// Generic video implementation for non-YouTube platforms (TikTok, Instagram, Twitter, etc.)
/// This is a stub that holds the URL and lets yt-dlp handle the actual download
/// </summary>
public class GenericVideo : IVideo
{
    public GenericVideo(string url, string title, string? videoId = null)
    {
        Url = url;
        Title = title;
        Id = TryCreateVideoId(videoId) ?? new VideoId(
            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url))
                .Replace("/", "_")
                .Replace("+", "-")
                .PadRight(11, 'A')[..11]
        );
        Author = new Author(new ChannelId("UCGenericVideo000"), title);
        Duration = null;
        Thumbnails = Array.Empty<Thumbnail>();
        Keywords = Array.Empty<string>();
        Engagement = new Engagement(0, 0, 0);
    }

    private static VideoId? TryCreateVideoId(string? videoId)
    {
        if (string.IsNullOrWhiteSpace(videoId) || videoId.Length != 11)
            return null;

        try
        {
            return new VideoId(videoId);
        }
        catch
        {
            return null;
        }
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
