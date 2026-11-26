using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
    private readonly UpdateCoordinator _updateCoordinator;

    public string Title { get; } = $"Enhanced YouTube Downloader v{Program.VersionString}";

    public DashboardViewModel Dashboard { get; }

    public SnackbarManager SnackbarManager { get; }

    public MainViewModel(
        ViewModelManager viewModelManager,
        SnackbarManager snackbarManager,
        SettingsService settingsService,
        DialogManager dialogManager,
        UpdateCoordinator updateCoordinator
    )
    {
        _dialogManager = dialogManager;
        _viewModelManager = viewModelManager;
        SnackbarManager = snackbarManager;
        _settingsService = settingsService;
        _updateCoordinator = updateCoordinator;
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
        if (!_updateCoordinator.IsSupported)
            return;

        // Respect Settings > General > "Automatic updates"; manual checks still work
        if (!_settingsService.IsAutoUpdateEnabled)
            return;

        try
        {
            var updateVersion = await _updateCoordinator.CheckForUpdatesAsync();
            if (updateVersion is null)
                return;

            if (await _updateCoordinator.PromptAsync(updateVersion))
            {
                // Progress dialog -> install -> restart (the app exits inside this call)
                await _updateCoordinator.InstallNowAsync(updateVersion);
                return;
            }

            SnackbarManager.Notify(
                $"Update v{updateVersion.ToString(3)} will install when you close the app"
            );
            await _updateCoordinator.ScheduleInstallOnExitAsync(updateVersion);
        }
        catch
        {
            // Failure to update shouldn't crash the application
            SnackbarManager.Notify("Couldn't check for updates");
        }
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
