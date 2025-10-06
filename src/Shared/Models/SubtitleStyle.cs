namespace EnhancedYoutubeDownloader.Shared.Models;

/// <summary>
/// Subtitle embedding and styling options
/// </summary>
public enum SubtitleStyle
{
    /// <summary>
    /// Embed basic subtitles as mov_text (can be toggled in player)
    /// </summary>
    Embedded,

    /// <summary>
    /// Burn professional-styled subtitles into video (permanent, always visible)
    /// </summary>
    BurnedIn,

    /// <summary>
    /// Create separate styled .ass subtitle file (requires player support)
    /// </summary>
    External
}
