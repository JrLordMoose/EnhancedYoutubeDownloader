using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace EnhancedYoutubeDownloader.Core.Utils;

public static class Http
{
    public static HttpClient Client { get; } =
        new()
        {
            DefaultRequestHeaders =
            {
                // Required by YouTube to accept requests
                UserAgent =
                {
                    new ProductInfoHeaderValue(
                        "EnhancedYoutubeDownloader",
                        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0"
                    ),
                },
            },
            // Set a reasonable timeout
            Timeout = TimeSpan.FromMinutes(5),
        };

    static Http()
    {
        Console.WriteLine($"[HTTP] Custom HttpClient initialized with User-Agent: EnhancedYoutubeDownloader/{Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0"}");
    }
}
