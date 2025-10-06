namespace EnhancedYoutubeDownloader.Shared.Interfaces;

/// <summary>
/// Service for burning styled subtitles into video using FFmpeg
/// </summary>
public interface ISubtitleBurnInService
{
    /// <summary>
    /// Burns subtitles into video with professional styling
    /// </summary>
    /// <param name="videoPath">Path to input video file</param>
    /// <param name="subtitlePath">Path to subtitle file (.srt)</param>
    /// <param name="outputPath">Path for output video with burned subtitles</param>
    /// <param name="fontSize">Font size for subtitles (default: 24)</param>
    /// <param name="backgroundOpacity">Background box opacity 0.0-1.0 (default: 0.75)</param>
    /// <param name="progress">Progress reporter (0.0 to 1.0)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> BurnSubtitlesAsync(
        string videoPath,
        string subtitlePath,
        string outputPath,
        int fontSize = 24,
        double backgroundOpacity = 0.75,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);
}
