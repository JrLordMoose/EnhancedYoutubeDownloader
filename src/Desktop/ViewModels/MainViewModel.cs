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

/// <summary>
/// The main view model for the application.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly ViewModelManager _viewModelManager;
    private readonly SettingsService _settingsService;
    private readonly UpdateService? _updateService;

    /// <summary>
    /// Gets the title of the application window.
    /// </summary>
    public string Title { get; } = $"{Program.Name} v{Program.VersionString}";

    /// <summary>
    /// Gets the dashboard view model.
    /// </summary>
    public DashboardViewModel Dashboard { get; }

    /// <summary>
    /// Gets the snackbar manager.
    /// </summary>
    public SnackbarManager SnackbarManager { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    /// <param name="viewModelManager">The view model manager.</param>
    /// <param name="snackbarManager">The snackbar manager.</param>
    /// <param name="settingsService">The settings service.</param>
    /// <param name="updateService">The update service.</param>
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

    /// <summary>
    /// Initializes the view model.
    /// </summary>
    [RelayCommand]
    private async Task InitializeAsync()
    {
        await ShowUkraineSupportMessageAsync();
        await ShowDevelopmentBuildMessageAsync();
        await ShowFFmpegMessageAsync();
        // Temporarily disabled until first GitHub release is published
        // await CheckForUpdatesAsync();
    }

    /// <inheritdoc />
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