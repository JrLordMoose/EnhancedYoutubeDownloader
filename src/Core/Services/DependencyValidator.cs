using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace EnhancedYoutubeDownloader.Core.Services;

public interface IDependencyValidator
{
    Task<DependencyValidationResult> ValidateAsync();
}

public class DependencyValidationResult
{
    public bool IsValid => MissingDependencies.Count == 0;
    public List<MissingDependency> MissingDependencies { get; set; } = new();
}

public class MissingDependency
{
    public required string Name { get; set; }
    public required string ExpectedPath { get; set; }
    public required string DownloadUrl { get; set; }
    public required bool IsRequired { get; set; }
}

public class DependencyValidator : IDependencyValidator
{
    public async Task<DependencyValidationResult> ValidateAsync()
    {
        var result = new DependencyValidationResult();
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;

        // Check for yt-dlp.exe
        var ytdlpPath = Path.Combine(baseDir, "yt-dlp.exe");
        if (!File.Exists(ytdlpPath))
        {
            result.MissingDependencies.Add(new MissingDependency
            {
                Name = "yt-dlp",
                ExpectedPath = ytdlpPath,
                DownloadUrl = "https://github.com/yt-dlp/yt-dlp/releases",
                IsRequired = true
            });
            Console.WriteLine($"[DEPENDENCY] MISSING: yt-dlp.exe at {ytdlpPath}");
        }
        else
        {
            var fileInfo = new FileInfo(ytdlpPath);
            var sizeMB = fileInfo.Length / 1024.0 / 1024.0;
            Console.WriteLine($"[DEPENDENCY] ✓ yt-dlp found: {sizeMB:F2} MB at {ytdlpPath}");

            // Try to get version
            try
            {
                var versionInfo = await GetYtDlpVersionAsync(ytdlpPath);
                Console.WriteLine($"[DEPENDENCY] yt-dlp version: {versionInfo}");
            }
            catch
            {
                Console.WriteLine("[DEPENDENCY] Could not retrieve yt-dlp version");
            }
        }

        // Check for ffmpeg.exe
        var ffmpegPath = Path.Combine(baseDir, "ffmpeg.exe");
        if (!File.Exists(ffmpegPath))
        {
            result.MissingDependencies.Add(new MissingDependency
            {
                Name = "FFmpeg",
                ExpectedPath = ffmpegPath,
                DownloadUrl = "https://www.gyan.dev/ffmpeg/builds/",
                IsRequired = true
            });
            Console.WriteLine($"[DEPENDENCY] MISSING: ffmpeg.exe at {ffmpegPath}");
        }
        else
        {
            var fileInfo = new FileInfo(ffmpegPath);
            var sizeMB = fileInfo.Length / 1024.0 / 1024.0;
            Console.WriteLine($"[DEPENDENCY] ✓ FFmpeg found: {sizeMB:F2} MB at {ffmpegPath}");

            // Try to get version
            try
            {
                var versionInfo = await GetFFmpegVersionAsync(ffmpegPath);
                Console.WriteLine($"[DEPENDENCY] FFmpeg version: {versionInfo}");
            }
            catch
            {
                Console.WriteLine("[DEPENDENCY] Could not retrieve FFmpeg version");
            }
        }

        if (result.IsValid)
        {
            Console.WriteLine("[DEPENDENCY] ✓ All dependencies validated successfully");
        }
        else
        {
            Console.WriteLine($"[DEPENDENCY] ✗ Validation failed: {result.MissingDependencies.Count} missing dependencies");
        }

        return result;
    }

    private static async Task<string> GetYtDlpVersionAsync(string ytdlpPath)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ytdlpPath,
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return output.Trim();
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    private static async Task<string> GetFFmpegVersionAsync(string ffmpegPath)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            // Extract just the version line (first line)
            var firstLine = output.Split('\n')[0];
            return firstLine.Trim();
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
