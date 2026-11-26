namespace EnhancedYoutubeDownloader.Core.Utils;

/// <summary>
/// Kinds of yt-dlp failure that need a different response from the user.
/// </summary>
public enum YtDlpErrorKind
{
    /// <summary>Not an authentication problem.</summary>
    Other,

    /// <summary>The site wants a signed-in session (bot check, login wall, 403).</summary>
    SignInRequired,

    /// <summary>yt-dlp could not read the browser's cookie store (e.g. Chrome/Edge DPAPI on Windows).</summary>
    CookieReadFailed,
}

/// <summary>
/// Classifies yt-dlp error text. YouTube changed its block from "HTTP 403" to
/// "Sign in to confirm you're not a bot", so matching only "403" misses the common case.
/// </summary>
public static class YtDlpErrorClassifier
{
    private static readonly string[] CookieReadMarkers =
    [
        "Failed to decrypt with DPAPI",
        "Could not copy Chrome cookie database",
        "could not find chrome cookies database",
        "could not find edge cookies database",
        "could not find firefox cookies database",
        "Permission denied: 'Cookies'",
    ];

    private static readonly string[] SignInMarkers =
    [
        "confirm you're not a bot",
        "confirm you’re not a bot",
        "confirm you are not a bot",
        "Sign in to confirm",
        "--cookies-from-browser or --cookies",
        "Account authentication is required",
        "only works when logged-in",
        "login required",
        "requires authentication",
        "This video is only available to registered users",
        "HTTP Error 403",
        "403: Forbidden",
        "403 Forbidden",
    ];

    public static YtDlpErrorKind Classify(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return YtDlpErrorKind.Other;

        // yt-dlp's error output also carries WARNING lines (e.g. Bilibili: "Subtitles are
        // only available when logged in. Use --cookies…"). Only the ERROR part decides.
        var errorIndex = message.IndexOf("ERROR:", StringComparison.Ordinal);
        if (errorIndex >= 0)
            message = message[errorIndex..];

        if (CookieReadMarkers.Any(m => message.Contains(m, StringComparison.OrdinalIgnoreCase)))
            return YtDlpErrorKind.CookieReadFailed;

        if (SignInMarkers.Any(m => message.Contains(m, StringComparison.OrdinalIgnoreCase)))
            return YtDlpErrorKind.SignInRequired;

        return YtDlpErrorKind.Other;
    }

    public static bool NeedsAuthentication(string? message) => Classify(message) != YtDlpErrorKind.Other;
}
