using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Services;
using EnhancedYoutubeDownloader.Utils;
using EnhancedYoutubeDownloader.Utils.Extensions;
using EnhancedYoutubeDownloader.ViewModels.Components;

namespace EnhancedYoutubeDownloader.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly ViewModelManager _viewModelManager;
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;
    private readonly UpdateService? _updateService;

    public string Title { get; } = $"Enhanced YouTube Downloader v{Program.VersionString}";

    public DashboardViewModel Dashboard { get; }

    public SnackbarManager SnackbarManager { get; }

    public MainViewModel(
        ViewModelManager viewModelManager,
        SnackbarManager snackbarManager,
        SettingsService settingsService,
        DialogManager dialogManager,
        UpdateService? updateService = null
    )
    {
        _dialogManager = dialogManager;
        _viewModelManager = viewModelManager;
        SnackbarManager = snackbarManager;
        _settingsService = settingsService;
        _updateService = updateService;
        Dashboard = viewModelManager.CreateDashboardViewModel();
    }

    private Task ShowDevelopmentBuildMessageAsync()
    {
        if (!Program.IsDevelopmentBuild)
            return Task.CompletedTask;

        // If debugging, the user is likely a developer
        if (Debugger.IsAttached)
            return Task.CompletedTask;

        var dialog = _viewModelManager.CreateMessageBoxViewModel();
        // Set dialog properties here
        return Task.CompletedTask;
    }

    private Task ShowFFmpegMessageAsync()
    {
        // Check if FFmpeg is available
        var dialog = _viewModelManager.CreateMessageBoxViewModel();
        // Set dialog properties here
        return Task.CompletedTask;
    }

    private async Task CheckForUpdatesAsync()
    {
        // Updates are only supported on Windows
        if (_updateService == null || !OperatingSystem.IsWindows())
            return;

        // Respect Settings > General > "Automatic updates"; manual checks still work
        if (!_settingsService.IsAutoUpdateEnabled)
            return;

        try
        {
            var updateVersion = await _updateService.CheckForUpdatesAsync();
            if (updateVersion is null)
                return;

            // Ask with a real dialog. Older builds only showed a toast whose "INSTALL NOW"
            // button couldn't be clicked, and nothing installed the update on exit.
            var prompt = _viewModelManager.CreateMessageBoxViewModel();
            prompt.Title = "Update Available";
            prompt.Message =
                $"Version {updateVersion.ToString(3)} is available (you have {Program.VersionString}).\n\n"
                + "Update now? The app will close, install the update and reopen.\n"
                + "Choose Later to install it automatically the next time you close the app.";
            prompt.PrimaryButtonText = "Update Now";
            prompt.SecondaryButtonText = "Later";

            var updateNow = await _dialogManager.ShowDialogAsync(prompt) == true;

            SnackbarManager.Notify($"Downloading update v{updateVersion.ToString(3)}...");
            await _updateService.PrepareUpdateAsync(updateVersion);

            if (updateNow)
            {
                _updateService.FinalizeUpdate(updateVersion, restart: true);
                ShutdownForUpdate();
                return;
            }

            // Install on exit (no restart)
            _pendingUpdate = updateVersion;
            if (
                Application.Current?.ApplicationLifetime
                is IClassicDesktopStyleApplicationLifetime lifetime
            )
            {
                lifetime.Exit += (_, _) => ApplyPendingUpdateOnExit();
            }

            SnackbarManager.Notify(
                $"Update v{updateVersion.ToString(3)} is ready and will install when you close the app"
            );
        }
        catch
        {
            // Failure to update shouldn't crash the application
            SnackbarManager.Notify("Failed to perform application update");
        }
    }

    private Version? _pendingUpdate;

    private void ApplyPendingUpdateOnExit()
    {
        if (_pendingUpdate is null || _updateService is null || !OperatingSystem.IsWindows())
            return;

        try
        {
            _updateService.FinalizeUpdate(_pendingUpdate, restart: false);
        }
        catch
        {
            // Never block exit; the update will be offered again next launch
        }
        finally
        {
            _pendingUpdate = null;
        }
    }

    private static void ShutdownForUpdate()
    {
        if (
            Application.Current?.ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime desktop
        )
            desktop.Shutdown(2);
        else
            Environment.Exit(2);
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        await ShowDevelopmentBuildMessageAsync();
        await ShowFFmpegMessageAsync();
        // Check for updates on startup
        await CheckForUpdatesAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Save settings
            _settingsService.Save();
        }

        base.Dispose(disposing);
    }
}
