using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace EnhancedYoutubeDownloader.Core.Utils;

/// <summary>
/// Provides a shared, pre-configured HttpClient for making HTTP requests.
/// </summary>
public static class Http
{
    /// <summary>
    /// Gets the shared HttpClient instance.
    /// </summary>
    /// <remarks>
    /// This client is configured with a User-Agent header required by YouTube and a default timeout.
    /// </remarks>
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

    /// <summary>
    /// Initializes the <see cref="Http"/> class.
    /// </summary>
    static Http()
    {
        Console.WriteLine($"[HTTP] Custom HttpClient initialized with User-Agent: EnhancedYoutubeDownloader/{Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0"}");
    }
}