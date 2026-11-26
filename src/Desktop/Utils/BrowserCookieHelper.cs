using System;
using System.IO;

namespace EnhancedYoutubeDownloader.Utils;

/// <summary>
/// Helpers for choosing a browser yt-dlp can actually read cookies from.
/// </summary>
public static class BrowserCookieHelper
{
    /// <summary>Plain-English guide: what cookies.txt is, when it's needed, setup, fixes.</summary>
    public const string GuideUrl =
        "https://github.com/JrLordMoose/EnhancedYoutubeDownloader/blob/main/COOKIES_GUIDE.md";

    /// <summary>
    /// True when a Firefox profile exists. Firefox is the only mainstream Windows browser
    /// whose cookies yt-dlp can read; Chromium browsers use app-bound (DPAPI) encryption.
    /// </summary>
    public static bool IsFirefoxInstalled()
    {
        try
        {
            var profiles =
                OperatingSystem.IsWindows()
                    ? Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "Mozilla",
                        "Firefox",
                        "Profiles"
                    )
                : OperatingSystem.IsMacOS()
                    ? Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "Library",
                        "Application Support",
                        "Firefox",
                        "Profiles"
                    )
                : Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".mozilla",
                    "firefox"
                );

            return Directory.Exists(profiles);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
