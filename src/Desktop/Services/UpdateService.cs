using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Onova;
using Onova.Services;

namespace EnhancedYoutubeDownloader.Services;

[SupportedOSPlatform("windows")]
public class UpdateService
{
    private readonly UpdateManager _updateManager;

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

    public async Task PrepareUpdateAsync(Version version)
    {
        await _updateManager.PrepareUpdateAsync(version);
    }

    public void FinalizeUpdate(Version version, bool restart)
    {
        _updateManager.LaunchUpdater(version, restart);
    }
}
