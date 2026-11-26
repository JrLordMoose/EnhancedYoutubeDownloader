// Download health check for Enhanced YouTube Downloader.
// Drives the app's REAL download service (YtDlpDownloadService) against the platforms in
// platforms.json, downloading MP4 and MP3, and verifies every file with ffmpeg.
// Writes results.json (machine-readable, used for month-to-month trends) and results.md.
// Exit code: 0 = every "pass" platform passed, 1 = at least one regression, 2 = bad arguments.
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using EnhancedYoutubeDownloader.Core.Models;
using EnhancedYoutubeDownloader.Core.Services;
using EnhancedYoutubeDownloader.Shared.Interfaces;
using EnhancedYoutubeDownloader.Shared.Models;

var outDir = GetArg("--out") ?? Path.Combine(Path.GetTempPath(), "eyd-healthcheck-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"));
var cookies = GetArg("--cookies") ?? string.Empty;
var only = GetArg("--only");
var timeout = TimeSpan.FromMinutes(int.TryParse(GetArg("--timeout-minutes"), out var t) ? t : 5);
var baseDir = AppContext.BaseDirectory;
var ffmpeg = Path.Combine(baseDir, "ffmpeg.exe");
var ytdlp = Path.Combine(baseDir, "yt-dlp.exe");

if (!File.Exists(ffmpeg) || !File.Exists(ytdlp))
{
    Console.Error.WriteLine("yt-dlp.exe / ffmpeg.exe missing next to the harness. Build from the repo so they are copied.");
    return 2;
}

if (cookies.Length > 0 && !File.Exists(cookies))
{
    Console.Error.WriteLine("Cookies file not found; continuing without cookies.");
    cookies = string.Empty;
}

Directory.CreateDirectory(outDir);
var config = JsonSerializer.Deserialize<PlatformFile>(
    File.ReadAllText(Path.Combine(baseDir, "platforms.json")),
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

var settings = new HarnessSettings { CookiesFilePath = cookies };
using var service = new YtDlpDownloadService(new SubtitleBurnInService(), settings, 3);
var resolver = new QueryResolver(new NoCache());
var results = new List<CheckResult>();

Console.WriteLine($"yt-dlp {RunTool(ytdlp, "--version").Trim()} | cookies: {(cookies.Length > 0 ? "yes" : "no")} | out: {outDir}");

foreach (var platform in config.Platforms.Where(p => only is null || p.Id == only))
{
    var info = await service.TryFetchVideoInfoAsync(platform.Url);
    foreach (var (format, enabled) in new[] { ("mp4", platform.Mp4), ("mp3", platform.Mp3) })
    {
        if (!enabled)
            continue;

        var sw = Stopwatch.StartNew();
        var folder = Path.Combine(outDir, platform.Id, format);
        Directory.CreateDirectory(folder);
        var profile = format == "mp4"
            ? new FormatProfile { Quality = "720p", Container = "mp4", IncludeSubtitles = false }
            : new FormatProfile { Quality = "audio-only", Container = "mp3", IncludeSubtitles = false };

        var item = await service.CreateDownloadAsync(
            info ?? new GenericVideo(platform.Url, platform.Name),
            Path.Combine(folder, $"{platform.Id}.{format}"),
            profile,
            resolver.DetectPlatform(platform.Url));
        await service.StartDownloadAsync(item);

        while (item.Status is DownloadStatus.Queued or DownloadStatus.Started && sw.Elapsed < timeout)
            await Task.Delay(500);

        var result = new CheckResult
        {
            Platform = platform.Id,
            Name = platform.Name,
            Format = format,
            Expected = platform.Expected,
            Title = info?.Title,
            Status = item.Status == DownloadStatus.Started ? "Timeout" : item.Status.ToString(),
            Error = Truncate(item.ErrorMessage, 220),
            Seconds = Math.Round(sw.Elapsed.TotalSeconds, 1),
        };

        if (item.Status == DownloadStatus.Started)
            await service.CancelDownloadAsync(item);

        if (item.Status == DownloadStatus.Completed && File.Exists(item.FilePath))
        {
            var probe = RunTool(ffmpeg, $"-hide_banner -i \"{item.FilePath}\"");
            result.SizeKb = new FileInfo(item.FilePath).Length / 1024;
            result.Duration = Regex.Match(probe, @"Duration: ([\d:.]+)").Groups[1].Value;
            result.VideoCodec = Regex.Match(probe, @"Video: (\w+)(?![^\n]*attached pic)").Groups[1].Value;
            result.AudioCodec = Regex.Match(probe, @"Audio: (\w+)").Groups[1].Value;
            result.Verified = format == "mp4"
                ? result.VideoCodec.Length > 0 && result.AudioCodec.Length > 0
                : result.AudioCodec == "mp3";
        }

        result.Outcome = result.Verified ? "PASS" : result.Expected == "site-limited" ? "SITE-LIMITED" : "FAIL";
        results.Add(result);
        Console.WriteLine($"{result.Outcome,-12} {platform.Name,-18} {format}  {result.Status,-9} {result.VideoCodec}/{result.AudioCodec} {result.Seconds}s {result.Error}");
    }
}

var summary = new
{
    RanAt = DateTimeOffset.Now,
    YtDlpVersion = RunTool(ytdlp, "--version").Trim(),
    UsedCookies = cookies.Length > 0,
    Passed = results.Count(r => r.Outcome == "PASS"),
    Failed = results.Count(r => r.Outcome == "FAIL"),
    SiteLimited = results.Count(r => r.Outcome == "SITE-LIMITED"),
    Results = results,
};

File.WriteAllText(Path.Combine(outDir, "results.json"), JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true }));

var md = new StringBuilder()
    .AppendLine($"**{summary.Passed} passed · {summary.Failed} failed · {summary.SiteLimited} site-limited** (yt-dlp {summary.YtDlpVersion}, cookies: {(summary.UsedCookies ? "yes" : "no")})")
    .AppendLine()
    .AppendLine("| Result | Platform | Format | Codecs | Duration | Size | Time | Note |")
    .AppendLine("|---|---|---|---|---|---|---|---|");
foreach (var r in results)
{
    md.AppendLine($"| {Icon(r.Outcome)} {r.Outcome} | {r.Name} | {r.Format.ToUpperInvariant()} | {r.VideoCodec}{(r.VideoCodec.Length > 0 ? " + " : "")}{r.AudioCodec} | {r.Duration} | {(r.SizeKb > 0 ? $"{r.SizeKb:N0} KB" : "")} | {r.Seconds}s | {(r.Error ?? "").Replace("|", "/")} |");
}

File.WriteAllText(Path.Combine(outDir, "results.md"), md.ToString());
Console.WriteLine($"Wrote {Path.Combine(outDir, "results.md")}");

// Downloaded media is only evidence: keep the reports, drop the large files.
foreach (var file in Directory.EnumerateFiles(outDir, "*", SearchOption.AllDirectories))
{
    if (!file.EndsWith("results.json", StringComparison.Ordinal) && !file.EndsWith("results.md", StringComparison.Ordinal))
        File.Delete(file);
}

return summary.Failed > 0 ? 1 : 0;

string? GetArg(string name)
{
    var i = Array.IndexOf(args, name);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}

static string Icon(string outcome) => outcome switch { "PASS" => "✅", "FAIL" => "❌", _ => "⚠️" };

static string? Truncate(string? s, int max) => s is null || s.Length <= max ? s : s[..max] + "…";

static string RunTool(string exe, string arguments)
{
    var psi = new ProcessStartInfo(exe, arguments)
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
    };
    using var p = Process.Start(psi)!;
    var output = p.StandardOutput.ReadToEndAsync();
    var error = p.StandardError.ReadToEndAsync();
    p.WaitForExit(60_000);
    return output.Result + error.Result;
}

internal sealed class PlatformFile
{
    public List<Platform> Platforms { get; set; } = [];
}

internal sealed class Platform
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";
    public bool Mp4 { get; set; }
    public bool Mp3 { get; set; }
    public string Expected { get; set; } = "pass";
}

internal sealed class CheckResult
{
    public string Platform { get; set; } = "";
    public string Name { get; set; } = "";
    public string Format { get; set; } = "";
    public string Expected { get; set; } = "";
    public string Outcome { get; set; } = "";
    public string Status { get; set; } = "";
    public string? Title { get; set; }
    public string? Error { get; set; }
    public double Seconds { get; set; }
    public long SizeKb { get; set; }
    public string Duration { get; set; } = "";
    public string VideoCodec { get; set; } = "";
    public string AudioCodec { get; set; } = "";
    public bool Verified { get; set; }
}

/// <summary>Settings object read by YtDlpDownloadService via reflection (same names as SettingsService).</summary>
public sealed class HarnessSettings
{
    public int ParallelLimit { get; set; } = 3;
    public string CookiesFilePath { get; set; } = "";
    public bool UseBrowserCookies { get; set; }
    public string BrowserForCookies { get; set; } = "";
    public bool EnableAISubtitles { get; set; }
    public string SubtitleLanguage { get; set; } = "en";
    public int AudioQuality { get; set; }
}

internal sealed class NoCache : ICacheService
{
    public Task<CachedVideo?> GetVideoMetadataAsync(string videoId) => Task.FromResult<CachedVideo?>(null);
    public Task SetVideoMetadataAsync(string videoId, CachedVideo video, TimeSpan? expiration = null) => Task.CompletedTask;
    public Task<bool> IsVideoMetadataCachedAsync(string videoId) => Task.FromResult(false);
    public Task ClearCacheAsync() => Task.CompletedTask;
    public Task ClearExpiredCacheAsync() => Task.CompletedTask;
}
