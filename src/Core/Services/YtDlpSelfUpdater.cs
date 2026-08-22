using System.Diagnostics;
using EnhancedYoutubeDownloader.Core.Utils;

namespace EnhancedYoutubeDownloader.Core.Services;

/// <summary>
/// Updates the bundled yt-dlp binary in place so YouTube extractor fixes ship without a full app release.
/// </summary>
/// <remarks>
/// YouTube changes its player often enough that a yt-dlp binary frozen at release time
/// stops working within months. Running the official binary's own <c>-U</c> updater on
/// startup keeps extraction working between app releases. This is best-effort: any
/// failure (offline, read-only install directory, antivirus) is logged and ignored.
/// </remarks>
public static class YtDlpSelfUpdater
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(2);

    public static async Task TryUpdateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var exeName = OperatingSystem.IsWindows() ? "yt-dlp.exe" : "yt-dlp";
            var ytdlpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, exeName);
            if (!File.Exists(ytdlpPath))
            {
                TraceLog.Write($"[YTDLP-UPDATE] Skipped, binary not found: {ytdlpPath}");
                return;
            }

            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken
            );
            timeoutSource.CancelAfter(Timeout);

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ytdlpPath,
                    Arguments = "-U",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                },
            };

            process.Start();

            // Drain both pipes concurrently; yt-dlp can otherwise block on a full buffer.
            var stdoutTask = process.StandardOutput.ReadToEndAsync(timeoutSource.Token);
            var stderrTask = process.StandardError.ReadToEndAsync(timeoutSource.Token);
            await process.WaitForExitAsync(timeoutSource.Token);

            var stdout = (await stdoutTask).Trim();
            var stderr = (await stderrTask).Trim();

            TraceLog.Write(
                $"[YTDLP-UPDATE] Exit {process.ExitCode}. {stdout}{(stderr.Length > 0 ? $" | {stderr}" : "")}"
            );
        }
        catch (Exception ex)
        {
            // Never let a failed update break startup - the bundled binary still works.
            TraceLog.Write($"[YTDLP-UPDATE] Update attempt failed (non-fatal): {ex.Message}");
        }
    }
}
