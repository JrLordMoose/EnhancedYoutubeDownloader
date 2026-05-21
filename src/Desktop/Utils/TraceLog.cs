using System.Diagnostics;

namespace EnhancedYoutubeDownloader.Utils;

/// <summary>
/// Debug-only trace logging (stripped from Release builds).
/// </summary>
internal static class TraceLog
{
    [Conditional("DEBUG")]
    public static void Write(string message) => Debug.WriteLine(message);
}
