using System.Collections.Concurrent;
using System.Reactive.Subjects;
using EnhancedYoutubeDownloader.Core.Utils;
using EnhancedYoutubeDownloader.Shared.Interfaces;
using EnhancedYoutubeDownloader.Shared.Models;
using Gress;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos.ClosedCaptions;

namespace EnhancedYoutubeDownloader.Core.Services;

public class DownloadService : IDownloadService, IDisposable
{
    private readonly ConcurrentDictionary<string, DownloadItem> _downloads = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cancellationTokens = new();
    private readonly Subject<DownloadItem> _downloadStatusChanged = new();
    private readonly Subject<double> _overallProgress = new();
    private readonly YoutubeClient _youtubeClient;
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _downloadSemaphore;
    private readonly DownloadStateRepository _stateRepository;

    public DownloadService(int maxConcurrentDownloads = 3)
    {
        // Use custom HttpClient with User-Agent header to avoid 403 errors
        _youtubeClient = new YoutubeClient(Http.Client);
        _httpClient = Http.Client;
        _downloadSemaphore = new SemaphoreSlim(maxConcurrentDownloads, maxConcurrentDownloads);
        _stateRepository = new DownloadStateRepository();
    }

    public IObservable<DownloadItem> DownloadStatusChanged => _downloadStatusChanged;
    public IObservable<double> OverallProgress => _overallProgress;

    public Task<DownloadItem> CreateDownloadAsync(IVideo video, string filePath, FormatProfile? profile = null)
    {
        var downloadItem = new DownloadItem
        {
            Video = video,
            FilePath = filePath,
            Status = DownloadStatus.Queued,
            PartialFilePath = filePath + ".part"
        };

        _downloads[downloadItem.Id] = downloadItem;
        _downloadStatusChanged.OnNext(downloadItem);

        return Task.FromResult(downloadItem);
    }

    public Task StartDownloadAsync(DownloadItem downloadItem)
    {
        if (downloadItem.Status != DownloadStatus.Queued && downloadItem.Status != DownloadStatus.Paused)
            return Task.CompletedTask;

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

    public async Task PauseDownloadAsync(DownloadItem downloadItem)
    {
        if (downloadItem.Status != DownloadStatus.Started)
            return;

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

        // Save state to repository
        await _stateRepository.SaveStateAsync(downloadItem);

        _downloadStatusChanged.OnNext(downloadItem);
    }

    public async Task ResumeDownloadAsync(DownloadItem downloadItem)
    {
        if (downloadItem.Status != DownloadStatus.Paused)
            return;

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
                }
                else
                {
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

    public async Task CancelDownloadAsync(DownloadItem downloadItem)
    {
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
        Console.WriteLine($"[DOWNLOAD] ProcessDownloadAsync started for {downloadItem.Id}");
        await _downloadSemaphore.WaitAsync(cancellationToken);
        Console.WriteLine($"[DOWNLOAD] Semaphore acquired for {downloadItem.Id}");

        try
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine($"[DOWNLOAD] Cancellation requested before start for {downloadItem.Id}");
                return;
            }

            // Get stream manifest
            Console.WriteLine($"[DOWNLOAD] Getting stream manifest for video: {downloadItem.Video!.Id}");
            var manifest = await _youtubeClient.Videos.Streams.GetManifestAsync(
                downloadItem.Video!.Id,
                cancellationToken
            );
            Console.WriteLine($"[DOWNLOAD] Manifest retrieved. Muxed streams: {manifest.GetMuxedStreams().Count()}");

            // Select best muxed stream (video + audio combined)
            var streamInfo = manifest
                .GetMuxedStreams()
                .OrderByDescending(s => s.VideoQuality)
                .FirstOrDefault();

            // If no muxed stream, we need to download video and audio separately
            // For simplicity in this implementation, we'll use YoutubeExplode.Converter
            // which handles this automatically with FFmpeg
            if (streamInfo == null)
            {
                Console.WriteLine($"[DOWNLOAD] No muxed stream found, using converter method");
                await DownloadWithConverterAsync(downloadItem, manifest, cancellationToken);
            }
            else
            {
                Console.WriteLine($"[DOWNLOAD] Using muxed stream: {streamInfo.VideoQuality} - {streamInfo.Container}");
                await DownloadChunkedAsync(downloadItem, streamInfo, cancellationToken);
            }

            // Download completed successfully
            if (!cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine($"[DOWNLOAD] Download completed, moving file from .part");
                // Move .part file to final location
                if (!string.IsNullOrEmpty(downloadItem.PartialFilePath) &&
                    File.Exists(downloadItem.PartialFilePath))
                {
                    if (File.Exists(downloadItem.FilePath))
                    {
                        File.Delete(downloadItem.FilePath);
                    }
                    File.Move(downloadItem.PartialFilePath, downloadItem.FilePath!);
                    Console.WriteLine($"[DOWNLOAD] File moved to: {downloadItem.FilePath}");
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
                Console.WriteLine($"[DOWNLOAD] Status updated to Completed");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[DOWNLOAD] OperationCanceledException for {downloadItem.Id}");
            // Download was paused or canceled - state already updated by Pause/Cancel methods
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DOWNLOAD] EXCEPTION in ProcessDownloadAsync for {downloadItem.Id}");
            Console.WriteLine($"[DOWNLOAD] Exception type: {ex.GetType().Name}");
            Console.WriteLine($"[DOWNLOAD] Exception message: {ex.Message}");
            Console.WriteLine($"[DOWNLOAD] Stack trace: {ex.StackTrace}");

            downloadItem.Status = DownloadStatus.Failed;
            downloadItem.ErrorMessage = ex.Message;
            downloadItem.CanPause = false;
            downloadItem.CanResume = false;
            downloadItem.CanCancel = false;
            downloadItem.CanRestart = true;
            _downloadStatusChanged.OnNext(downloadItem);
            Console.WriteLine($"[DOWNLOAD] Status updated to Failed");
        }
        finally
        {
            _downloadSemaphore.Release();
            Console.WriteLine($"[DOWNLOAD] Semaphore released for {downloadItem.Id}");
            _cancellationTokens.TryRemove(downloadItem.Id, out _);
        }
    }

    private async Task DownloadChunkedAsync(
        DownloadItem downloadItem,
        IStreamInfo streamInfo,
        CancellationToken cancellationToken
    )
    {
        Console.WriteLine($"[CHUNKED] Starting chunked download for {downloadItem.Id}");
        var partialFilePath = downloadItem.PartialFilePath!;
        var startByte = downloadItem.BytesDownloaded;
        downloadItem.TotalBytes = streamInfo.Size.Bytes;
        Console.WriteLine($"[CHUNKED] File: {partialFilePath}, Start byte: {startByte}, Total bytes: {downloadItem.TotalBytes}");

        // Create request with Range header for resume support
        var request = new HttpRequestMessage(HttpMethod.Get, streamInfo.Url);
        if (startByte > 0)
        {
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(startByte, null);
            Console.WriteLine($"[CHUNKED] Resuming from byte {startByte}");
        }

        Console.WriteLine($"[CHUNKED] Sending HTTP request to {streamInfo.Url}");
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );
        Console.WriteLine($"[CHUNKED] HTTP response: {response.StatusCode}");
        response.EnsureSuccessStatusCode();

        // Ensure directory exists
        var directory = Path.GetDirectoryName(partialFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
            Console.WriteLine($"[CHUNKED] Directory ensured: {directory}");
        }

        // Open file in append mode if resuming, otherwise create new
        Console.WriteLine($"[CHUNKED] Opening file stream");
        using var fileStream = new FileStream(
            partialFilePath,
            startByte > 0 ? FileMode.Append : FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            8192,
            true
        );

        using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        Console.WriteLine($"[CHUNKED] Starting download loop");

        var buffer = new byte[8192];
        int bytesRead;
        var totalRead = 0L;

        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            totalRead += bytesRead;

            downloadItem.BytesDownloaded += bytesRead;
            downloadItem.Progress = downloadItem.BytesProgress;

            // Save state periodically (every 1MB)
            if (downloadItem.BytesDownloaded % (1024 * 1024) < 8192)
            {
                await _stateRepository.SaveStateAsync(downloadItem);
            }

            _downloadStatusChanged.OnNext(downloadItem);
        }

        Console.WriteLine($"[CHUNKED] Download loop completed. Total bytes read: {totalRead}");
        // Final state save
        await _stateRepository.SaveStateAsync(downloadItem);
        Console.WriteLine($"[CHUNKED] Final state saved");
    }

    private async Task DownloadWithConverterAsync(
        DownloadItem downloadItem,
        StreamManifest manifest,
        CancellationToken cancellationToken
    )
    {
        Console.WriteLine($"[CONVERTER] Starting converter download for {downloadItem.Id}");
        // For streams that need conversion (separate video/audio), use YoutubeExplode.Converter
        // Note: This doesn't support pause/resume natively, but we can still cancel it
        var videoStream = manifest.GetVideoOnlyStreams().GetWithHighestVideoQuality();
        var audioStream = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();
        Console.WriteLine($"[CONVERTER] Video stream: {videoStream?.VideoQuality}, Audio stream bitrate: {audioStream?.Bitrate}");

        if (videoStream == null || audioStream == null)
        {
            Console.WriteLine($"[CONVERTER] ERROR: No suitable streams found");
            throw new InvalidOperationException("No suitable streams found");
        }

        var streamInfos = new List<IStreamInfo> { videoStream, audioStream };
        downloadItem.TotalBytes = videoStream.Size.Bytes + audioStream.Size.Bytes;
        Console.WriteLine($"[CONVERTER] Total bytes: {downloadItem.TotalBytes}");

        var progressHandler = new Progress<double>(p =>
        {
            downloadItem.Progress = p * 100;
            downloadItem.BytesDownloaded = (long)(downloadItem.TotalBytes * p);
            _downloadStatusChanged.OnNext(downloadItem);
            if (p % 0.1 < 0.01) // Log every 10%
                Console.WriteLine($"[CONVERTER] Progress: {p * 100:F1}%");
        });

        // Download directly to partial file path using converter
        Console.WriteLine($"[CONVERTER] Output file: {downloadItem.PartialFilePath}");
        var conversionRequest = new ConversionRequestBuilder(downloadItem.PartialFilePath!)
            .Build();

        Console.WriteLine($"[CONVERTER] Starting YoutubeExplode.Converter download (requires FFmpeg)");
        await _youtubeClient.Videos.DownloadAsync(
            streamInfos,
            conversionRequest,
            progressHandler,
            cancellationToken
        );
        Console.WriteLine($"[CONVERTER] Download completed successfully");
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
        _httpClient?.Dispose();
    }
}
