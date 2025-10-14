namespace EnhancedYoutubeDownloader.Shared.Models;

/// <summary>
/// Represents the type of platform/service from which content is being downloaded.
/// </summary>
public enum PlatformType
{
    /// <summary>
    /// Unknown or unsupported platform
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// YouTube (youtube.com, youtu.be) - videos, shorts, playlists, channels
    /// </summary>
    YouTube = 1,

    /// <summary>
    /// TikTok (tiktok.com) - short-form videos
    /// </summary>
    TikTok = 2,

    /// <summary>
    /// Instagram (instagram.com) - posts, reels, stories, IGTV
    /// </summary>
    Instagram = 3,

    /// <summary>
    /// Twitter/X (twitter.com, x.com) - videos and GIFs
    /// </summary>
    Twitter = 4,

    /// <summary>
    /// Generic video URL - any of the 1,800+ sites supported by yt-dlp
    /// </summary>
    Generic = 5
}
