using System.Collections.Concurrent;
using System.Reactive.Subjects;
using EnhancedYoutubeDownloader.Shared.Interfaces;
using EnhancedYoutubeDownloader.Shared.Models;
using Gress;
using YoutubeExplode.Videos;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace EnhancedYoutubeDownloader.Core.Services;

/// <summary>
/// A download service that uses yt-dlp to download videos.
/// </summary>
public class YtDlpDownloadService : IDownloadService, IDisposable
{
    private readonly ConcurrentDictionary<string, DownloadItem> _downloads = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cancellationTokens = new();
    private readonly Subject<DownloadItem> _downloadStatusChanged = new();
    private readonly Subject<double> _overallProgress = new();
    private readonly YoutubeDL _ytdl;
    private readonly SemaphoreSlim _downloadSemaphore;
    private readonly DownloadStateRepository _stateRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="YtDlpDownloadService"/> class.
    /// </summary>
    /// <param name="maxConcurrentDownloads">The maximum number of concurrent downloads.</param>
    public YtDlpDownloadService(int maxConcurrentDownloads = 3)
    {
        // Initialize YoutubeDL with path to yt-dlp.exe
        var ytdlpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp.exe");
        var ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");

        Console.WriteLine($"[YTDLP] Initializing YoutubeDL with yt-dlp: {ytdlpPath}");
        Console.WriteLine($"[YTDLP] FFmpeg path: {ffmpegPath}");

        _ytdl = new YoutubeDL
        {
            YoutubeDLPath = ytdlpPath,
            FFmpegPath = ffmpegPath,
            OutputFolder = Path.GetTempPath(),
            OverwriteFiles = false
        };

        _downloadSemaphore = new SemaphoreSlim(maxConcurrentDownloads, maxConcurrentDownloads);
        _stateRepository = new DownloadStateRepository();
    }

    /// <inheritdoc />
    public IObservable<DownloadItem> DownloadStatusChanged => _downloadStatusChanged;

    /// <inheritdoc />
    public IObservable<double> OverallProgress => _overallProgress;

    /// <inheritdoc />
    public Task<DownloadItem> CreateDownloadAsync(IVideo video, string filePath, FormatProfile? profile = null)
    {
        Console.WriteLine($"[YTDLP] Creating download for: {video.Title}");

        var downloadItem = new DownloadItem
        {
            Video = video,
            FilePath = filePath,
            FormatProfile = profile ?? GetDefaultProfile(),
            Status = DownloadStatus.Queued,
            PartialFilePath = filePath + ".part"
        };

        _downloads[downloadItem.Id] = downloadItem;
        _downloadStatusChanged.OnNext(downloadItem);

        Console.WriteLine($"[YTDLP] Download item created: {downloadItem.Id} with profile: {downloadItem.FormatProfile.Quality} {downloadItem.FormatProfile.Container}");
        return Task.FromResult(downloadItem);
    }

    /// <inheritdoc />
    public Task StartDownloadAsync(DownloadItem downloadItem)
    {
        if (downloadItem.Status != DownloadStatus.Queued && downloadItem.Status != DownloadStatus.Paused)
        {
            Console.WriteLine($"[YTDLP] Cannot start download {downloadItem.Id} - invalid status: {downloadItem.Status}");
            return Task.CompletedTask;
        }

        Console.WriteLine($"[YTDLP] Starting download: {downloadItem.Id}");

        downloadItem.Status = DownloadStatus.Started;
        downloadItem.StartedAt = DateTime.UtcNow;
        downloadItem.CanPause = true;
        downloadItem.CanCancel = true;
        downloadItem.CanResume = false;
        downloadItem.CanRestart = false;
        _downloadStatusChanged.OnNext(downloadItem);

        // Create a new CancellationTokenSource for this download
        var cts = new CancellationTokenSource();
        _cancellationTokens[downloadItem.Id] = cts;

        _ = Task.Run(async () => await ProcessDownloadAsync(downloadItem, cts.Token));

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task PauseDownloadAsync(DownloadItem downloadItem)
    {
        if (downloadItem.Status != DownloadStatus.Started)
            return;

        Console.WriteLine($"[YTDLP] Pausing download: {downloadItem.Id}");

        // Cancel the download operation
        if (_cancellationTokens.TryGetValue(downloadItem.Id, out var cts))
        {
            cts.Cancel();
        }

        downloadItem.Status = DownloadStatus.Paused;
        downloadItem.CanPause = false;
        downloadItem.CanResume = true;
        downloadItem.CanCancel = true;
        downloadItem.CanRestart = false;

        // Reset speed and ETA tracking
        downloadItem.ResetTracking();

        // Save state to repository
        await _stateRepository.SaveStateAsync(downloadItem);

        _downloadStatusChanged.OnNext(downloadItem);
    }

    /// <inheritdoc />
    public async Task ResumeDownloadAsync(DownloadItem downloadItem)
    {
        if (downloadItem.Status != DownloadStatus.Paused)
            return;

        Console.WriteLine($"[YTDLP] Resuming download: {downloadItem.Id}");

        // Load saved state if available
        var savedState = await _stateRepository.LoadStateAsync(downloadItem.Id);
        if (savedState != null)
        {
            // Validate partial file
            if (!string.IsNullOrEmpty(savedState.PartialFilePath) && File.Exists(savedState.PartialFilePath))
            {
                var fileInfo = new FileInfo(savedState.PartialFilePath);
                if (fileInfo.Length == savedState.BytesDownloaded)
                {
                    // State is valid, restore it
                    downloadItem.BytesDownloaded = savedState.BytesDownloaded;
                    downloadItem.TotalBytes = savedState.TotalBytes;
                    downloadItem.PartialFilePath = savedState.PartialFilePath;
                    Console.WriteLine($"[YTDLP] Restored state: {downloadItem.BytesDownloaded}/{downloadItem.TotalBytes} bytes");
                }
                else
                {
                    Console.WriteLine($"[YTDLP] File size mismatch, resetting state");
                    // File size mismatch, reset
                    downloadItem.BytesDownloaded = 0;
                    if (File.Exists(savedState.PartialFilePath))
                    {
                        File.Delete(savedState.PartialFilePath);
                    }
                }
            }
        }

        downloadItem.Status = DownloadStatus.Started;
        downloadItem.CanPause = true;
        downloadItem.CanResume = false;
        downloadItem.CanCancel = true;
        downloadItem.CanRestart = false;
        _downloadStatusChanged.OnNext(downloadItem);

        // Create a new CancellationTokenSource for the resumed download
        var cts = new CancellationTokenSource();
        _cancellationTokens[downloadItem.Id] = cts;

        _ = Task.Run(async () => await ProcessDownloadAsync(downloadItem, cts.Token));
    }

    /// <inheritdoc />
    public async Task CancelDownloadAsync(DownloadItem downloadItem)
    {
        Console.WriteLine($"[YTDLP] Canceling download: {downloadItem.Id}");

        // Cancel the download operation
        if (_cancellationTokens.TryGetValue(downloadItem.Id, out var cts))
        {
            cts.Cancel();
        }

        downloadItem.Status = DownloadStatus.Canceled;
        downloadItem.CanPause = false;
        downloadItem.CanResume = false;
        downloadItem.CanCancel = false;
        downloadItem.CanRestart = true;

        // Reset speed and ETA tracking
        downloadItem.ResetTracking();

        // Clean up partial file
        if (!string.IsNullOrEmpty(downloadItem.PartialFilePath) && File.Exists(downloadItem.PartialFilePath))
        {
            File.Delete(downloadItem.PartialFilePath);
        }

        // Delete saved state
        await _stateRepository.DeleteStateAsync(downloadItem.Id);

        _downloadStatusChanged.OnNext(downloadItem);
    }

    /// <inheritdoc />
    public async Task RestartDownloadAsync(DownloadItem downloadItem)
    {
        Console.WriteLine($"[YTDLP] Restarting download: {downloadItem.Id}");

        downloadItem.Status = DownloadStatus.Queued;
        downloadItem.Progress = 0;
        downloadItem.BytesDownloaded = 0;
        downloadItem.TotalBytes = 0;
        downloadItem.ErrorMessage = null;
        downloadItem.StartedAt = null;
        downloadItem.CompletedAt = null;
        downloadItem.CanPause = false;
        downloadItem.CanResume = false;
        downloadItem.CanCancel = false;
        downloadItem.CanRestart = false;

        // Reset speed and ETA tracking
        downloadItem.ResetTracking();

        // Clean up partial file
        if (!string.IsNullOrEmpty(downloadItem.PartialFilePath) && File.Exists(downloadItem.PartialFilePath))
        {
            File.Delete(downloadItem.PartialFilePath);
        }

        // Delete saved state
        await _stateRepository.DeleteStateAsync(downloadItem.Id);

        _downloadStatusChanged.OnNext(downloadItem);

        await StartDownloadAsync(downloadItem);
    }

    /// <inheritdoc />
    public async Task DeleteDownloadAsync(DownloadItem downloadItem)
    {
        Console.WriteLine($"[YTDLP] Deleting download: {downloadItem.Id}");

        // Cancel if running
        if (_cancellationTokens.TryGetValue(downloadItem.Id, out var cts))
        {
            cts.Cancel();
        }

        _downloads.TryRemove(downloadItem.Id, out _);
        _cancellationTokens.TryRemove(downloadItem.Id, out _);

        // Clean up partial file
        if (!string.IsNullOrEmpty(downloadItem.PartialFilePath) && File.Exists(downloadItem.PartialFilePath))
        {
            File.Delete(downloadItem.PartialFilePath);
        }

        // Delete saved state
        await _stateRepository.DeleteStateAsync(downloadItem.Id);

        downloadItem.Status = DownloadStatus.Canceled;
        _downloadStatusChanged.OnNext(downloadItem);
    }

    private async Task ProcessDownloadAsync(DownloadItem downloadItem, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[YTDLP] ProcessDownloadAsync started for {downloadItem.Id}");
        await _downloadSemaphore.WaitAsync(cancellationToken);
        Console.WriteLine($"[YTDLP] Semaphore acquired for {downloadItem.Id}");

        try
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine($"[YTDLP] Cancellation requested before start for {downloadItem.Id}");
                return;
            }

            // Build yt-dlp options from FormatProfile
            var profile = downloadItem.FormatProfile ?? GetDefaultProfile();
            var formatString = BuildFormatString(profile);
            var isAudioOnly = IsAudioOnly(profile);

            var options = new OptionSet
            {
                Format = formatString,
                Output = downloadItem.PartialFilePath,
                NoPlaylist = true,
                NoPart = true, // Don't use .part files (we manage this ourselves)

                // Subtitle options
                WriteSubs = profile.IncludeSubtitles,
                EmbedSubs = profile.IncludeSubtitles,
                SubLangs = "en",

                // Metadata and thumbnail embedding
                EmbedMetadata = profile.IncludeTags,
                EmbedThumbnail = profile.IncludeTags,

                // Audio extraction (for audio-only downloads)
                ExtractAudio = isAudioOnly
            };

            // Only set AudioFormat if extracting audio
            if (isAudioOnly)
            {
                var audioFormat = GetAudioFormat(profile);
                if (audioFormat.HasValue)
                {
                    options.AudioFormat = audioFormat.Value;
                }
            }

            Console.WriteLine($"[YTDLP] Downloading video: {downloadItem.Video!.Url}");
            Console.WriteLine($"[YTDLP] Format: {formatString}");
            Console.WriteLine($"[YTDLP] Quality: {profile.Quality}, Container: {profile.Container}");
            Console.WriteLine($"[YTDLP] Subtitles: {profile.IncludeSubtitles}, Tags: {profile.IncludeTags}");
            Console.WriteLine($"[YTDLP] Output path: {downloadItem.PartialFilePath}");

            // Set up progress reporting
            var progress = new Progress<DownloadProgress>(p =>
            {
                downloadItem.Progress = p.Progress * 100;

                // Parse download size if available
                if (!string.IsNullOrEmpty(p.TotalDownloadSize))
                {
                    if (TryParseSize(p.TotalDownloadSize, out long totalBytes))
                    {
                        downloadItem.TotalBytes = totalBytes;
                        var newBytesDownloaded = (long)(totalBytes * p.Progress);

                        // Calculate speed and ETA
                        var currentTime = DateTime.UtcNow;
                        var timeDelta = (currentTime - downloadItem.LastUpdateTime).TotalSeconds;

                        if (timeDelta > 0.1) // Update at most every 100ms
                        {
                            var byteDelta = newBytesDownloaded - downloadItem.LastBytesDownloaded;

                            if (byteDelta > 0)
                            {
                                // Calculate instantaneous speed
                                var instantSpeed = byteDelta / timeDelta;

                                // Apply exponential moving average for smoothing
                                if (downloadItem.DownloadSpeed == 0)
                                {
                                    downloadItem.DownloadSpeed = instantSpeed;
                                }
                                else
                                {
                                    downloadItem.DownloadSpeed = (downloadItem.DownloadSpeed * 0.7) + (instantSpeed * 0.3);
                                }

                                // Calculate ETA
                                if (downloadItem.DownloadSpeed > 0)
                                {
                                    var remainingBytes = totalBytes - newBytesDownloaded;
                                    var etaSeconds = remainingBytes / downloadItem.DownloadSpeed;
                                    downloadItem.EstimatedTimeRemaining = TimeSpan.FromSeconds(etaSeconds);
                                }
                            }

                            downloadItem.UpdateTracking(newBytesDownloaded, currentTime);
                        }

                        downloadItem.BytesDownloaded = newBytesDownloaded;
                    }
                }

                _downloadStatusChanged.OnNext(downloadItem);

                if (downloadItem.Progress % 10 < 1) // Log every 10%
                {
                    Console.WriteLine($"[YTDLP] Progress: {downloadItem.Progress:F1}% • {downloadItem.FormattedSpeed} • ETA: {downloadItem.FormattedEta}");
                }
            });

            // Download with yt-dlp
            var result = await _ytdl.RunVideoDownload(
                downloadItem.Video!.Url,
                progress: progress,
                ct: cancellationToken,
                overrideOptions: options
            );

            if (!result.Success)
            {
                Console.WriteLine($"[YTDLP] Download failed: {string.Join(", ", result.ErrorOutput)}");
                throw new Exception($"yt-dlp download failed: {string.Join(", ", result.ErrorOutput)}");
            }

            // Download completed successfully
            if (!cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine($"[YTDLP] Download completed, moving file from .part");

                // Move .part file to final location
                if (!string.IsNullOrEmpty(downloadItem.PartialFilePath) &&
                    File.Exists(downloadItem.PartialFilePath))
                {
                    if (File.Exists(downloadItem.FilePath))
                    {
                        File.Delete(downloadItem.FilePath);
                    }
                    File.Move(downloadItem.PartialFilePath, downloadItem.FilePath!);
                    Console.WriteLine($"[YTDLP] File moved to: {downloadItem.FilePath}");
                }

                downloadItem.Status = DownloadStatus.Completed;
                downloadItem.CompletedAt = DateTime.UtcNow;
                downloadItem.Progress = 100;
                downloadItem.CanPause = false;
                downloadItem.CanResume = false;
                downloadItem.CanCancel = false;
                downloadItem.CanRestart = true;

                // Delete saved state
                await _stateRepository.DeleteStateAsync(downloadItem.Id);

                _downloadStatusChanged.OnNext(downloadItem);
                Console.WriteLine($"[YTDLP] Status updated to Completed");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[YTDLP] OperationCanceledException for {downloadItem.Id}");
            // Download was paused or canceled - state already updated by Pause/Cancel methods
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[YTDLP] EXCEPTION in ProcessDownloadAsync for {downloadItem.Id}");
            Console.WriteLine($"[YTDLP] Exception type: {ex.GetType().Name}");
            Console.WriteLine($"[YTDLP] Exception message: {ex.Message}");
            Console.WriteLine($"[YTDLP] Stack trace: {ex.StackTrace}");

            downloadItem.Status = DownloadStatus.Failed;
            downloadItem.ErrorMessage = ex.Message;
            downloadItem.CanPause = false;
            downloadItem.CanResume = false;
            downloadItem.CanCancel = false;
            downloadItem.CanRestart = true;
            _downloadStatusChanged.OnNext(downloadItem);
            Console.WriteLine($"[YTDLP] Status updated to Failed");
        }
        finally
        {
            _downloadSemaphore.Release();
            Console.WriteLine($"[YTDLP] Semaphore released for {downloadItem.Id}");
            _cancellationTokens.TryRemove(downloadItem.Id, out _);
        }
    }

    private static bool TryParseSize(string sizeString, out long bytes)
    {
        bytes = 0;
        if (string.IsNullOrEmpty(sizeString))
            return false;

        // Try to parse strings like "10.5MiB", "1.2GiB", "500KiB"
        var trimmed = sizeString.Trim();
        var numberPart = new string(trimmed.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
        var unitPart = trimmed.Substring(numberPart.Length).Trim();

        if (!double.TryParse(numberPart, out double value))
            return false;

        bytes = unitPart.ToUpperInvariant() switch
        {
            "B" => (long)value,
            "KB" or "KIB" => (long)(value * 1024),
            "MB" or "MIB" => (long)(value * 1024 * 1024),
            "GB" or "GIB" => (long)(value * 1024 * 1024 * 1024),
            _ => 0
        };

        return bytes > 0;
    }

    private static FormatProfile GetDefaultProfile()
    {
        return new FormatProfile
        {
            Quality = "highest",
            Container = "mp4",
            IncludeSubtitles = true,
            IncludeTags = true
        };
    }

    private static string BuildFormatString(FormatProfile profile)
    {
        // Handle audio-only formats
        if (profile.Quality == "audio-only")
        {
            return profile.Container switch
            {
                "mp3" => "bestaudio",
                "m4a" => "bestaudio[ext=m4a]/bestaudio",
                "webm" => "bestaudio[ext=webm]/bestaudio",
                _ => "bestaudio"
            };
        }

        // Handle video formats
        var qualityFilter = profile.Quality switch
        {
            "1080p" => "[height<=1080]",
            "720p" => "[height<=720]",
            "480p" => "[height<=480]",
            "360p" => "[height<=360]",
            _ => "" // highest quality, no filter
        };

        return profile.Container switch
        {
            "webm" => $"bestvideo{qualityFilter}[ext=webm]+bestaudio[ext=webm]/best{qualityFilter}[ext=webm]",
            "mp4" => $"bestvideo{qualityFilter}[ext=mp4]+bestaudio[ext=m4a]/best{qualityFilter}[ext=mp4]/best{qualityFilter}",
            _ => $"bestvideo{qualityFilter}[ext=mp4]+bestaudio[ext=m4a]/best{qualityFilter}[ext=mp4]/best{qualityFilter}"
        };
    }

    private static bool IsAudioOnly(FormatProfile profile)
    {
        return profile.Quality == "audio-only" || profile.Container == "mp3";
    }

    private static YoutubeDLSharp.Options.AudioConversionFormat? GetAudioFormat(FormatProfile profile)
    {
        if (!IsAudioOnly(profile))
            return null;

        return profile.Container switch
        {
            "mp3" => YoutubeDLSharp.Options.AudioConversionFormat.Mp3,
            "m4a" => YoutubeDLSharp.Options.AudioConversionFormat.M4a,
            "wav" => YoutubeDLSharp.Options.AudioConversionFormat.Wav,
            "flac" => YoutubeDLSharp.Options.AudioConversionFormat.Flac,
            _ => YoutubeDLSharp.Options.AudioConversionFormat.Best
        };
    }

    /// <summary>
    /// Disposes the service and cancels any ongoing downloads.
    /// </summary>
    public void Dispose()
    {
        foreach (var cts in _cancellationTokens.Values)
        {
            cts?.Cancel();
            cts?.Dispose();
        }
        _cancellationTokens.Clear();

        _downloadSemaphore?.Dispose();
        _downloadStatusChanged?.Dispose();
        _overallProgress?.Dispose();
        _stateRepository?.Dispose();
    }
}