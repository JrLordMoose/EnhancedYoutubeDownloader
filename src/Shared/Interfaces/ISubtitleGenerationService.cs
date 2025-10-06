namespace EnhancedYoutubeDownloader.Shared.Interfaces;

/// <summary>
/// Service for generating subtitles using AI speech recognition (Whisper.net)
/// </summary>
public interface ISubtitleGenerationService
{
    /// <summary>
    /// Generates subtitle file (.srt) from a video file using AI transcription
    /// </summary>
    /// <param name="videoFilePath">Path to the video file to transcribe</param>
    /// <param name="outputSubtitlePath">Path where the .srt file should be saved</param>
    /// <param name="language">Language code (e.g., "en", "es", "auto" for auto-detect)</param>
    /// <param name="progress">Progress reporter (0.0 to 1.0)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> GenerateSubtitlesAsync(
        string videoFilePath,
        string outputSubtitlePath,
        string language = "auto",
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the Whisper model is downloaded and ready
    /// </summary>
    /// <returns>True if model is available, false otherwise</returns>
    Task<bool> IsModelAvailableAsync();

    /// <summary>
    /// Downloads the Whisper model if not already available
    /// </summary>
    /// <param name="progress">Progress reporter (0.0 to 1.0)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DownloadModelAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default);
}
