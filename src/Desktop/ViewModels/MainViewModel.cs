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
    private readonly SettingsService _settingsService;
    private readonly UpdateService? _updateService;

    public string Title { get; } = $"{Program.Name} v{Program.VersionString}";

    public DashboardViewModel Dashboard { get; }

    public SnackbarManager SnackbarManager { get; }

    public MainViewModel(
        ViewModelManager viewModelManager,
        SnackbarManager snackbarManager,
        SettingsService settingsService,
        UpdateService? updateService = null
    )
    {
        _viewModelManager = viewModelManager;
        SnackbarManager = snackbarManager;
        _settingsService = settingsService;
        _updateService = updateService;
        Dashboard = viewModelManager.CreateDashboardViewModel();
    }

    private Task ShowUkraineSupportMessageAsync()
    {
        if (!_settingsService.IsUkraineSupportMessageEnabled)
            return Task.CompletedTask;

        var dialog = _viewModelManager.CreateMessageBoxViewModel();
        // Set dialog properties here

        // Disable this message in the future
        _settingsService.IsUkraineSupportMessageEnabled = false;
        _settingsService.Save();
        return Task.CompletedTask;
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

        try
        {
            var updateVersion = await _updateService.CheckForUpdatesAsync();
            if (updateVersion is null)
                return;

            SnackbarManager.Notify($"Downloading update to {Program.Name} v{updateVersion}...");
            await _updateService.PrepareUpdateAsync(updateVersion);

            SnackbarManager.Notify(
                "Update has been downloaded and will be installed when you exit",
                "INSTALL NOW",
                () =>
                {
                    if (OperatingSystem.IsWindows())
                    {
                        _updateService.FinalizeUpdate(updateVersion, true);
                    }

                    if (
                        Application.Current?.ApplicationLifetime
                        is IClassicDesktopStyleApplicationLifetime desktop
                    )
                        desktop.Shutdown(2);
                    else
                        Environment.Exit(2);
                }
            );
        }
        catch
        {
            // Failure to update shouldn't crash the application
            SnackbarManager.Notify("Failed to perform application update");
        }
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        await ShowUkraineSupportMessageAsync();
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
