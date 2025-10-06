using EnhancedYoutubeDownloader.Shared.Models;

namespace EnhancedYoutubeDownloader.Shared.Interfaces;

/// <summary>
/// Defines the contract for a service that resolves YouTube queries.
/// </summary>
public interface IQueryResolver
{
    /// <summary>
    /// Resolves a YouTube query (URL or search term) into a <see cref="QueryResult"/>.
    /// </summary>
    /// <param name="query">The YouTube URL or search query.</param>
    /// <returns>A <see cref="QueryResult"/> containing the resolved video(s) metadata.</returns>
    Task<QueryResult> ResolveAsync(string query);

    /// <summary>
    /// Checks if a string is a valid YouTube URL.
    /// </summary>
    /// <param name="query">The string to check.</param>
    /// <returns>True if the string is a YouTube URL, otherwise false.</returns>
    bool IsYouTubeUrl(string query);

    /// <summary>
    /// Extracts a video ID from a YouTube URL.
    /// </summary>
    /// <param name="url">The YouTube URL.</param>
    /// <returns>The video ID if found, otherwise null.</returns>
    string? ExtractVideoId(string url);

    /// <summary>
    /// Extracts a playlist ID from a YouTube URL.
    /// </summary>
    /// <param name="url">The YouTube URL.</param>
    /// <returns>The playlist ID if found, otherwise null.</returns>
    string? ExtractPlaylistId(string url);

    /// <summary>
    /// Extracts a channel ID from a YouTube URL.
    /// </summary>
    /// <param name="url">The YouTube URL.</param>
    /// <returns>The channel ID if found, otherwise null.</returns>
    string? ExtractChannelId(string url);
}