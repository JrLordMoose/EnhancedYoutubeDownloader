using EnhancedYoutubeDownloader.Core.Services;
using EnhancedYoutubeDownloader.Core.Utils;
using EnhancedYoutubeDownloader.Shared.Interfaces;
using EnhancedYoutubeDownloader.Shared.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnhancedYoutubeDownloader.Tests.Core.Services;

/// <summary>
/// Guards for the v0.4.8 fixes: sign-in detection, format fallbacks, platform
/// detection and the live parallel-download limit.
/// </summary>
public class Session39RegressionTests
{
    // Real yt-dlp 2026.11.23 output captured on 2026-11-25
    private const string YouTubeBotCheck =
        "ERROR: [youtube] jNQXAC9IVRw: Sign in to confirm you’re not a bot. This helps protect our community. "
        + "Use --cookies-from-browser or --cookies for the authentication.";

    [Theory]
    [InlineData(YouTubeBotCheck)]
    [InlineData("ERROR: [Reddit] 1grt9hm: Account authentication is required. Use --cookies-from-browser or --cookies")]
    [InlineData("ERROR: [vimeo] 76979871: The web client only works when logged-in.")]
    [InlineData("ERROR: unable to download video data: HTTP Error 403: Forbidden")]
    public void Classify_SignInWalls_AreDetected(string message)
    {
        YtDlpErrorClassifier.Classify(message).Should().Be(YtDlpErrorKind.SignInRequired);
        YtDlpErrorClassifier.NeedsAuthentication(message).Should().BeTrue();
    }

    [Fact]
    public void Classify_ChromiumDpapiFailure_IsCookieReadFailure()
    {
        const string message =
            "ERROR: Failed to decrypt with DPAPI. See  https://github.com/yt-dlp/yt-dlp/issues/10927  for more info";

        YtDlpErrorClassifier.Classify(message).Should().Be(YtDlpErrorKind.CookieReadFailed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ERROR: [twitch:vod] 2276391219: Video 2276391219 does not exist")]
    [InlineData("Requested format is not available")]
    public void Classify_OtherErrors_AreNotAuthentication(string? message)
    {
        YtDlpErrorClassifier.NeedsAuthentication(message).Should().BeFalse();
    }

    [Theory]
    [InlineData("highest", "mp4")]
    [InlineData("720p", "mp4")]
    [InlineData("1080p", "webm")]
    public void BuildFormatString_Video_EndsWithUniversalFallback(string quality, string container)
    {
        var format = YtDlpDownloadService.BuildFormatString(
            new FormatProfile { Quality = quality, Container = container }
        );

        // Sites without mp4/webm streams previously failed with "Requested format is not available"
        format.Should().EndWith("/best");
        format.Should().Contain("+bestaudio/");
    }

    [Theory]
    [InlineData("audio-only", "mp3")]
    [InlineData("highest", "mp3")]
    public void BuildFormatString_Mp3_IsAudioWithCombinedFallback(string quality, string container)
    {
        var format = YtDlpDownloadService.BuildFormatString(
            new FormatProfile { Quality = quality, Container = container }
        );

        format.Should().Be("bestaudio/best");
    }

    [Theory]
    [InlineData("https://x.com/SpaceX/status/1732824684683784516", PlatformType.Twitter)]
    [InlineData("https://twitter.com/NASA", PlatformType.Twitter)]
    [InlineData("https://www.netflix.com/title/80057281", PlatformType.Generic)]
    [InlineData("https://www.dropbox.com/s/abc/video.mp4", PlatformType.Generic)]
    [InlineData("https://www.tiktok.com/@scout2015/video/6718335390845095173", PlatformType.TikTok)]
    public void DetectPlatform_UsesHostNotSubstring(string url, PlatformType expected)
    {
        var resolver = new QueryResolver(new Mock<ICacheService>().Object);

        resolver.DetectPlatform(url).Should().Be(expected);
    }

    [Fact]
    public void ParallelLimit_IsReadLiveFromSettingsAndClamped()
    {
        var settings = new FakeSettings { ParallelLimit = 5 };
        using var service = new YtDlpDownloadService(
            new Mock<ISubtitleBurnInService>().Object,
            settings
        );

        service.GetParallelLimit().Should().Be(5);

        settings.ParallelLimit = 1;
        service.GetParallelLimit().Should().Be(1);

        settings.ParallelLimit = 0;
        service.GetParallelLimit().Should().Be(1);

        settings.ParallelLimit = 50;
        service.GetParallelLimit().Should().Be(10);
    }

    [Fact]
    public void FindSubtitleFile_FindsSidecarWithBracketsInTitle()
    {
        var dir = Path.Combine(Path.GetTempPath(), "eyd-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var video = Path.Combine(dir, "Song [Official Video].mp4");
            File.WriteAllText(video, "v");
            File.WriteAllText(Path.Combine(dir, "Song [Official Video].es.srt"), "1");
            File.WriteAllText(Path.Combine(dir, "Other.en.srt"), "1");

            var found = YtDlpDownloadService.FindSubtitleFile(video);

            found.Should().NotBeNull();
            Path.GetFileName(found).Should().Be("Song [Official Video].es.srt");
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    private sealed class FakeSettings
    {
        public int ParallelLimit { get; set; }
    }
}

public class Session39UrlAndPipelineTests
{
    private readonly QueryResolver _resolver = new(new Mock<ICacheService>().Object);

    [Theory]
    [InlineData("https://m.youtube.com/watch?app=desktop&v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?feature=share&v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/live/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://music.youtube.com/watch?v=dQw4w9WgXcQ&list=RDAMVM", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/shorts/tPEE9ZwTmy0", "tPEE9ZwTmy0")]
    public void ExtractVideoId_HandlesExtraParametersAndPaths(string url, string expected)
    {
        _resolver.ExtractVideoId(url).Should().Be(expected);
    }

    [Theory]
    [InlineData("vimeo.com/76979871", PlatformType.Generic)]
    [InlineData("dailymotion.com/video/x7tgad0", PlatformType.Generic)]
    [InlineData("funny cat videos", PlatformType.Unknown)]
    [InlineData("example.com", PlatformType.Unknown)]
    public void DetectPlatform_BareUrlsAreNotSearches(string query, PlatformType expected)
    {
        _resolver.DetectPlatform(query).Should().Be(expected);
    }

    [Theory]
    [InlineData("10.50MiB", 11010048L)]
    [InlineData("~1.5GiB", 1610612736L)]
    [InlineData("500KiB", 512000L)]
    public void TryParseSize_IsCultureInvariantAndAcceptsEstimates(string input, long expected)
    {
        var previous = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
            var method = typeof(YtDlpDownloadService).GetMethod(
                "TryParseSize",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
            var args = new object?[] { input, 0L };
            ((bool)method.Invoke(null, args)!).Should().BeTrue();
            ((long)args[1]!).Should().Be(expected);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void FindDownloadedFileCandidates_IgnoresOlderAndUnrelatedFiles()
    {
        var dir = Path.Combine(Path.GetTempPath(), "eyd-cand-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var unrelated = Path.Combine(dir, "Intro to Python - Full Course.mp4");
            File.WriteAllText(unrelated, new string('x', 5000));
            var stale = Path.Combine(dir, "Intro.webm");
            File.WriteAllText(stale, "old");
            File.SetLastWriteTimeUtc(stale, DateTime.UtcNow.AddHours(-2));
            var produced = Path.Combine(dir, "Intro.mkv");
            File.WriteAllText(produced, "new");
            File.WriteAllText(Path.Combine(dir, "Intro.en.srt"), "subs");

            var found = YtDlpDownloadService.FindDownloadedFileCandidates(dir, "Intro", DateTime.UtcNow.AddMinutes(-1));

            found.Should().ContainSingle().Which.Should().Be(produced);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Theory]
    [InlineData(0, "0")]
    [InlineData(1, "320K")]
    [InlineData(3, "128K")]
    public void AudioQuality_MapsToYtDlpArgument(int index, string expected)
    {
        YtDlpDownloadService.GetAudioQualityArgument(index).Should().Be(expected);
    }
}

public class Session39ClassifierWarningTests
{
    [Fact]
    public void Classify_IgnoresSignInWordingInWarnings_WhenTheErrorIsSomethingElse()
    {
        const string message =
            "yt-dlp download failed: WARNING: [BiliBili] Subtitles are only available when logged in. "
            + "Use --cookies-from-browser or --cookies for the authentication., "
            + "ERROR: [BiliBili] BV1: Requested format is not available";

        YtDlpErrorClassifier.Classify(message).Should().Be(YtDlpErrorKind.Other);
    }

    [Fact]
    public void BuildFormatString_Mp4_PrefersH264()
    {
        var format = YtDlpDownloadService.BuildFormatString(
            new FormatProfile { Quality = "720p", Container = "mp4" }
        );

        format.Should().StartWith("bestvideo[height<=720][ext=mp4][vcodec^=avc1]+bestaudio[ext=m4a]");
    }
}
