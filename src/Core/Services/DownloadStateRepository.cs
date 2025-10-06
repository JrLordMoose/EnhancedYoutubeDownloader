using EnhancedYoutubeDownloader.Shared.Models;
using Microsoft.Data.Sqlite;

namespace EnhancedYoutubeDownloader.Core.Services;

/// <summary>
/// Manages the persistence of download state to a local database.
/// </summary>
public class DownloadStateRepository : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadStateRepository"/> class.
    /// </summary>
    public DownloadStateRepository()
    {
        var cacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EnhancedYoutubeDownloader"
        );
        Directory.CreateDirectory(cacheDir);

        _connectionString = $"Data Source={Path.Combine(cacheDir, "download_state.db")}";
        _connection = new SqliteConnection(_connectionString);

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        _connection.Open();

        var createTableCommand = _connection.CreateCommand();
        createTableCommand.CommandText = @"
            CREATE TABLE IF NOT EXISTS DownloadState (
                DownloadId TEXT PRIMARY KEY,
                VideoId TEXT NOT NULL,
                FilePath TEXT NOT NULL,
                PartialFilePath TEXT,
                BytesDownloaded INTEGER NOT NULL,
                TotalBytes INTEGER NOT NULL,
                Status TEXT NOT NULL,
                LastUpdated DATETIME NOT NULL
            )";
        createTableCommand.ExecuteNonQuery();
    }

    /// <summary>
    /// Saves the state of a download item.
    /// </summary>
    /// <param name="downloadItem">The download item to save.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SaveStateAsync(DownloadItem downloadItem)
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO DownloadState
                (DownloadId, VideoId, FilePath, PartialFilePath, BytesDownloaded, TotalBytes, Status, LastUpdated)
                VALUES (@downloadId, @videoId, @filePath, @partialFilePath, @bytesDownloaded, @totalBytes, @status, @lastUpdated)";

            command.Parameters.AddWithValue("@downloadId", downloadItem.Id);
            command.Parameters.AddWithValue("@videoId", downloadItem.Video?.Id ?? "");
            command.Parameters.AddWithValue("@filePath", downloadItem.FilePath ?? "");
            command.Parameters.AddWithValue("@partialFilePath", downloadItem.PartialFilePath ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@bytesDownloaded", downloadItem.BytesDownloaded);
            command.Parameters.AddWithValue("@totalBytes", downloadItem.TotalBytes);
            command.Parameters.AddWithValue("@status", downloadItem.Status.ToString());
            command.Parameters.AddWithValue("@lastUpdated", DateTime.UtcNow);

            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            // Log error - for now, we'll just throw
            throw new InvalidOperationException($"Failed to save download state: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads the state of a download item.
    /// </summary>
    /// <param name="downloadId">The ID of the download.</param>
    /// <returns>The download state, or null if not found.</returns>
    public async Task<DownloadState?> LoadStateAsync(string downloadId)
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT DownloadId, VideoId, FilePath, PartialFilePath, BytesDownloaded, TotalBytes, Status, LastUpdated
                FROM DownloadState
                WHERE DownloadId = @downloadId";

            command.Parameters.AddWithValue("@downloadId", downloadId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new DownloadState
                {
                    DownloadId = reader.GetString(0),
                    VideoId = reader.GetString(1),
                    FilePath = reader.GetString(2),
                    PartialFilePath = reader.IsDBNull(3) ? null : reader.GetString(3),
                    BytesDownloaded = reader.GetInt64(4),
                    TotalBytes = reader.GetInt64(5),
                    Status = Enum.Parse<DownloadStatus>(reader.GetString(6)),
                    LastUpdated = reader.GetDateTime(7)
                };
            }
        }
        catch (Exception ex)
        {
            // Log error
            throw new InvalidOperationException($"Failed to load download state: {ex.Message}", ex);
        }

        return null;
    }

    /// <summary>
    /// Deletes the state of a download item.
    /// </summary>
    /// <param name="downloadId">The ID of the download.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteStateAsync(string downloadId)
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "DELETE FROM DownloadState WHERE DownloadId = @downloadId";
            command.Parameters.AddWithValue("@downloadId", downloadId);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            // Log error
            throw new InvalidOperationException($"Failed to delete download state: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets all saved download states.
    /// </summary>
    /// <returns>A list of all download states.</returns>
    public async Task<List<DownloadState>> GetAllStatesAsync()
    {
        var states = new List<DownloadState>();

        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT DownloadId, VideoId, FilePath, PartialFilePath, BytesDownloaded, TotalBytes, Status, LastUpdated
                FROM DownloadState
                ORDER BY LastUpdated DESC";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                states.Add(new DownloadState
                {
                    DownloadId = reader.GetString(0),
                    VideoId = reader.GetString(1),
                    FilePath = reader.GetString(2),
                    PartialFilePath = reader.IsDBNull(3) ? null : reader.GetString(3),
                    BytesDownloaded = reader.GetInt64(4),
                    TotalBytes = reader.GetInt64(5),
                    Status = Enum.Parse<DownloadStatus>(reader.GetString(6)),
                    LastUpdated = reader.GetDateTime(7)
                });
            }
        }
        catch (Exception ex)
        {
            // Log error
            throw new InvalidOperationException($"Failed to get all download states: {ex.Message}", ex);
        }

        return states;
    }

    /// <summary>
    /// Disposes the database connection.
    /// </summary>
    public void Dispose()
    {
        _connection?.Dispose();
    }
}

/// <summary>
/// Represents the persisted state of a download.
/// </summary>
public class DownloadState
{
    /// <summary>
    /// Gets or sets the download ID.
    /// </summary>
    public required string DownloadId { get; init; }
    /// <summary>
    /// Gets or sets the video ID.
    /// </summary>
    public required string VideoId { get; init; }
    /// <summary>
    /// Gets or sets the final file path.
    /// </summary>
    public required string FilePath { get; init; }
    /// <summary>
    /// Gets or sets the partial file path.
    /// </summary>
    public string? PartialFilePath { get; init; }
    /// <summary>
    /// Gets or sets the number of bytes downloaded.
    /// </summary>
    public long BytesDownloaded { get; init; }
    /// <summary>
    /// Gets or sets the total number of bytes to download.
    /// </summary>
    public long TotalBytes { get; init; }
    /// <summary>
    /// Gets or sets the download status.
    /// </summary>
    public DownloadStatus Status { get; init; }
    /// <summary>
    /// Gets or sets the last updated timestamp.
    /// </summary>
    public DateTime LastUpdated { get; init; }
}