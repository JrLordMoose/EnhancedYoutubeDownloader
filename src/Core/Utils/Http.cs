using System.Net.Http;

namespace EnhancedYoutubeDownloader.Core.Utils;

public static class Http
{
    public static HttpClient Client { get; } =
        new()
        {
            Timeout = TimeSpan.FromMinutes(5),
        };

    static Http()
    {
        Client.DefaultRequestHeaders.TryAddWithoutValidation(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36"
        );
        TraceLog.Write("[HTTP] HttpClient initialized with browser User-Agent");
    }
}
