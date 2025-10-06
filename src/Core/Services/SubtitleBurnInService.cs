using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using EnhancedYoutubeDownloader.Shared.Interfaces;

namespace EnhancedYoutubeDownloader.Core.Services;

/// <summary>
/// Service for burning professional-styled subtitles into videos using FFmpeg
/// </summary>
public class SubtitleBurnInService : ISubtitleBurnInService
{
    private readonly string _ffmpegPath;
    private readonly string _ffprobePath;

    public SubtitleBurnInService()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        _ffmpegPath = Path.Combine(baseDir, isWindows ? "ffmpeg.exe" : "ffmpeg");
        _ffprobePath = Path.Combine(baseDir, isWindows ? "ffprobe.exe" : "ffprobe");
    }

    public async Task<bool> BurnSubtitlesAsync(
        string videoPath,
        string subtitlePath,
        string outputPath,
        int fontSize = 24,
        double backgroundOpacity = 0.75,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(videoPath))
                throw new FileNotFoundException($"Video file not found: {videoPath}");

            if (!File.Exists(subtitlePath))
                throw new FileNotFoundException($"Subtitle file not found: {subtitlePath}");

            if (!File.Exists(_ffmpegPath))
                throw new FileNotFoundException($"FFmpeg not found: {_ffmpegPath}");

            // Validate parameters
            if (fontSize <= 0 || fontSize > 200)
                throw new ArgumentOutOfRangeException(nameof(fontSize), "Font size must be between 1 and 200");

            if (backgroundOpacity < 0.0 || backgroundOpacity > 1.0)
                throw new ArgumentOutOfRangeException(nameof(backgroundOpacity), "Opacity must be between 0.0 and 1.0");

            // Escape paths for FFmpeg (Windows requires quadruple backslashes, double for colon)
            var escapedSubtitlePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? subtitlePath.Replace("\\", "\\\\\\\\").Replace(":", "\\\\:")
                : subtitlePath.Replace(":", "\\:");

            // Build FFmpeg subtitle filter with professional styling
            // This creates subtitles with black semi-transparent box and white text
            var subtitleFilter = $"subtitles='{escapedSubtitlePath}':force_style='" +
                $"FontName=Arial," +
                $"FontSize={fontSize}," +
                $"PrimaryColour=&H00FFFFFF," + // White text
                $"OutlineColour=&H00000000," + // Black outline
                $"BackColour=&H{GetAlphaHex(backgroundOpacity)}000000," + // Black background with opacity
                $"BorderStyle=4," + // Box background
                $"Outline=1," + // Text outline thickness
                $"Shadow=0," + // No shadow
                $"MarginV=20," + // Bottom margin
                $"Alignment=2'"; // Bottom center

            // Build FFmpeg command
            var arguments = $"-i \"{videoPath}\" -vf \"{subtitleFilter}\" -c:a copy -y \"{outputPath}\"";

            Console.WriteLine($"[BURN-SUBS] Starting FFmpeg process...");
            Console.WriteLine($"[BURN-SUBS] Command: {_ffmpegPath} {arguments}");

            var processInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            Process? process = null;

            try
            {
                process = new Process { StartInfo = processInfo };

                // Get total duration for progress reporting
                var duration = await GetVideoDurationAsync(videoPath, cancellationToken);

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data))
                        return;

                    // Parse FFmpeg progress output
                    // Example: "frame= 1234 fps=30 size=  12345kB time=00:00:41.23 bitrate=..."
                    var timeMatch = Regex.Match(e.Data, @"time=(\d{2}):(\d{2}):(\d{2}\.\d{2})");
                    if (timeMatch.Success && duration > 0)
                    {
                        try
                        {
                            var hours = int.Parse(timeMatch.Groups[1].Value);
                            var minutes = int.Parse(timeMatch.Groups[2].Value);
                            var seconds = double.Parse(timeMatch.Groups[3].Value);
                            var currentTime = hours * 3600 + minutes * 60 + seconds;

                            var progressValue = Math.Min(currentTime / duration, 1.0);
                            progress?.Report(progressValue);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[BURN-SUBS] Failed to parse progress: {ex.Message}");
                        }
                    }
                };

                // Consume stdout to prevent blocking
                process.OutputDataReceived += (sender, e) => { };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(cancellationToken);

                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"[BURN-SUBS] FFmpeg exited with code: {process.ExitCode}");
                    return false;
                }

                progress?.Report(1.0);
                Console.WriteLine($"[BURN-SUBS] Subtitles burned successfully: {outputPath}");
                return true;
            }
            catch (OperationCanceledException)
            {
                try
                {
                    if (process != null && !process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                        Console.WriteLine($"[BURN-SUBS] FFmpeg process killed due to cancellation");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BURN-SUBS] Failed to kill process: {ex.Message}");
                }
                throw;
            }
            finally
            {
                process?.Dispose();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BURN-SUBS] Error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the duration of a video file in seconds
    /// </summary>
    private async Task<double> GetVideoDurationAsync(string videoPath, CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(_ffprobePath))
            {
                Console.WriteLine($"[BURN-SUBS] ffprobe not found at {_ffprobePath}, progress reporting disabled");
                return 0;
            }

            var arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{videoPath}\"";

            var processInfo = new ProcessStartInfo
            {
                FileName = _ffprobePath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (double.TryParse(output.Trim(), out var duration))
                return duration;

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BURN-SUBS] Failed to get video duration: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Converts opacity (0.0-1.0) to ASS alpha hex value (00-FF inverted)
    /// </summary>
    private static string GetAlphaHex(double opacity)
    {
        // ASS format uses inverted alpha: 00 = fully opaque, FF = fully transparent
        var alpha = (int)((1.0 - opacity) * 255);
        return alpha.ToString("X2");
    }
}
