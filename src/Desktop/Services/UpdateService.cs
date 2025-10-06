using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Onova;
using Onova.Services;

namespace EnhancedYoutubeDownloader.Services;

/// <summary>
/// Manages application updates using Onova.
/// </summary>
[SupportedOSPlatform("windows")]
public class UpdateService
{
    private readonly UpdateManager _updateManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateService"/> class.
    /// </summary>
    public UpdateService()
    {
        _updateManager = new UpdateManager(
            new GithubPackageResolver(
                "JrLordMoose",
                "EnhancedYoutubeDownloader",
                "EnhancedYoutubeDownloader-*.zip"
            ),
            new ZipPackageExtractor()
        );
    }

    /// <summary>
    /// Checks for application updates.
    /// </summary>
    /// <returns>The latest version if an update is available, otherwise null.</returns>
    public async Task<Version?> CheckForUpdatesAsync()
    {
        try
        {
            var result = await _updateManager.CheckForUpdatesAsync();
            return result.CanUpdate ? result.LastVersion : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Prepares an update for installation.
    /// </summary>
    /// <param name="version">The version to prepare.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task PrepareUpdateAsync(Version version)
    {
        await _updateManager.PrepareUpdateAsync(version);
    }

    /// <summary>
    /// Finalizes an update and optionally restarts the application.
    /// </summary>
    /// <param name="version">The version to finalize.</param>
    /// <param name="restart">Whether to restart the application after the update.</param>
    public void FinalizeUpdate(Version version, bool restart)
    {
        _updateManager.LaunchUpdater(version, restart);
    }
}