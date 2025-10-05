using EnhancedYoutubeDownloader.Shared.Models;

namespace EnhancedYoutubeDownloader.Shared.Interfaces;

/// <summary>
/// Service for resolving YouTube queries (URLs or search terms) into video metadata
/// </summary>
public interface IQueryResolver
{
    /// <summary>
    /// Resolves a YouTube query (URL or search term) into a QueryResult
    /// </summary>
    /// <param name="query">YouTube URL or search query</param>
    /// <returns>QueryResult containing video(s) metadata</returns>
    Task<QueryResult> ResolveAsync(string query);

    /// <summary>
    /// Checks if a string is a valid YouTube URL
    /// </summary>
    /// <param name="query">String to check</param>
    /// <returns>True if the string is a YouTube URL</returns>
    bool IsYouTubeUrl(string query);

    /// <summary>
    /// Extracts a video ID from a YouTube URL
    /// </summary>
    /// <param name="url">YouTube URL</param>
    /// <returns>Video ID if found, null otherwise</returns>
    string? ExtractVideoId(string url);

    /// <summary>
    /// Extracts a playlist ID from a YouTube URL
    /// </summary>
    /// <param name="url">YouTube URL</param>
    /// <returns>Playlist ID if found, null otherwise</returns>
    string? ExtractPlaylistId(string url);

    /// <summary>
    /// Extracts a channel ID from a YouTube URL
    /// </summary>
    /// <param name="url">YouTube URL</param>
    /// <returns>Channel ID if found, null otherwise</returns>
    string? ExtractChannelId(string url);
}
