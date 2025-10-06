using System.Globalization;
using System.Text;
using EnhancedYoutubeDownloader.Shared.Interfaces;
using Whisper.net;
using Whisper.net.Ggml;

namespace EnhancedYoutubeDownloader.Core.Services;

/// <summary>
/// Service for generating subtitles using OpenAI Whisper via Whisper.net
/// </summary>
public class SubtitleGenerationService : ISubtitleGenerationService, IDisposable
{
    private const string ModelName = "ggml-base.bin"; // 74MB base model
    private readonly string _modelDirectory;
    private WhisperFactory? _whisperFactory;
    private bool _disposed;

    public SubtitleGenerationService()
    {
        // Store models in AppData
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _modelDirectory = Path.Combine(appDataPath, "EnhancedYoutubeDownloader", "WhisperModels");
        Directory.CreateDirectory(_modelDirectory);
    }

    public async Task<bool> GenerateSubtitlesAsync(
        string videoFilePath,
        string outputSubtitlePath,
        string language = "auto",
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(videoFilePath))
            {
                throw new FileNotFoundException($"Video file not found: {videoFilePath}");
            }

            // Ensure model is downloaded
            progress?.Report(0.05);
            if (!await IsModelAvailableAsync())
            {
                await DownloadModelAsync(new Progress<double>(p => progress?.Report(p * 0.2)), cancellationToken);
            }

            progress?.Report(0.2);

            // Initialize Whisper factory if not already done
            var modelPath = Path.Combine(_modelDirectory, ModelName);
            _whisperFactory ??= WhisperFactory.FromPath(modelPath);

            progress?.Report(0.25);

            // Create processor with language settings
            using var processor = _whisperFactory.CreateBuilder()
                .WithLanguage(language == "auto" ? "en" : language) // Whisper.net auto-detect not directly supported, default to English
                .Build();

            progress?.Report(0.3);

            var subtitles = new List<SubtitleSegment>();
            var totalSegments = 0;
            var processedSegments = 0;

            // Process audio from video file
            await using var fileStream = File.OpenRead(videoFilePath);
            await foreach (var segment in processor.ProcessAsync(fileStream, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                subtitles.Add(new SubtitleSegment
                {
                    Index = ++totalSegments,
                    StartTime = segment.Start,
                    EndTime = segment.End,
                    Text = segment.Text.Trim()
                });

                processedSegments++;

                // Report progress (30% to 90% during processing)
                var processingProgress = 0.3 + (processedSegments * 0.6 / Math.Max(totalSegments, 1));
                progress?.Report(Math.Min(processingProgress, 0.9));
            }

            progress?.Report(0.9);

            // Write SRT file
            await WriteSrtFileAsync(outputSubtitlePath, subtitles, cancellationToken);

            progress?.Report(1.0);
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to generate subtitles: {ex.Message}");
            return false;
        }
    }

    public Task<bool> IsModelAvailableAsync()
    {
        var modelPath = Path.Combine(_modelDirectory, ModelName);
        return Task.FromResult(File.Exists(modelPath));
    }

    public async Task DownloadModelAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var modelPath = Path.Combine(_modelDirectory, ModelName);

            if (File.Exists(modelPath))
            {
                progress?.Report(1.0);
                return;
            }

            progress?.Report(0.0);

            // Download from Hugging Face (official Whisper.net models)
            var modelUrl = $"https://huggingface.co/ggerganov/whisper.cpp/resolve/main/{ModelName}";

            using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
            using var response = await httpClient.GetAsync(modelUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            var downloadedBytes = 0L;

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

                downloadedBytes += bytesRead;

                if (totalBytes > 0)
                {
                    var downloadProgress = (double)downloadedBytes / totalBytes;
                    progress?.Report(downloadProgress);
                }
            }

            progress?.Report(1.0);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to download Whisper model: {ex.Message}");
            throw;
        }
    }

    private static async Task WriteSrtFileAsync(string outputPath, List<SubtitleSegment> segments, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();

        foreach (var segment in segments)
        {
            sb.AppendLine(segment.Index.ToString());
            sb.AppendLine($"{FormatSrtTime(segment.StartTime)} --> {FormatSrtTime(segment.EndTime)}");
            sb.AppendLine(segment.Text);
            sb.AppendLine();
        }

        await File.WriteAllTextAsync(outputPath, sb.ToString(), Encoding.UTF8, cancellationToken);
    }

    private static string FormatSrtTime(TimeSpan time)
    {
        // SRT format: HH:MM:SS,mmm
        return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2},{time.Milliseconds:D3}";
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _whisperFactory?.Dispose();
        _whisperFactory = null;
        _disposed = true;
    }

    private class SubtitleSegment
    {
        public int Index { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
