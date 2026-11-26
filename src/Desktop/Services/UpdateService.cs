using System;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Onova;
using Onova.Models;
using Onova.Services;

namespace EnhancedYoutubeDownloader.Services;

/// <summary>
/// Thin wrapper around Onova. Use <see cref="UpdateCoordinator"/> for the user-facing flow:
/// LaunchUpdater only STARTS the updater, which then waits about 15 seconds for this app
/// to exit before giving up ("Updatee did not exit in time"), so the app must close
/// immediately afterwards.
/// </summary>
[SupportedOSPlatform("windows")]
public class UpdateService
{
    /// <summary>
    /// Testing hook: set EYD_UPDATE_SOURCE to a folder containing
    /// "EnhancedYoutubeDownloader-X.Y.Z.zip" to exercise the full update flow against a
    /// local package instead of GitHub. Never set on users' machines.
    /// </summary>
    public const string LocalSourceVariable = "EYD_UPDATE_SOURCE";

    private readonly UpdateManager _updateManager;

    public UpdateService()
    {
        var localSource = Environment.GetEnvironmentVariable(LocalSourceVariable);
        IPackageResolver resolver =
            !string.IsNullOrWhiteSpace(localSource) && Directory.Exists(localSource)
                ? new LocalPackageResolver(localSource, "EnhancedYoutubeDownloader-*.zip")
                : new GithubPackageResolver(
                    "JrLordMoose",
                    "EnhancedYoutubeDownloader",
                    "EnhancedYoutubeDownloader-*.zip"
                );

        _updateManager = new UpdateManager(resolver, new ZipPackageExtractor());
    }

    public async Task<Version?> CheckForUpdatesAsync()
    {
        try
        {
            CheckForUpdatesResult result = await _updateManager.CheckForUpdatesAsync();
            return result.CanUpdate ? result.LastVersion : null;
        }
        catch
        {
            return null;
        }
    }

    public Task PrepareUpdateAsync(
        Version version,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default
    ) => _updateManager.PrepareUpdateAsync(version, progress, cancellationToken);

    /// <summary>
    /// Starts the updater. The caller MUST exit the app right away (see class remarks).
    /// </summary>
    public void FinalizeUpdate(Version version, bool restart)
    {
        _updateManager.LaunchUpdater(version, restart);
    }
}
