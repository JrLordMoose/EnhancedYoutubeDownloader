using System.Collections.Concurrent;
using System.Reactive.Subjects;
using EnhancedYoutubeDownloader.Shared.Interfaces;
using EnhancedYoutubeDownloader.Shared.Models;
using Gress;
using YoutubeExplode.Videos;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace EnhancedYoutubeDownloader.Core.Services;
using EnhancedYoutubeDownloader.Core.Utils;

public class YtDlpDownloadService : IDownloadService, IDisposable
{
    private readonly ConcurrentDictionary<string, DownloadItem> _downloads = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cancellationTokens = new();
    private readonly Subject<DownloadItem> _downloadStatusChanged = new();
    private readonly Subject<double> _overallProgress = new();
    private readonly YoutubeDL _ytdl;
    private readonly object _slotLock = new();
    private readonly int _defaultParallelLimit;
    private int _activeDownloads;
    private readonly ISubtitleGenerationService? _subtitleGenerationService;
    private readonly DownloadStateRepository _stateRepository;
    private readonly ISubtitleBurnInService _subtitleBurnInService;
    private readonly object _settingsService;

    public YtDlpDownloadService(
        ISubtitleBurnInService subtitleBurnInService,
        object settingsService,
        int maxConcurrentDownloads = 3,
        ISubtitleGenerationService? subtitleGenerationService = null,
        DownloadStateRepository? stateRepository = null)
    {
        _subtitleGenerationService = subtitleGenerationService;
        _subtitleBurnInService = subtitleBurnInService ?? throw new ArgumentNullException(nameof(subtitleBurnInService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        // Initialize YoutubeDL with path to yt-dlp.exe
        var ytdlpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp.exe");
        var ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");

        TraceLog.Write($"[YTDLP] Initializing YoutubeDL with yt-dlp: {ytdlpPath}");
        TraceLog.Write($"[YTDLP] FFmpeg path: {ffmpegPath}");

        _ytdl = new YoutubeDL
        {
            YoutubeDLPath = ytdlpPath,
            FFmpegPath = ffmpegPath,
            OutputFolder = Path.GetTempPath(),
            // Pause/resume and Restart re-run yt-dlp on the same path. Without overwriting,
            // yt-dlp saw the cut-off file from the earlier run, said "already downloaded"
            // and the item showed Completed with a truncated video. Paths are made unique
            // when the download is created, so this only ever replaces our own partial file.
            OverwriteFiles = true
        };

        _defaultParallelLimit = Math.Max(1, maxConcurrentDownloads);
        // Use the DI instance so a custom cache folder is honoured
        _stateRepository = stateRepository ?? new DownloadStateRepository();
    }

    public IObservable<DownloadItem> DownloadStatusChanged => _downloadStatusChanged;
    public IObservable<double> OverallProgress => _overallProgress;

    public Task<DownloadItem> CreateDownloadAsync(IVideo video, string filePath, FormatProfile? profile = null, PlatformType platform = PlatformType.YouTube)
    {
        TraceLog.Write($"[YTDLP] Creating download for: {video.Title}");

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

        TraceLog.Write($"[YTDLP] Download item created: {downloadItem.Id} with profile: {downloadItem.FormatProfile.Quality} {downloadItem.FormatProfile.Container}");
        return Task.FromResult(downloadItem);
    }

    public Task StartDownloadAsync(DownloadItem downloadItem)
    {
        if (downloadItem.Status != DownloadStatus.Queued && downloadItem.Status != DownloadStatus.Paused)
        {
            TraceLog.Write($"[YTDLP] Cannot start download {downloadItem.Id} - invalid status: {downloadItem.Status}");
            return Task.CompletedTask;
        }

        TraceLog.Write($"[YTDLP] Starting download: {downloadItem.Id}");

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

        TraceLog.Write($"[YTDLP] Pausing download: {downloadItem.Id}");

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
            TraceLog.Write($"[YTDLP] Canceling download token for {downloadItem.Id}");
            cts.Cancel();
        }

        // Save state to repository
        TraceLog.Write($"[YTDLP] Saving pause state for {downloadItem.Id}");
        await _stateRepository.SaveStateAsync(downloadItem);
    }

    public async Task ResumeDownloadAsync(DownloadItem downloadItem)
    {
        if (downloadItem.Status != DownloadStatus.Paused)
            return;

        TraceLog.Write($"[YTDLP] Resuming download: {downloadItem.Id}");

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
                    TraceLog.Write($"[YTDLP] Restored state: {downloadItem.BytesDownloaded}/{downloadItem.TotalBytes} bytes");
                }
                else
                {
                    TraceLog.Write($"[YTDLP] File size mismatch, resetting state");
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
        TraceLog.Write($"[YTDLP] Canceling download: {downloadItem.Id}");

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

        // Remove the cut-off output and per-stream fragments of the stopped run
        DeletePartialOutputs(downloadItem);

        // Delete saved state
        await _stateRepository.DeleteStateAsync(downloadItem.Id);

        _downloadStatusChanged.OnNext(downloadItem);
    }

    public async Task RestartDownloadAsync(DownloadItem downloadItem)
    {
        TraceLog.Write($"[YTDLP] Restarting download: {downloadItem.Id}");

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
        TraceLog.Write($"[YTDLP] Deleting download: {downloadItem.Id}");

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

        // Remove what an unfinished run left behind (a cut-off file, per-stream fragments).
        // A completed download is the user's file and is never deleted here.
        if (downloadItem.Status != DownloadStatus.Completed)
        {
            DeletePartialOutputs(downloadItem);
        }

        // Delete saved state
        await _stateRepository.DeleteStateAsync(downloadItem.Id);

        downloadItem.Status = DownloadStatus.Canceled;
        _downloadStatusChanged.OnNext(downloadItem);
    }

    private async Task ProcessDownloadAsync(DownloadItem downloadItem, CancellationToken cancellationToken)
    {
        TraceLog.Write($"[YTDLP] ProcessDownloadAsync started for {downloadItem.Id}");

        var slotAcquired = false;
        var outputComplete = false;
        try
        {
            // Only release a slot we actually hold: canceling while queued must not free a slot.
            downloadItem.IsWaitingForSlot = true;
            try
            {
                await AcquireSlotAsync(cancellationToken);
            }
            finally
            {
                downloadItem.IsWaitingForSlot = false;
            }
            slotAcquired = true;
            TraceLog.Write($"[YTDLP] Download slot acquired for {downloadItem.Id}");

            if (cancellationToken.IsCancellationRequested)
            {
                TraceLog.Write($"[YTDLP] Cancellation requested before start for {downloadItem.Id}");
                return;
            }

            // Validate download item
            if (downloadItem.Video == null)
            {
                TraceLog.Write($"[YTDLP] ERROR: Video is null for download {downloadItem.Id}");
                throw new InvalidOperationException($"Cannot download: Video is null for download {downloadItem.Id}");
            }

            if (string.IsNullOrWhiteSpace(downloadItem.FilePath))
            {
                TraceLog.Write($"[YTDLP] ERROR: FilePath is null or empty for download {downloadItem.Id}");
                throw new InvalidOperationException($"Cannot download: FilePath is required for download {downloadItem.Id}");
            }

            // Validate and create directory if needed
            var directory = Path.GetDirectoryName(downloadItem.FilePath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                TraceLog.Write($"[YTDLP] ERROR: Invalid directory path for {downloadItem.Id}");
                throw new InvalidOperationException($"Cannot download: Invalid directory path for {downloadItem.FilePath}");
            }

            if (!Directory.Exists(directory))
            {
                TraceLog.Write($"[YTDLP] Creating directory: {directory}");
                Directory.CreateDirectory(directory);
            }

            // Build yt-dlp options from FormatProfile
            var profile = downloadItem.FormatProfile ?? GetDefaultProfile();
            var formatString = BuildFormatString(profile);
            var isAudioOnly = IsAudioOnly(profile);

            // For BurnedIn subtitles, we need to write .srt file but NOT embed it
            // We'll burn it in post-processing with FFmpeg
            // External keeps the .srt next to the video instead of embedding it.
            // Audio-only downloads never carry subtitles.
            var shouldWriteSubs = profile.IncludeSubtitles && !isAudioOnly;
            var shouldEmbedSubs = shouldWriteSubs && profile.SubtitleStyle == SubtitleStyle.Embedded;

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
                SubLangs = ReadSetting("SubtitleLanguage", "en"),
                SubFormat = shouldWriteSubs ? "srt/best" : null, // Convert to SRT for compatibility

                // Metadata and thumbnail embedding (not supported by WebM)
                EmbedMetadata = profile.IncludeTags && profile.Container != "webm",
                EmbedThumbnail = profile.IncludeTags && profile.Container != "webm",

                // Audio extraction (for audio-only downloads)
                ExtractAudio = isAudioOnly
            };

            ApplyCompatibilityOptions(options);

            if (shouldWriteSubs)
            {
                // --sub-format only *prefers* srt; convert so burn-in/external always get an .srt
                options.AddCustomOption("--convert-subs", "srt");
            }

            if (!isAudioOnly && profile.Container is "mp4" or "webm")
            {
                // Fallback formats can pick streams in another container; keep the requested one
                options.AddCustomOption("--merge-output-format", profile.Container);
            }

            // Authentication: a cookies.txt file wins over reading a browser's cookie store,
            // because Chromium browsers on Windows encrypt cookies so yt-dlp cannot read them.
            ApplyCookieOptions(options);

            // AI subtitles need the raw .srt on disk (we embed it ourselves afterwards),
            // so let yt-dlp write subtitles but not embed them when AI fallback is active.
            var aiSubtitlesActive = IsAiSubtitleFallbackActive(profile, isAudioOnly);
            if (aiSubtitlesActive)
            {
                options.EmbedSubs = false;
            }

            // Only set AudioFormat if extracting audio
            if (isAudioOnly)
            {
                var audioFormat = GetAudioFormat(profile);
                if (audioFormat.HasValue)
                {
                    options.AudioFormat = audioFormat.Value;
                }

                // Settings > Downloads > MP3 Quality (0 = best VBR)
                options.AddCustomOption("--audio-quality", GetAudioQualityArgument(ReadSetting("AudioQuality", 0)));
            }

            TraceLog.Write($"[YTDLP] Downloading video: {downloadItem.Video!.Url}");
            TraceLog.Write($"[YTDLP] Format: {formatString}");
            TraceLog.Write($"[YTDLP] Quality: {profile.Quality}, Container: {profile.Container}");
            TraceLog.Write($"[YTDLP] Subtitles: {profile.IncludeSubtitles}, Style: {profile.SubtitleStyle}, Tags: {profile.IncludeTags}");
            TraceLog.Write($"[YTDLP] Subtitle Settings - EmbedSubs: {shouldEmbedSubs}, WriteSubs: {shouldWriteSubs}");
            TraceLog.Write($"[YTDLP] Output path: {downloadItem.FilePath}");

            // Set up progress reporting
            var progress = new Progress<DownloadProgress>(p =>
            {
                // Progress<T> callbacks run on the thread pool and can arrive late/out of
                // order; ignore anything after the item left the Started state so a stale
                // callback can't rewind a Completed/Paused item or re-fire its events.
                if (downloadItem.Status != DownloadStatus.Started)
                    return;

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
                    TraceLog.Write($"[YTDLP] Progress: {downloadItem.Progress:F1}% • {downloadItem.FormattedSpeed} • ETA: {downloadItem.FormattedEta}");
                }
            });

            // Download with yt-dlp
            var result = await _ytdl.RunVideoDownload(
                downloadItem.Video!.Url,
                progress: progress,
                ct: cancellationToken,
                overrideOptions: options
            );

            // Browser cookies couldn't be read (Chrome/Edge/Brave on Windows): that shouldn't
            // fail a download that may not need sign-in at all. Retry once without them.
            if (!result.Success
                && YtDlpErrorClassifier.Classify(string.Join(", ", result.ErrorOutput)) == YtDlpErrorKind.CookieReadFailed
                && RemoveBrowserCookieOption(options))
            {
                TraceLog.Write("[YTDLP] Browser cookies unreadable; retrying without them");
                result = await _ytdl.RunVideoDownload(
                    downloadItem.Video!.Url,
                    progress: progress,
                    ct: cancellationToken,
                    overrideOptions: options
                );
            }

            if (!result.Success)
            {
                TraceLog.Write($"[YTDLP] Download failed: {string.Join(", ", result.ErrorOutput)}");
                throw new Exception($"yt-dlp download failed: {string.Join(", ", result.ErrorOutput)}");
            }

            // Download completed successfully
            if (!cancellationToken.IsCancellationRequested)
            {
                TraceLog.Write($"[YTDLP] Download completed, checking file location");
                TraceLog.Write($"[YTDLP] Expected FilePath: {downloadItem.FilePath}");

                // yt-dlp may save to various locations, search for the actual file
                string? actualFilePath = null;

                // Check expected location first
                if (File.Exists(downloadItem.FilePath))
                {
                    actualFilePath = downloadItem.FilePath;
                    TraceLog.Write($"[YTDLP] File found at expected location: {actualFilePath}");
                }
                // Check if yt-dlp added extension to the FilePath (e.g., .mp4.part.mp4)
                else
                {
                    var searchDir = Path.GetDirectoryName(downloadItem.FilePath) ?? "";
                    var baseFileName = Path.GetFileNameWithoutExtension(downloadItem.FilePath);

                    var possibleFiles = FindDownloadedFileCandidates(
                        searchDir,
                        baseFileName,
                        downloadItem.StartedAt ?? downloadItem.CreatedAt);

                    TraceLog.Write($"[YTDLP] Searching directory: {searchDir}");
                    TraceLog.Write($"[YTDLP] Base filename: {baseFileName}");
                    TraceLog.Write($"[YTDLP] Found {possibleFiles.Count} possible files");

                    if (possibleFiles.Any())
                    {
                        actualFilePath = possibleFiles.First();
                        TraceLog.Write($"[YTDLP] Found actual file: {actualFilePath}");

                        // If it's not at the expected location, move it
                        if (actualFilePath != downloadItem.FilePath && !string.IsNullOrEmpty(downloadItem.FilePath))
                        {
                            if (File.Exists(downloadItem.FilePath))
                            {
                                File.Delete(downloadItem.FilePath);
                            }
                            File.Move(actualFilePath, downloadItem.FilePath!);
                            actualFilePath = downloadItem.FilePath;
                            TraceLog.Write($"[YTDLP] File moved to expected location: {actualFilePath}");
                        }
                    }
                    else
                    {
                        TraceLog.Write($"[YTDLP] ERROR: No matching files found!");
                    }
                }

                // Update FilePath to actual location
                if (!string.IsNullOrEmpty(actualFilePath))
                {
                    downloadItem.FilePath = actualFilePath;
                    TraceLog.Write($"[YTDLP] Final FilePath: {downloadItem.FilePath}");
                    outputComplete = true;
                }
                else
                {
                    // yt-dlp reported success but no output exists: that is a failure, not
                    // "Completed" (QA saw Completed items with no file on disk).
                    TraceLog.Write($"[YTDLP] ERROR: Could not locate downloaded file!");
                    throw new InvalidOperationException(
                        "yt-dlp finished but the downloaded file could not be found");
                }

                // Embedded style: the track is inside the video now, so remove the .srt that
                // --convert-subs leaves next to it
                if (shouldEmbedSubs && !aiSubtitlesActive && !string.IsNullOrEmpty(downloadItem.FilePath))
                {
                    while (FindSubtitleFile(downloadItem.FilePath) is { } sidecar
                        && File.GetLastWriteTimeUtc(sidecar) >= (downloadItem.StartedAt ?? downloadItem.CreatedAt).AddSeconds(-5))
                    {
                        TryDelete(sidecar);
                        if (File.Exists(sidecar))
                            break;
                    }
                }

                // Post-processing: AI subtitle fallback when the site provided none
                if (aiSubtitlesActive && !string.IsNullOrEmpty(downloadItem.FilePath) && File.Exists(downloadItem.FilePath))
                {
                    await ProcessAiSubtitlesAsync(downloadItem, profile, cancellationToken);
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
                    TraceLog.Write($"[YTDLP] Set file size from completed file: {fileInfo.Length} bytes");
                }

                downloadItem.CanPause = false;
                downloadItem.CanResume = false;
                downloadItem.CanCancel = false;
                downloadItem.CanRestart = true;
                downloadItem.CanOpen = true;

                TraceLog.Write($"[YTDLP] Setting CanOpen=true for completed download");
                TraceLog.Write($"[YTDLP] FilePath: {downloadItem.FilePath}");

                // Delete saved state
                await _stateRepository.DeleteStateAsync(downloadItem.Id);

                _downloadStatusChanged.OnNext(downloadItem);
                TraceLog.Write($"[YTDLP] Status updated to Completed");
            }
        }
        catch (OperationCanceledException)
        {
            TraceLog.Write($"[YTDLP] OperationCanceledException for {downloadItem.Id}");
            // Download was paused or canceled - state already updated by Pause/Cancel methods
            return;
        }
        catch (Exception ex)
        {
            TraceLog.Write($"[YTDLP] EXCEPTION in ProcessDownloadAsync for {downloadItem.Id}");
            TraceLog.Write($"[YTDLP] Exception type: {ex.GetType().Name}");
            TraceLog.Write($"[YTDLP] Exception message: {ex.Message}");
            TraceLog.Write($"[YTDLP] Stack trace: {ex.StackTrace}");

            // Build detailed error message with context
            var errorMessage = BuildDetailedErrorMessage(ex, downloadItem);

            // Don't leave half-written media, thumbnails or subtitle files behind, but never
            // touch the output once yt-dlp finished it (a later post-processing error must
            // not delete a good download)
            if (!outputComplete)
            {
                DeletePartialOutputs(downloadItem);
                DeleteSidecarLeftovers(downloadItem);
            }

            downloadItem.Status = DownloadStatus.Failed;
            downloadItem.ErrorMessage = errorMessage;
            downloadItem.CanPause = false;
            downloadItem.CanResume = false;
            downloadItem.CanCancel = false;
            downloadItem.CanRestart = true;
            downloadItem.CanOpen = false;
            _downloadStatusChanged.OnNext(downloadItem);
            TraceLog.Write($"[YTDLP] Status updated to Failed");
        }
        finally
        {
            if (slotAcquired)
            {
                ReleaseSlot();
                TraceLog.Write($"[YTDLP] Download slot released for {downloadItem.Id}");
            }

            // Only remove OUR token: a quick Pause→Resume registers a new one before this
            // run finishes, and removing it made the next Pause unable to stop yt-dlp.
            if (_cancellationTokens.TryGetValue(downloadItem.Id, out var current)
                && current.Token == cancellationToken)
            {
                _cancellationTokens.TryRemove(
                    new KeyValuePair<string, CancellationTokenSource>(downloadItem.Id, current));
            }
        }
    }

    private async Task ProcessBurnInSubtitlesAsync(DownloadItem downloadItem, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(downloadItem.FilePath) || !File.Exists(downloadItem.FilePath))
            {
                TraceLog.Write($"[BURN-IN] Video file not found: {downloadItem.FilePath}");
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
                TraceLog.Write($"[BURN-IN] Subtitle file not found. Searched for:");
                foreach (var pattern in searchPatterns)
                {
                    TraceLog.Write($"[BURN-IN]   - {pattern}");
                }
                return;
            }

            TraceLog.Write($"[BURN-IN] Found subtitle file: {subtitlePath}");
            TraceLog.Write($"[BURN-IN] Starting subtitle burn-in process...");

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
                    TraceLog.Write($"[BURN-IN] Progress: {downloadItem.Progress:F0}%");
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
                TraceLog.Write($"[BURN-IN] Failed to burn in subtitles");
                downloadItem.ErrorMessage = "Failed to burn in subtitles, continuing with original video";
                _downloadStatusChanged.OnNext(downloadItem);

                // Clean up temp file if it exists
                if (File.Exists(tempOutputPath))
                {
                    File.Delete(tempOutputPath);
                }
                return;
            }

            TraceLog.Write($"[BURN-IN] Burn-in successful, replacing original video");

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

                TraceLog.Write($"[BURN-IN] Video replaced successfully: {originalFilePath}");
            }
            catch (Exception ex)
            {
                TraceLog.Write($"[BURN-IN] Error replacing file: {ex.Message}");

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
                    TraceLog.Write($"[BURN-IN] Cleaned up subtitle file: {subtitlePath}");
                }
            }
            catch (Exception ex)
            {
                TraceLog.Write($"[BURN-IN] Warning: Could not delete subtitle file: {ex.Message}");
                // Non-critical, continue
            }

            // Update download item with final file size
            if (File.Exists(downloadItem.FilePath))
            {
                var fileInfo = new FileInfo(downloadItem.FilePath);
                downloadItem.TotalBytes = fileInfo.Length;
                downloadItem.BytesDownloaded = fileInfo.Length;
                TraceLog.Write($"[BURN-IN] Updated file size: {fileInfo.Length} bytes");
            }

            downloadItem.Progress = 100;
            downloadItem.ErrorMessage = null;
            TraceLog.Write($"[BURN-IN] Subtitle burn-in process completed successfully");
        }
        catch (OperationCanceledException)
        {
            TraceLog.Write($"[BURN-IN] Burn-in process was canceled");
            throw;
        }
        catch (Exception ex)
        {
            TraceLog.Write($"[BURN-IN] Error during burn-in process: {ex.Message}");
            TraceLog.Write($"[BURN-IN] Stack trace: {ex.StackTrace}");
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

        // Try to parse strings like "10.5MiB", "1.2GiB", "500KiB", "~12.3MiB" (estimate).
        // yt-dlp always prints '.' decimals, so parse with the invariant culture (on de-DE
        // "10.50" used to parse as 1050).
        var trimmed = sizeString.Trim().TrimStart('~').Trim();
        var numberPart = new string(trimmed.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
        var unitPart = trimmed.Substring(numberPart.Length).Trim();

        if (!double.TryParse(numberPart, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double value))
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

    internal static string BuildFormatString(FormatProfile profile)
    {
        // Handle audio-only formats (MP3 is always audio-only). Sites such as TikTok and
        // Twitter only offer combined streams, so fall back to "best" and extract audio.
        if (profile.Quality == "audio-only" || profile.Container == "mp3")
        {
            return profile.Container switch
            {
                "m4a" => "bestaudio[ext=m4a]/bestaudio/best",
                "webm" => "bestaudio[ext=webm]/bestaudio/best",
                _ => "bestaudio/best"
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

        // Preferred container first, then any streams at the requested quality, then
        // anything at all, so sites without that container (Vimeo, TikTok, Bilibili) still
        // download; --merge-output-format keeps the requested container where possible.
        var fallback = $"bestvideo{qualityFilter}+bestaudio/best{qualityFilter}/best";

        // For MP4 prefer H.264 (avc1): YouTube's mp4 video is often AV1 and TikTok's HEVC,
        // which Windows can't play without extra codec packs and many editors can't open.
        return profile.Container switch
        {
            "webm" => $"bestvideo{qualityFilter}[ext=webm]+bestaudio[ext=webm]/best{qualityFilter}[ext=webm]/{fallback}",
            _ => $"bestvideo{qualityFilter}[ext=mp4][vcodec^=avc1]+bestaudio[ext=m4a]"
                + $"/best{qualityFilter}[ext=mp4][vcodec^=avc1]"
                + $"/bestvideo{qualityFilter}[ext=mp4]+bestaudio[ext=m4a]/best{qualityFilter}[ext=mp4]/{fallback}"
        };
    }

    private static bool IsAudioOnly(FormatProfile profile)
    {
        return profile.Quality == "audio-only" || profile.Container == "mp3";
    }

    private static readonly HashSet<string> MediaExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".webm", ".mkv", ".mov", ".m4a", ".mp3", ".opus", ".ogg", ".aac", ".flac", ".wav",
    };

    /// <summary>
    /// Files this download produced under a different name than expected (e.g. "X.mkv" or
    /// "X.mp4.webm"), largest first. Only "base." prefixes with a media extension that were
    /// written after the download started qualify, so an older "Intro to Python.mp4" can
    /// never be picked up (and renamed) for a download named "Intro".
    /// </summary>
    internal static List<string> FindDownloadedFileCandidates(string directory, string baseFileName, DateTime startedUtc)
    {
        if (string.IsNullOrEmpty(baseFileName) || !Directory.Exists(directory))
            return [];

        var notBefore = startedUtc.AddSeconds(-5);
        return Directory
            .EnumerateFiles(directory)
            .Where(f =>
            {
                var name = Path.GetFileName(f);
                return name.StartsWith(baseFileName + ".", StringComparison.OrdinalIgnoreCase)
                    && MediaExtensions.Contains(Path.GetExtension(f))
                    && File.GetLastWriteTimeUtc(f) >= notBefore;
            })
            .OrderByDescending(f => new FileInfo(f).Length)
            .ToList();
    }

    /// <summary>
    /// Removes files a stopped (paused/canceled/deleted) run left behind: the cut-off output
    /// and yt-dlp's per-stream fragments ("X.f137.mp4"). Same safety rules as above.
    /// </summary>
    private static void DeletePartialOutputs(DownloadItem downloadItem)
    {
        if (string.IsNullOrEmpty(downloadItem.FilePath))
            return;

        try
        {
            var directory = Path.GetDirectoryName(downloadItem.FilePath) ?? string.Empty;
            var baseFileName = Path.GetFileNameWithoutExtension(downloadItem.FilePath);
            var candidates = FindDownloadedFileCandidates(
                directory, baseFileName, downloadItem.StartedAt ?? downloadItem.CreatedAt);

            foreach (var file in candidates.Append(downloadItem.FilePath).Distinct())
            {
                if (File.Exists(file) && File.GetLastWriteTimeUtc(file) >= (downloadItem.StartedAt ?? downloadItem.CreatedAt).AddSeconds(-5))
                    File.Delete(file);
            }
        }
        catch (Exception ex)
        {
            TraceLog.Write($"[YTDLP] Could not clean partial files: {ex.Message}");
        }
    }

    private static readonly HashSet<string> SidecarExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".srt", ".vtt", ".ass", ".part", ".ytdl",
    };

    /// <summary>
    /// After a failed download, removes the thumbnail/subtitle/.part files yt-dlp wrote for it
    /// ("X.jpg", "X.en.srt"). Only "base." names written during this download qualify.
    /// </summary>
    private static void DeleteSidecarLeftovers(DownloadItem downloadItem)
    {
        if (string.IsNullOrEmpty(downloadItem.FilePath))
            return;

        try
        {
            var directory = Path.GetDirectoryName(downloadItem.FilePath) ?? string.Empty;
            if (!Directory.Exists(directory))
                return;

            var baseFileName = Path.GetFileNameWithoutExtension(downloadItem.FilePath);
            var notBefore = (downloadItem.StartedAt ?? downloadItem.CreatedAt).AddSeconds(-5);
            foreach (var file in Directory.EnumerateFiles(directory))
            {
                if (Path.GetFileName(file).StartsWith(baseFileName + ".", StringComparison.OrdinalIgnoreCase)
                    && SidecarExtensions.Contains(Path.GetExtension(file))
                    && File.GetLastWriteTimeUtc(file) >= notBefore)
                {
                    File.Delete(file);
                }
            }
        }
        catch (Exception ex)
        {
            TraceLog.Write($"[YTDLP] Could not clean leftover files: {ex.Message}");
        }
    }

    /// <summary>Maps the MP3 Quality setting index to yt-dlp's --audio-quality value.</summary>
    internal static string GetAudioQualityArgument(int index) =>
        index switch
        {
            1 => "320K",
            2 => "192K",
            3 => "128K",
            _ => "0",
        };

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

    private static void ApplyCompatibilityOptions(OptionSet options)
    {
        // YouTube requires a JavaScript runtime to solve the EJS player challenges that
        // produce signed media URLs. Without one, yt-dlp falls back to degraded clients
        // and every download fails with HTTP 403 Forbidden. We ship QuickJS next to the
        // executable; yt-dlp still auto-detects and prefers a system Deno/Node/Bun.
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var quickJsPath = Path.Combine(baseDir, OperatingSystem.IsWindows() ? "qjs.exe" : "qjs");

        if (File.Exists(quickJsPath))
        {
            options.AddCustomOption("--js-runtimes", $"quickjs:{quickJsPath}");
            TraceLog.Write($"[YTDLP] Bundled QuickJS JS runtime registered: {quickJsPath}");
        }
        else
        {
            // Not fatal: a system runtime may still be on PATH, but warn loudly because
            // this is the single most common cause of "every download fails".
            TraceLog.Write(
                "[YTDLP] WARNING: bundled QuickJS not found. Downloads will fail with "
                    + "HTTP 403 unless Deno, Node or Bun is installed and on PATH."
            );
        }

        options.AddCustomOption("--retries", "10");
        options.AddCustomOption("--fragment-retries", "10");
    }

    private static string BuildDetailedErrorMessage(Exception ex, DownloadItem downloadItem)
    {
        var errorType = ex.GetType().Name;
        var baseMessage = ex.Message;

        // Authentication problems first: their raw text mentions "network"/"unavailable"
        // and would otherwise be mislabeled. Keep the raw text so the UI can classify it.
        switch (YtDlpErrorClassifier.Classify(baseMessage))
        {
            case YtDlpErrorKind.CookieReadFailed:
                return "Could not read your browser's cookies (Chrome/Edge lock them on Windows). "
                    + "Use a cookies.txt file or Firefox in Settings > Advanced > Authentication. "
                    + $"Details: {baseMessage}";
            case YtDlpErrorKind.SignInRequired:
                return "Sign-in required: the site blocked this download until you're signed in. "
                    + "Add your cookies in Settings > Advanced > Authentication, then click Restart. "
                    + $"Details: {baseMessage}";
        }

        if (baseMessage.Contains("DRM protected", StringComparison.OrdinalIgnoreCase))
        {
            return "This video is DRM-protected and cannot be downloaded.";
        }

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

    /// <summary>
    /// Fetches title/channel/thumbnail with yt-dlp (same cookies and JS runtime as downloads).
    /// Used when YoutubeExplode is blocked (YouTube bot check) and for non-YouTube sites,
    /// which previously showed "YouTube video" / "Video from TikTok" placeholders.
    /// </summary>
    public async Task<IVideo?> TryFetchVideoInfoAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(45));

            var options = new OptionSet { NoPlaylist = true };
            ApplyCompatibilityOptions(options);
            ApplyCookieOptions(options);

            var result = await _ytdl.RunVideoDataFetch(url, timeout.Token, flat: false, overrideOptions: options);
            if (!result.Success
                && YtDlpErrorClassifier.Classify(string.Join(", ", result.ErrorOutput ?? [])) == YtDlpErrorKind.CookieReadFailed
                && RemoveBrowserCookieOption(options))
            {
                result = await _ytdl.RunVideoDataFetch(url, timeout.Token, flat: false, overrideOptions: options);
            }
            if (!result.Success || result.Data == null || string.IsNullOrWhiteSpace(result.Data.Title))
            {
                TraceLog.Write($"[YTDLP] Metadata fetch failed: {string.Join(", ", result.ErrorOutput ?? [])}");
                return null;
            }

            var data = result.Data;
            var author = FirstNonEmpty(data.Uploader, data.Channel, data.Extractor);
            TimeSpan? duration = data.Duration is > 0 ? TimeSpan.FromSeconds(data.Duration.Value) : null;
            return new Models.GenericVideo(url, data.Title, data.ID, author, duration, data.Thumbnail);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            TraceLog.Write($"[YTDLP] Metadata fetch error: {ex.Message}");
            return null;
        }
    }

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

    /// <summary>
    /// Reads a property from the Desktop settings object (Core cannot reference Desktop).
    /// Returns <paramref name="fallback"/> when the property is missing or the wrong type.
    /// </summary>
    private T ReadSetting<T>(string name, T fallback)
    {
        try
        {
            var value = _settingsService.GetType().GetProperty(name)?.GetValue(_settingsService);
            return value is T typed ? typed : fallback;
        }
        catch (Exception ex)
        {
            TraceLog.Write($"[YTDLP] Could not read setting {name}: {ex.Message}");
            return fallback;
        }
    }

    internal int GetParallelLimit() =>
        Math.Clamp(ReadSetting("ParallelLimit", _defaultParallelLimit), 1, 10);

    /// <summary>
    /// Waits for a free download slot. The limit is re-read on every check so changing
    /// "Parallel downloads" in Settings applies to queued items without a restart.
    /// </summary>
    private async Task AcquireSlotAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            lock (_slotLock)
            {
                if (_activeDownloads < GetParallelLimit())
                {
                    _activeDownloads++;
                    return;
                }
            }

            await Task.Delay(250, cancellationToken);
        }
    }

    private void ReleaseSlot()
    {
        lock (_slotLock)
        {
            _activeDownloads = Math.Max(0, _activeDownloads - 1);
        }
    }

    /// <summary>Removes --cookies-from-browser if present. Returns true when something was removed.</summary>
    internal static bool RemoveBrowserCookieOption(OptionSet options)
    {
        if (!options.ToString().Contains("--cookies-from-browser", StringComparison.Ordinal))
            return false;

        options.DeleteCustomOption("--cookies-from-browser");
        return !options.ToString().Contains("--cookies-from-browser", StringComparison.Ordinal);
    }

    private void ApplyCookieOptions(OptionSet options)
    {
        var cookiesFile = ReadSetting("CookiesFilePath", string.Empty);
        if (!string.IsNullOrWhiteSpace(cookiesFile))
        {
            if (File.Exists(cookiesFile))
            {
                options.AddCustomOption("--cookies", cookiesFile);
                TraceLog.Write("[YTDLP] Using cookies.txt file for authentication");
                return;
            }

            TraceLog.Write($"[YTDLP] Warning: cookies file not found, ignoring: {cookiesFile}");
        }

        var useBrowser = ReadSetting("UseBrowserCookies", false);
        var browser = ReadSetting("BrowserForCookies", string.Empty);
        if (useBrowser && !string.IsNullOrWhiteSpace(browser))
        {
            options.AddCustomOption("--cookies-from-browser", browser);
            TraceLog.Write($"[YTDLP] Using browser cookies from: {browser}");
        }
    }

    private bool IsAiSubtitleFallbackActive(FormatProfile profile, bool isAudioOnly) =>
        profile.IncludeSubtitles
        && !isAudioOnly
        && _subtitleGenerationService != null
        && ReadSetting("EnableAISubtitles", false);

    /// <summary>
    /// Finds the subtitle file yt-dlp wrote next to the video (any language), if any.
    /// </summary>
    internal static string? FindSubtitleFile(string videoPath)
    {
        var directory = Path.GetDirectoryName(videoPath);
        if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            return null;

        var nameWithoutExt = Path.GetFileNameWithoutExtension(videoPath);
        var exact = Path.Combine(directory, $"{nameWithoutExt}.en.srt");
        if (File.Exists(exact))
            return exact;

        // Match by prefix in code rather than a glob: titles can contain glob characters
        return Directory
            .EnumerateFiles(directory, "*.srt")
            .Where(f => Path.GetFileName(f).StartsWith(nameWithoutExt + ".", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(f => new FileInfo(f).Length)
            .FirstOrDefault();
    }

    private async Task ProcessAiSubtitlesAsync(
        DownloadItem downloadItem,
        FormatProfile profile,
        CancellationToken cancellationToken)
    {
        var videoPath = downloadItem.FilePath!;
        try
        {
            var subtitlePath = FindSubtitleFile(videoPath);

            if (subtitlePath == null)
            {
                var language = ReadSetting("AISubtitleLanguage", "auto");
                subtitlePath = Path.Combine(
                    Path.GetDirectoryName(videoPath) ?? string.Empty,
                    $"{Path.GetFileNameWithoutExtension(videoPath)}.srt");

                TraceLog.Write($"[AI-SUBS] No subtitles from source, generating with Whisper ({language})");
                downloadItem.ErrorMessage = "Generating AI subtitles...";
                _downloadStatusChanged.OnNext(downloadItem);

                var progress = new Progress<double>(p =>
                {
                    downloadItem.ErrorMessage = $"Generating AI subtitles... {p * 100:F0}%";
                    _downloadStatusChanged.OnNext(downloadItem);
                });

                var generated = await _subtitleGenerationService!.GenerateSubtitlesAsync(
                    videoPath, subtitlePath, language, progress, cancellationToken);

                if (!generated || !File.Exists(subtitlePath))
                {
                    TraceLog.Write("[AI-SUBS] Generation failed, keeping video without subtitles");
                    downloadItem.ErrorMessage = null;
                    return;
                }
            }

            // yt-dlp embedding was disabled for this download, so embed ourselves when asked
            if (profile.SubtitleStyle == SubtitleStyle.Embedded)
            {
                downloadItem.ErrorMessage = "Embedding subtitles...";
                _downloadStatusChanged.OnNext(downloadItem);

                if (await _subtitleBurnInService.EmbedSubtitlesAsync(videoPath, subtitlePath, cancellationToken))
                {
                    TryDelete(subtitlePath);
                }
            }

            downloadItem.ErrorMessage = null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Subtitles are best-effort; never fail a finished download because of them
            TraceLog.Write($"[AI-SUBS] Error: {ex.Message}");
            downloadItem.ErrorMessage = null;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception ex)
        {
            TraceLog.Write($"[YTDLP] Could not delete {path}: {ex.Message}");
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

        _downloadStatusChanged?.Dispose();
        _overallProgress?.Dispose();
        _stateRepository?.Dispose();
    }
}
