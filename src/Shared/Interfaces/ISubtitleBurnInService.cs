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
    /// <param name="progress">Progress reporter (0.0 to 1.0)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> BurnSubtitlesAsync(
        string videoPath,
        string subtitlePath,
        string outputPath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);
}
