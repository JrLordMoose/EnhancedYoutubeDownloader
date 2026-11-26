using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Utils;
using EnhancedYoutubeDownloader.ViewModels.Dialogs;

namespace EnhancedYoutubeDownloader.Services;

/// <summary>
/// The ONE user-facing update flow, used by both the startup check and Settings ›
/// "Check for updates now". Lessons from v0.4.7 that this class exists to enforce:
/// <list type="number">
/// <item>Ask with a real dialog (v0.4.7's toast never rendered, so nobody knew an update existed).</item>
/// <item>Show download progress (the app used to just vanish).</item>
/// <item>Stop yt-dlp/ffmpeg/QuickJS so no file in the install folder is locked.</item>
/// <item>EXIT IMMEDIATELY after launching the updater: it waits ~15s for this process to
/// exit, then gives up with "Updatee did not exit in time" and nothing is installed.
/// v0.4.7's Settings path launched the updater but never exited.</item>
/// </list>
/// Test locally before every release: see UpdateService.LocalSourceVariable.
/// </summary>
public class UpdateCoordinator
{
    private static readonly string[] HelperProcesses = ["yt-dlp", "ffmpeg", "ffprobe", "qjs"];

    private readonly DialogManager _dialogManager;
    private readonly UpdateService? _updateService;
    private Version? _installOnExitVersion;
    private bool _exitHandlerRegistered;

    public UpdateCoordinator(DialogManager dialogManager, UpdateService? updateService = null)
    {
        _dialogManager = dialogManager;
        _updateService = updateService;
    }

    [System.Runtime.Versioning.SupportedOSPlatformGuard("windows")]
    public bool IsSupported => _updateService != null && OperatingSystem.IsWindows();

    public Task<Version?> CheckForUpdatesAsync() =>
        IsSupported ? _updateService!.CheckForUpdatesAsync() : Task.FromResult<Version?>(null);

    /// <summary>Asks "Update now?". Returns true when the user chose to update now.</summary>
    public async Task<bool> PromptAsync(Version version)
    {
        // Unattended update testing (monthly health check): auto-click "Update Now", but ONLY
        // when a local test package folder is configured, so real GitHub updates always ask.
        if (
            Environment.GetEnvironmentVariable("EYD_UPDATE_AUTOACCEPT") == "1"
            && !string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(UpdateService.LocalSourceVariable)
            )
        )
        {
            return true;
        }

        var prompt = new MessageBoxViewModel
        {
            Title = "Update Available",
            Message =
                $"Version {version.ToString(3)} is available (you have {Program.VersionString}).\n\n"
                + "Update now? You'll see the download progress, then the app closes, installs "
                + "the update and reopens by itself (about 10–20 seconds). Active downloads will stop.\n\n"
                + "Choose Later to install it automatically the next time you close the app.",
            PrimaryButtonText = "Update Now",
            SecondaryButtonText = "Later",
        };

        return await _dialogManager.ShowDialogAsync(prompt) == true;
    }

    /// <summary>Downloads with a visible progress dialog, then installs and restarts.</summary>
    public async Task InstallNowAsync(Version version)
    {
        if (!IsSupported)
            return;

        var progressDialog = new UpdateProgressViewModel
        {
            Title = $"Updating to v{version.ToString(3)}",
            Status = "Downloading update...",
            Detail = "You can keep this window open. Don't turn off your computer.",
        };

        // Shown without awaiting: the dialog stays up while we drive it
        _ = _dialogManager.ShowDialogAsync(progressDialog);

        using var cancellation = new System.Threading.CancellationTokenSource();
        try
        {
            var progress = new Progress<double>(p =>
            {
                if (progressDialog.IsCanceled)
                    cancellation.Cancel();

                progressDialog.Progress = p * 100;
                progressDialog.Status = $"Downloading update... {p * 100:F0}%";
            });

            await _updateService!.PrepareUpdateAsync(version, progress, cancellation.Token);

            if (progressDialog.IsCanceled)
                return;

            progressDialog.CanCancel = false;
            progressDialog.IsIndeterminate = true;
            progressDialog.Status = "Installing update...";
            progressDialog.Detail =
                "The app will close now and reopen automatically in a few seconds. "
                + "A small updater window may appear briefly.";

            // Let the user read it before the window disappears
            await Task.Delay(TimeSpan.FromSeconds(2.5));

            StopHelperProcesses();
            _installOnExitVersion = null; // this path installs now; the exit hook must not run too
            _updateService.FinalizeUpdate(version, restart: true);
            TraceLog.Write($"[UPDATE] Updater launched for v{version}; exiting now");
            ExitApplication();
        }
        catch (OperationCanceledException) when (progressDialog.IsCanceled)
        {
            TraceLog.Write("[UPDATE] Update download canceled by the user");
        }
        catch (Exception ex)
        {
            TraceLog.Write($"[UPDATE] Update failed: {ex.Message}");
            progressDialog.CloseCommand.Execute(false);

            await _dialogManager.ShowDialogAsync(
                new MessageBoxViewModel
                {
                    Title = "Update Failed",
                    Message =
                        $"The update couldn't be installed:\n{ex.Message}\n\n"
                        + "You can try again from Settings › Advanced › Check for updates now, or "
                        + "download the installer from github.com/JrLordMoose/EnhancedYoutubeDownloader/releases.",
                    PrimaryButtonText = "OK",
                }
            );
        }
    }

    /// <summary>
    /// "Later": download quietly now, install when the user closes the app (no restart).
    /// </summary>
    public async Task ScheduleInstallOnExitAsync(Version version)
    {
        if (!IsSupported)
            return;

        try
        {
            await _updateService!.PrepareUpdateAsync(version);
            _installOnExitVersion = version;

            if (
                !_exitHandlerRegistered
                && Application.Current?.ApplicationLifetime
                    is IClassicDesktopStyleApplicationLifetime lifetime
            )
            {
                lifetime.Exit += (_, _) => InstallPendingOnExit();
                _exitHandlerRegistered = true;
            }
        }
        catch (Exception ex)
        {
            // Not fatal: the update is offered again on next launch
            TraceLog.Write($"[UPDATE] Background download failed: {ex.Message}");
        }
    }

    private void InstallPendingOnExit()
    {
        if (_installOnExitVersion is not { } version || !IsSupported)
            return;

        try
        {
            StopHelperProcesses();
            _updateService!.FinalizeUpdate(version, restart: false);
        }
        catch (Exception ex)
        {
            TraceLog.Write($"[UPDATE] Install-on-exit failed: {ex.Message}");
        }
        finally
        {
            _installOnExitVersion = null;
        }
    }

    /// <summary>
    /// Kills yt-dlp/ffmpeg/QuickJS processes started from THIS install folder (downloads, the
    /// startup yt-dlp self-update). A running yt-dlp.exe is locked and the updater can't
    /// replace it.
    /// </summary>
    private static void StopHelperProcesses()
    {
        var installDir = Path.GetFullPath(AppContext.BaseDirectory);
        foreach (var name in HelperProcesses)
        {
            foreach (var process in Process.GetProcessesByName(name))
            {
                try
                {
                    var path = process.MainModule?.FileName;
                    if (
                        path != null
                        && Path.GetFullPath(path)
                            .StartsWith(installDir, StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        process.Kill(entireProcessTree: true);
                        process.WaitForExit(3000);
                    }
                }
                catch (Exception)
                {
                    // Access denied / already exited: nothing to do
                }
                finally
                {
                    process.Dispose();
                }
            }
        }
    }

    /// <summary>
    /// Normal shutdown, plus a hard exit as a safety net: if anything keeps the process alive,
    /// the updater's ~15s window lapses and the update silently fails (the v0.4.7 bug).
    /// </summary>
    private static void ExitApplication()
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(4));
            Environment.Exit(0);
        });

        if (
            Application.Current?.ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime desktop
        )
            desktop.Shutdown(0);
        else
            Environment.Exit(0);
    }
}
