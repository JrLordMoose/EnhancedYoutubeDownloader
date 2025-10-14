using System.Collections.Concurrent;
using System.Reactive.Subjects;
using EnhancedYoutubeDownloader.Shared.Interfaces;
using EnhancedYoutubeDownloader.Shared.Models;
using Gress;
using YoutubeExplode.Videos;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace EnhancedYoutubeDownloader.Core.Services;

public class YtDlpDownloadService : IDownloadService, IDisposable
{
    private readonly ConcurrentDictionary<string, DownloadItem> _downloads = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cancellationTokens = new();
    private readonly Subject<DownloadItem> _downloadStatusChanged = new();
    private readonly Subject<double> _overallProgress = new();
    private readonly YoutubeDL _ytdl;
    private readonly SemaphoreSlim _downloadSemaphore;
    private readonly DownloadStateRepository _stateRepository;
    private readonly ISubtitleBurnInService _subtitleBurnInService;

    public YtDlpDownloadService(ISubtitleBurnInService subtitleBurnInService, int maxConcurrentDownloads = 3)
    {
        _subtitleBurnInService = subtitleBurnInService ?? throw new ArgumentNullException(nameof(subtitleBurnInService));
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

    public IObservable<DownloadItem> DownloadStatusChanged => _downloadStatusChanged;
    public IObservable<double> OverallProgress => _overallProgress;

    public Task<DownloadItem> CreateDownloadAsync(IVideo video, string filePath, FormatProfile? profile = null, PlatformType platform = PlatformType.YouTube)
    {
        Console.WriteLine($"[YTDLP] Creating download for: {video.Title}");

        var downloadItem = new DownloadItem
        {
            Video = video,
            FilePath = filePath,
            Platform = platform,
            FormatProfile = profile ?? GetDefaultProfile(),
            Status = DownloadStatus.Queued,
            PartialFilePath = filePath + ".part",
            CanStart = true,
            CanPause = false,
            CanResume = false,
            CanCancel = true,
            CanRestart = false
        };

        _downloads[downloadItem.Id] = downloadItem;
        _downloadStatusChanged.OnNext(downloadItem);

        Console.WriteLine($"[YTDLP] Download item created: {downloadItem.Id} with profile: {downloadItem.FormatProfile.Quality} {downloadItem.FormatProfile.Container}");
        return Task.FromResult(downloadItem);
    }

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
        downloadItem.CanStart = false;
        downloadItem.CanPause = true;
        downloadItem.CanCancel = true;
        downloadItem.CanResume = false;
        downloadItem.CanRestart = false;
        downloadItem.CanOpen = false;
        _downloadStatusChanged.OnNext(downloadItem);

        // Create a new CancellationTokenSource for this download
        var cts = new CancellationTokenSource();
        _cancellationTokens[downloadItem.Id] = cts;

        _ = Task.Run(async () => await ProcessDownloadAsync(downloadItem, cts.Token));

        return Task.CompletedTask;
    }

    public async Task PauseDownloadAsync(DownloadItem downloadItem)
    {
        if (downloadItem.Status != DownloadStatus.Started)
            return;

        Console.WriteLine($"[YTDLP] Pausing download: {downloadItem.Id}");

        // Set status BEFORE canceling to avoid race condition
        downloadItem.Status = DownloadStatus.Paused;
        downloadItem.CanPause = false;
        downloadItem.CanResume = true;
        downloadItem.CanCancel = true;
        downloadItem.CanRestart = false;
        downloadItem.CanOpen = false;

        // Reset speed and ETA tracking
        downloadItem.ResetTracking();

        // Notify status change BEFORE cancellation
        _downloadStatusChanged.OnNext(downloadItem);

        // Cancel the download operation AFTER status is set
        if (_cancellationTokens.TryGetValue(downloadItem.Id, out var cts))
        {
            Console.WriteLine($"[YTDLP] Canceling download token for {downloadItem.Id}");
            cts.Cancel();
        }

        // Save state to repository
        Console.WriteLine($"[YTDLP] Saving pause state for {downloadItem.Id}");
        await _stateRepository.SaveStateAsync(downloadItem);
    }

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
        downloadItem.CanOpen = false;
        _downloadStatusChanged.OnNext(downloadItem);

        // Create a new CancellationTokenSource for the resumed download
        var cts = new CancellationTokenSource();
        _cancellationTokens[downloadItem.Id] = cts;

        _ = Task.Run(async () => await ProcessDownloadAsync(downloadItem, cts.Token));
    }

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
        downloadItem.CanOpen = false;

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
        downloadItem.CanStart = true;
        downloadItem.CanPause = false;
        downloadItem.CanResume = false;
        downloadItem.CanCancel = true;
        downloadItem.CanRestart = false;
        downloadItem.CanOpen = false;

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

        try
        {
            // Move WaitAsync inside try block to ensure semaphore is released even on early cancellation
            await _downloadSemaphore.WaitAsync(cancellationToken);
            Console.WriteLine($"[YTDLP] Semaphore acquired for {downloadItem.Id}");

            if (cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine($"[YTDLP] Cancellation requested before start for {downloadItem.Id}");
                return;
            }

            // Validate download item
            if (downloadItem.Video == null)
            {
                Console.WriteLine($"[YTDLP] ERROR: Video is null for download {downloadItem.Id}");
                throw new InvalidOperationException($"Cannot download: Video is null for download {downloadItem.Id}");
            }

            if (string.IsNullOrWhiteSpace(downloadItem.FilePath))
            {
                Console.WriteLine($"[YTDLP] ERROR: FilePath is null or empty for download {downloadItem.Id}");
                throw new InvalidOperationException($"Cannot download: FilePath is required for download {downloadItem.Id}");
            }

            // Validate and create directory if needed
            var directory = Path.GetDirectoryName(downloadItem.FilePath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                Console.WriteLine($"[YTDLP] ERROR: Invalid directory path for {downloadItem.Id}");
                throw new InvalidOperationException($"Cannot download: Invalid directory path for {downloadItem.FilePath}");
            }

            if (!Directory.Exists(directory))
            {
                Console.WriteLine($"[YTDLP] Creating directory: {directory}");
                Directory.CreateDirectory(directory);
            }

            // Build yt-dlp options from FormatProfile
            var profile = downloadItem.FormatProfile ?? GetDefaultProfile();
            var formatString = BuildFormatString(profile);
            var isAudioOnly = IsAudioOnly(profile);

            // For BurnedIn subtitles, we need to write .srt file but NOT embed it
            // We'll burn it in post-processing with FFmpeg
            var shouldEmbedSubs = profile.IncludeSubtitles && profile.SubtitleStyle != SubtitleStyle.BurnedIn;
            var shouldWriteSubs = profile.IncludeSubtitles; // Always write if subtitles enabled

            var options = new OptionSet
            {
                Format = formatString,
                Output = downloadItem.FilePath, // Direct output to final path
                NoPlaylist = true,
                NoPart = true, // Disable .part files to avoid path issues with non-YouTube URLs

                // Subtitle options (both manual and auto-generated)
                WriteSubs = shouldWriteSubs,
                WriteAutoSubs = shouldWriteSubs, // Also download auto-generated captions
                EmbedSubs = shouldEmbedSubs,
                SubLangs = "en",
                SubFormat = shouldWriteSubs ? "srt/best" : null, // Convert to SRT for compatibility

                // Metadata and thumbnail embedding (not supported by WebM)
                EmbedMetadata = profile.IncludeTags && profile.Container != "webm",
                EmbedThumbnail = profile.IncludeTags && profile.Container != "webm",

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
            Console.WriteLine($"[YTDLP] Subtitles: {profile.IncludeSubtitles}, Style: {profile.SubtitleStyle}, Tags: {profile.IncludeTags}");
            Console.WriteLine($"[YTDLP] Subtitle Settings - EmbedSubs: {shouldEmbedSubs}, WriteSubs: {shouldWriteSubs}");
            Console.WriteLine($"[YTDLP] Output path: {downloadItem.FilePath}");

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
                Console.WriteLine($"[YTDLP] Download completed, checking file location");
                Console.WriteLine($"[YTDLP] Expected FilePath: {downloadItem.FilePath}");

                // yt-dlp may save to various locations, search for the actual file
                string? actualFilePath = null;

                // Check expected location first
                if (File.Exists(downloadItem.FilePath))
                {
                    actualFilePath = downloadItem.FilePath;
                    Console.WriteLine($"[YTDLP] File found at expected location: {actualFilePath}");
                }
                // Check if yt-dlp added extension to the FilePath (e.g., .mp4.part.mp4)
                else
                {
                    var searchDir = Path.GetDirectoryName(downloadItem.FilePath) ?? "";
                    var baseFileName = Path.GetFileNameWithoutExtension(downloadItem.FilePath);

                    // Search for files matching the base name
                    // Order by file size (largest first) to avoid picking up small stub files
                    var possibleFiles = Directory.GetFiles(searchDir, $"{baseFileName}*")
                        .Where(f => !string.IsNullOrEmpty(baseFileName) && f.Contains(baseFileName))
                        .OrderByDescending(f => new FileInfo(f).Length)
                        .ToList();

                    Console.WriteLine($"[YTDLP] Searching directory: {searchDir}");
                    Console.WriteLine($"[YTDLP] Base filename: {baseFileName}");
                    Console.WriteLine($"[YTDLP] Found {possibleFiles.Count} possible files");

                    if (possibleFiles.Any())
                    {
                        actualFilePath = possibleFiles.First();
                        Console.WriteLine($"[YTDLP] Found actual file: {actualFilePath}");

                        // If it's not at the expected location, move it
                        if (actualFilePath != downloadItem.FilePath && !string.IsNullOrEmpty(downloadItem.FilePath))
                        {
                            if (File.Exists(downloadItem.FilePath))
                            {
                                File.Delete(downloadItem.FilePath);
                            }
                            File.Move(actualFilePath, downloadItem.FilePath!);
                            actualFilePath = downloadItem.FilePath;
                            Console.WriteLine($"[YTDLP] File moved to expected location: {actualFilePath}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[YTDLP] ERROR: No matching files found!");
                    }
                }

                // Update FilePath to actual location
                if (!string.IsNullOrEmpty(actualFilePath))
                {
                    downloadItem.FilePath = actualFilePath;
                    Console.WriteLine($"[YTDLP] Final FilePath: {downloadItem.FilePath}");
                }
                else
                {
                    Console.WriteLine($"[YTDLP] ERROR: Could not locate downloaded file!");
                }

                // Post-processing: Burn in subtitles if requested
                if (profile.IncludeSubtitles && profile.SubtitleStyle == SubtitleStyle.BurnedIn && !string.IsNullOrEmpty(downloadItem.FilePath))
                {
                    await ProcessBurnInSubtitlesAsync(downloadItem, cancellationToken);
                }

                downloadItem.Status = DownloadStatus.Completed;
                downloadItem.CompletedAt = DateTime.UtcNow;
                downloadItem.Progress = 100;

                // Set file size from completed file if not already set
                if (downloadItem.TotalBytes == 0 && File.Exists(downloadItem.FilePath))
                {
                    var fileInfo = new FileInfo(downloadItem.FilePath);
                    downloadItem.TotalBytes = fileInfo.Length;
                    downloadItem.BytesDownloaded = fileInfo.Length;
                    Console.WriteLine($"[YTDLP] Set file size from completed file: {fileInfo.Length} bytes");
                }

                downloadItem.CanPause = false;
                downloadItem.CanResume = false;
                downloadItem.CanCancel = false;
                downloadItem.CanRestart = true;
                downloadItem.CanOpen = true;

                Console.WriteLine($"[YTDLP] Setting CanOpen=true for completed download");
                Console.WriteLine($"[YTDLP] FilePath: {downloadItem.FilePath}");

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

            // Build detailed error message with context
            var errorMessage = BuildDetailedErrorMessage(ex, downloadItem);

            downloadItem.Status = DownloadStatus.Failed;
            downloadItem.ErrorMessage = errorMessage;
            downloadItem.CanPause = false;
            downloadItem.CanResume = false;
            downloadItem.CanCancel = false;
            downloadItem.CanRestart = true;
            downloadItem.CanOpen = false;
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

    private async Task ProcessBurnInSubtitlesAsync(DownloadItem downloadItem, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(downloadItem.FilePath) || !File.Exists(downloadItem.FilePath))
            {
                Console.WriteLine($"[BURN-IN] Video file not found: {downloadItem.FilePath}");
                return;
            }

            var profile = downloadItem.FormatProfile ?? GetDefaultProfile();
            var videoDirectory = Path.GetDirectoryName(downloadItem.FilePath) ?? "";
            var videoFileName = Path.GetFileName(downloadItem.FilePath);
            var videoFileNameWithoutExt = Path.GetFileNameWithoutExtension(downloadItem.FilePath);

            // Find the .srt subtitle file (yt-dlp saves it with .part extension during download)
            // Try multiple patterns: .mp4.part.en.srt, .part.en.srt, .en.srt, .mp4.part.srt, .part.srt, .srt
            string? subtitlePath = null;
            var searchPatterns = new[]
            {
                $"{videoFileName}.part.en.srt",     // is_video.mp4.part.en.srt
                $"{videoFileNameWithoutExt}.part.en.srt", // is_video.part.en.srt
                $"{videoFileNameWithoutExt}.en.srt", // is_video.en.srt
                $"{videoFileName}.part.srt",         // is_video.mp4.part.srt
                $"{videoFileNameWithoutExt}.part.srt", // is_video.part.srt
                $"{videoFileNameWithoutExt}.srt"     // is_video.srt
            };

            foreach (var pattern in searchPatterns)
            {
                var candidatePath = Path.Combine(videoDirectory, pattern);
                if (File.Exists(candidatePath))
                {
                    subtitlePath = candidatePath;
                    break;
                }
            }

            if (subtitlePath == null)
            {
                Console.WriteLine($"[BURN-IN] Subtitle file not found. Searched for:");
                foreach (var pattern in searchPatterns)
                {
                    Console.WriteLine($"[BURN-IN]   - {pattern}");
                }
                return;
            }

            Console.WriteLine($"[BURN-IN] Found subtitle file: {subtitlePath}");
            Console.WriteLine($"[BURN-IN] Starting subtitle burn-in process...");

            // Update download item status to show burn-in is in progress
            downloadItem.Progress = 0;
            downloadItem.ErrorMessage = "Burning in subtitles...";
            _downloadStatusChanged.OnNext(downloadItem);

            // Create temporary output path
            var tempOutputPath = Path.Combine(videoDirectory, $"{videoFileNameWithoutExt}_burned.mp4");

            // Set up progress reporting
            var burnInProgress = new Progress<double>(p =>
            {
                // Map burn-in progress to download progress (0-100)
                downloadItem.Progress = p * 100;
                downloadItem.ErrorMessage = $"Burning in subtitles... {downloadItem.Progress:F0}%";
                _downloadStatusChanged.OnNext(downloadItem);

                if ((int)downloadItem.Progress % 10 == 0) // Log every 10%
                {
                    Console.WriteLine($"[BURN-IN] Progress: {downloadItem.Progress:F0}%");
                }
            });

            // Burn in subtitles using the service
            var success = await _subtitleBurnInService.BurnSubtitlesAsync(
                videoPath: downloadItem.FilePath,
                subtitlePath: subtitlePath,
                outputPath: tempOutputPath,
                progress: burnInProgress,
                cancellationToken: cancellationToken
            );

            if (!success)
            {
                Console.WriteLine($"[BURN-IN] Failed to burn in subtitles");
                downloadItem.ErrorMessage = "Failed to burn in subtitles, continuing with original video";
                _downloadStatusChanged.OnNext(downloadItem);

                // Clean up temp file if it exists
                if (File.Exists(tempOutputPath))
                {
                    File.Delete(tempOutputPath);
                }
                return;
            }

            Console.WriteLine($"[BURN-IN] Burn-in successful, replacing original video");

            // Replace original video with burned version
            var originalFilePath = downloadItem.FilePath;
            var backupPath = originalFilePath + ".backup";

            try
            {
                // Backup original file
                File.Move(originalFilePath, backupPath);

                // Move burned version to final location
                File.Move(tempOutputPath, originalFilePath);

                // Delete backup
                File.Delete(backupPath);

                Console.WriteLine($"[BURN-IN] Video replaced successfully: {originalFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BURN-IN] Error replacing file: {ex.Message}");

                // Restore backup if move failed
                if (File.Exists(backupPath) && !File.Exists(originalFilePath))
                {
                    File.Move(backupPath, originalFilePath);
                }

                throw;
            }

            // Clean up subtitle file
            try
            {
                if (File.Exists(subtitlePath))
                {
                    File.Delete(subtitlePath);
                    Console.WriteLine($"[BURN-IN] Cleaned up subtitle file: {subtitlePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BURN-IN] Warning: Could not delete subtitle file: {ex.Message}");
                // Non-critical, continue
            }

            // Update download item with final file size
            if (File.Exists(downloadItem.FilePath))
            {
                var fileInfo = new FileInfo(downloadItem.FilePath);
                downloadItem.TotalBytes = fileInfo.Length;
                downloadItem.BytesDownloaded = fileInfo.Length;
                Console.WriteLine($"[BURN-IN] Updated file size: {fileInfo.Length} bytes");
            }

            downloadItem.Progress = 100;
            downloadItem.ErrorMessage = null;
            Console.WriteLine($"[BURN-IN] Subtitle burn-in process completed successfully");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[BURN-IN] Burn-in process was canceled");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BURN-IN] Error during burn-in process: {ex.Message}");
            Console.WriteLine($"[BURN-IN] Stack trace: {ex.StackTrace}");
            downloadItem.ErrorMessage = $"Subtitle burn-in failed: {ex.Message}";
            _downloadStatusChanged.OnNext(downloadItem);
            // Don't throw - we have the video, just without burned subtitles
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

    private static string BuildDetailedErrorMessage(Exception ex, DownloadItem downloadItem)
    {
        var errorType = ex.GetType().Name;
        var baseMessage = ex.Message;

        // Categorize error and add context
        if (ex is UnauthorizedAccessException || ex is System.Security.SecurityException)
        {
            return $"Permission denied accessing '{downloadItem.FilePath}'. Check folder permissions and try again.";
        }
        else if (ex is IOException ioEx)
        {
            // Check for disk space issues
            if (baseMessage.Contains("disk", StringComparison.OrdinalIgnoreCase) ||
                baseMessage.Contains("space", StringComparison.OrdinalIgnoreCase))
            {
                return $"Insufficient disk space for '{Path.GetFileName(downloadItem.FilePath)}'. Free up space and restart download.";
            }
            // Check for file in use
            else if (baseMessage.Contains("being used", StringComparison.OrdinalIgnoreCase) ||
                     baseMessage.Contains("in use", StringComparison.OrdinalIgnoreCase))
            {
                return $"File is in use: '{Path.GetFileName(downloadItem.FilePath)}'. Close other programs and restart.";
            }
            else
            {
                return $"File error: {baseMessage}. Check file path and permissions.";
            }
        }
        else if (ex is System.Net.Http.HttpRequestException || ex is System.Net.WebException ||
                 baseMessage.Contains("network", StringComparison.OrdinalIgnoreCase) ||
                 baseMessage.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
                 baseMessage.Contains("timeout", StringComparison.OrdinalIgnoreCase))
        {
            return $"Network error downloading '{downloadItem.Video?.Title ?? "video"}'. Check internet connection and restart.";
        }
        else if (baseMessage.Contains("unavailable", StringComparison.OrdinalIgnoreCase) ||
                 baseMessage.Contains("not available", StringComparison.OrdinalIgnoreCase) ||
                 baseMessage.Contains("removed", StringComparison.OrdinalIgnoreCase) ||
                 baseMessage.Contains("private", StringComparison.OrdinalIgnoreCase))
        {
            return $"Video unavailable: {baseMessage}";
        }
        else if (baseMessage.Contains("format", StringComparison.OrdinalIgnoreCase) ||
                 baseMessage.Contains("quality", StringComparison.OrdinalIgnoreCase))
        {
            return $"Format not available for '{downloadItem.Video?.Title ?? "video"}'. Try different quality/format.";
        }
        else if (ex is InvalidOperationException)
        {
            return $"Download error: {baseMessage}. Try restarting the download.";
        }
        else
        {
            // Generic error with video context
            var videoTitle = downloadItem.Video?.Title;
            if (!string.IsNullOrWhiteSpace(videoTitle) && videoTitle.Length > 40)
            {
                videoTitle = videoTitle.Substring(0, 37) + "...";
            }

            return !string.IsNullOrWhiteSpace(videoTitle)
                ? $"{baseMessage} ('{videoTitle}'). Click Restart to retry."
                : $"{baseMessage}. Click Restart to retry.";
        }
    }

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
