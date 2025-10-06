using System.Text.Json;
using EnhancedYoutubeDownloader.Shared.Interfaces;
using EnhancedYoutubeDownloader.Shared.Models;
using Microsoft.Data.Sqlite;

namespace EnhancedYoutubeDownloader.Core.Services;

/// <summary>
/// Provides caching services for video metadata to reduce redundant API calls.
/// </summary>
public class CacheService : ICacheService, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly string _connectionString;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheService"/> class.
    /// </summary>
    public CacheService()
    {
        var cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EnhancedYoutubeDownloader");
        Directory.CreateDirectory(cacheDir);

        _connectionString = $"Data Source={Path.Combine(cacheDir, "cache.db")}";
        _connection = new SqliteConnection(_connectionString);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        _connection.Open();

        var createTableCommand = _connection.CreateCommand();
        createTableCommand.CommandText = @"
            CREATE TABLE IF NOT EXISTS VideoMetadata (
                VideoId TEXT PRIMARY KEY,
                JsonData TEXT NOT NULL,
                CreatedAt DATETIME NOT NULL,
                ExpiresAt DATETIME
            )";
        createTableCommand.ExecuteNonQuery();
    }

    /// <summary>
    /// Gets cached video metadata by video ID.
    /// </summary>
    /// <param name="videoId">The ID of the video.</param>
    /// <returns>The cached video metadata, or null if not found or expired.</returns>
    public async Task<CachedVideo?> GetVideoMetadataAsync(string videoId)
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT JsonData FROM VideoMetadata
                WHERE VideoId = @videoId
                AND (ExpiresAt IS NULL OR ExpiresAt > @now)";

            command.Parameters.AddWithValue("@videoId", videoId);
            command.Parameters.AddWithValue("@now", DateTime.UtcNow);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var jsonData = reader.GetString(0);
                return JsonSerializer.Deserialize<CachedVideo>(jsonData, _jsonOptions);
            }
        }
        catch
        {
            // Log error
        }

        return null;
    }

    /// <summary>
    /// Sets cached video metadata.
    /// </summary>
    /// <param name="videoId">The ID of the video.</param>
    /// <param name="video">The video metadata to cache.</param>
    /// <param name="expiration">The cache expiration timespan. If null, the cache does not expire.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SetVideoMetadataAsync(string videoId, CachedVideo video, TimeSpan? expiration = null)
    {
        try
        {
            var jsonData = JsonSerializer.Serialize(video, _jsonOptions);
            var expiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : (DateTime?)null;

            using var command = _connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO VideoMetadata (VideoId, JsonData, CreatedAt, ExpiresAt)
                VALUES (@videoId, @jsonData, @createdAt, @expiresAt)";

            command.Parameters.AddWithValue("@videoId", videoId);
            command.Parameters.AddWithValue("@jsonData", jsonData);
            command.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("@expiresAt", expiresAt ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }
        catch
        {
            // Log error
        }
    }

    /// <summary>
    /// Checks if video metadata is cached.
    /// </summary>
    /// <param name="videoId">The ID of the video.</param>
    /// <returns>True if the metadata is cached and not expired, false otherwise.</returns>
    public async Task<bool> IsVideoMetadataCachedAsync(string videoId)
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*) FROM VideoMetadata
                WHERE VideoId = @videoId
                AND (ExpiresAt IS NULL OR ExpiresAt > @now)";

            command.Parameters.AddWithValue("@videoId", videoId);
            command.Parameters.AddWithValue("@now", DateTime.UtcNow);

            var count = await command.ExecuteScalarAsync();
            return Convert.ToInt32(count) > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Clears the entire cache.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ClearCacheAsync()
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "DELETE FROM VideoMetadata";
            await command.ExecuteNonQueryAsync();
        }
        catch
        {
            // Log error
        }
    }

    /// <summary>
    /// Clears expired cache entries.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ClearExpiredCacheAsync()
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "DELETE FROM VideoMetadata WHERE ExpiresAt IS NOT NULL AND ExpiresAt <= @now";
            command.Parameters.AddWithValue("@now", DateTime.UtcNow);
            await command.ExecuteNonQueryAsync();
        }
        catch
        {
            // Log error
        }
    }

    /// <summary>
    /// Disposes the database connection.
    /// </summary>
    public void Dispose()
    {
        _connection?.Dispose();
    }
}