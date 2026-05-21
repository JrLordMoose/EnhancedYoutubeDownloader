using System;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Services;
using EnhancedYoutubeDownloader.Shared.Interfaces;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

using EnhancedYoutubeDownloader.Utils;

public partial class SettingsViewModel : DialogViewModelBase
{
    private readonly DialogManager _dialogManager;
    private readonly ViewModelManager _viewModelManager;
    private readonly ICacheService _cacheService;
    private readonly UpdateService? _updateService;

    [ObservableProperty]
    private SettingsService _settings;

    // Snapshot for rollback on Cancel
    private string? _originalDownloadPath;
    private string? _originalCachePath;
    private ThemeVariant _originalTheme;
    private bool _originalAutoUpdate;
    private int _originalParallelLimit;
    private int _originalQuality;
    private int _originalFormat;
    private bool _originalSubtitles;
    private bool _originalTags;
    private bool _originalAutoStart;
    private bool _originalOpenFolder;
    private bool _originalLanguageAudio;

    [ObservableProperty]
    private string _cacheSize = "Calculating...";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckForUpdatesCommand))]
    private bool _isCheckingForUpdates = false;

    [ObservableProperty]
    private string _updateStatus = "No updates checked";

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    public string CurrentVersion =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "Unknown";

    public string ActualDownloadPath =>
        string.IsNullOrWhiteSpace(Settings.DefaultDownloadPath)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                "Downloads"
            )
            : Settings.DefaultDownloadPath;

    public string ActualCachePath =>
        string.IsNullOrWhiteSpace(Settings.DefaultCachePath)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EnhancedYoutubeDownloader"
            )
            : Settings.DefaultCachePath;

    public SettingsViewModel(
        SettingsService settingsService,
        DialogManager dialogManager,
        ViewModelManager viewModelManager,
        ICacheService cacheService,
        UpdateService? updateService = null
    )
    {
        _settings = settingsService;
        _dialogManager = dialogManager;
        _viewModelManager = viewModelManager;
        _cacheService = cacheService;
        _updateService = updateService;

        // Snapshot current settings for rollback on Cancel
        _originalDownloadPath = Settings.DefaultDownloadPath;
        _originalCachePath = Settings.DefaultCachePath;
        _originalTheme = Settings.Theme;
        _originalAutoUpdate = Settings.IsAutoUpdateEnabled;
        _originalParallelLimit = Settings.ParallelLimit;
        _originalQuality = Settings.DefaultQuality;
        _originalFormat = Settings.DefaultFormat;
        _originalSubtitles = Settings.ShouldInjectSubtitles;
        _originalTags = Settings.ShouldInjectTags;
        _originalAutoStart = Settings.AutoStartDownload;
        _originalOpenFolder = Settings.OpenFolderAfterDownload;
        _originalLanguageAudio = Settings.ShouldInjectLanguageSpecificAudioStreams;

        TraceLog.Write($"[SETTINGS] Settings dialog initialized. Current values snapshot taken.");

        // Calculate cache size on startup
        _ = CalculateCacheSizeAsync();
    }

    [RelayCommand]
    private void Save()
    {
        TraceLog.Write($"[SETTINGS] Saving settings to disk.");
        Settings.Save();
        TraceLog.Write($"[SETTINGS] Settings saved successfully.");
        Close(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        TraceLog.Write($"[SETTINGS] Cancel clicked. Rolling back changes to original values.");

        // Rollback all settings to original snapshot values
        Settings.DefaultDownloadPath = _originalDownloadPath ?? string.Empty;
        Settings.DefaultCachePath = _originalCachePath ?? string.Empty;
        Settings.Theme = _originalTheme;
        Settings.IsAutoUpdateEnabled = _originalAutoUpdate;
        Settings.ParallelLimit = _originalParallelLimit;
        Settings.DefaultQuality = _originalQuality;
        Settings.DefaultFormat = _originalFormat;
        Settings.ShouldInjectSubtitles = _originalSubtitles;
        Settings.ShouldInjectTags = _originalTags;
        Settings.AutoStartDownload = _originalAutoStart;
        Settings.OpenFolderAfterDownload = _originalOpenFolder;
        Settings.ShouldInjectLanguageSpecificAudioStreams = _originalLanguageAudio;

        TraceLog.Write($"[SETTINGS] All settings rolled back to original state.");

        Close(false);
    }

    [RelayCommand]
    private async Task BrowseDefaultLocationAsync()
    {
        TraceLog.Write($"[SETTINGS] Browse Default Download Location clicked.");
        var selectedPath = await _dialogManager.PromptFolderPathAsync(
            "Select Default Download Location",
            Settings.DefaultDownloadPath
        );

        if (!string.IsNullOrWhiteSpace(selectedPath))
        {
            TraceLog.Write($"[SETTINGS] Default download path set to: {selectedPath}");
            Settings.DefaultDownloadPath = selectedPath;
        }
        else
        {
            TraceLog.Write($"[SETTINGS] Browse canceled, no path selected.");
        }
    }

    [RelayCommand]
    private async Task BrowseCacheLocationAsync()
    {
        TraceLog.Write($"[SETTINGS] Browse Cache Location clicked.");
        var selectedPath = await _dialogManager.PromptFolderPathAsync(
            "Select Cache Location",
            Settings.DefaultCachePath
        );

        if (!string.IsNullOrWhiteSpace(selectedPath))
        {
            TraceLog.Write($"[SETTINGS] Cache path set to: {selectedPath}");
            Settings.DefaultCachePath = selectedPath;
        }
        else
        {
            TraceLog.Write($"[SETTINGS] Browse canceled, no path selected.");
        }
    }

    [RelayCommand]
    private void ResetCacheLocation()
    {
        TraceLog.Write($"[SETTINGS] Reset Cache Location to default.");
        Settings.DefaultCachePath = string.Empty;
        OnPropertyChanged(nameof(ActualCachePath));
        TraceLog.Write($"[SETTINGS] Cache path reset to: {ActualCachePath}");
    }

    [RelayCommand]
    private async Task ResetAllSettingsAsync()
    {
        var messageBox = new MessageBoxViewModel
        {
            Title = "Reset All Settings",
            Message =
                "Are you sure you want to reset all settings to their default values? This action cannot be undone.",
            PrimaryButtonText = "Yes",
            SecondaryButtonText = "No",
        };

        var result = await _dialogManager.ShowDialogAsync(messageBox);

        if (result == true)
        {
            TraceLog.Write(
                $"[SETTINGS] Reset All Settings confirmed. Resetting to default values."
            );

            // Reset all settings to defaults
            Settings.Theme = ThemeVariant.Auto;
            Settings.IsAutoUpdateEnabled = true;
            Settings.ParallelLimit = 3;
            Settings.DefaultQuality = 0; // Best Quality
            Settings.DefaultFormat = 0; // MP4
            Settings.ShouldInjectSubtitles = true; // FIXED: Changed from false to true
            Settings.ShouldInjectTags = true;
            Settings.AutoStartDownload = true;
            Settings.OpenFolderAfterDownload = false;
            Settings.DefaultDownloadPath = string.Empty;
            Settings.DefaultCachePath = string.Empty;
            Settings.ShouldInjectLanguageSpecificAudioStreams = true; // FIXED: Changed from false to true

            TraceLog.Write($"[SETTINGS] All settings reset to defaults.");

            // Save settings
            Settings.Save();
            TraceLog.Write($"[SETTINGS] Reset settings saved to disk.");

            // Update computed properties
            OnPropertyChanged(nameof(ActualDownloadPath));
            OnPropertyChanged(nameof(ActualCachePath));

            // Recalculate cache size
            await CalculateCacheSizeAsync();
        }
        else
        {
            TraceLog.Write($"[SETTINGS] Reset All Settings canceled by user.");
        }
    }

    [RelayCommand]
    private async Task ClearCacheAsync()
    {
        TraceLog.Write($"[SETTINGS] Clear Cache clicked. Starting cache clear operation.");
        CacheSize = "Clearing...";
        await _cacheService.ClearCacheAsync();
        TraceLog.Write($"[SETTINGS] Cache cleared successfully.");
        await CalculateCacheSizeAsync();
        TraceLog.Write($"[SETTINGS] Cache size recalculated: {CacheSize}");
    }

    private async Task CalculateCacheSizeAsync()
    {
        try
        {
            // Get the cache database path
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var cacheDbPath = Path.Combine(appDataPath, "EnhancedYoutubeDownloader", "cache.db");

            if (File.Exists(cacheDbPath))
            {
                var fileInfo = new FileInfo(cacheDbPath);
                var sizeInBytes = fileInfo.Length;

                // Format size appropriately
                if (sizeInBytes < 1024)
                {
                    CacheSize = $"{sizeInBytes} B";
                }
                else if (sizeInBytes < 1024 * 1024)
                {
                    CacheSize = $"{sizeInBytes / 1024.0:F2} KB";
                }
                else if (sizeInBytes < 1024 * 1024 * 1024)
                {
                    CacheSize = $"{sizeInBytes / (1024.0 * 1024.0):F2} MB";
                }
                else
                {
                    CacheSize = $"{sizeInBytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
                }
            }
            else
            {
                CacheSize = "0 B";
            }
        }
        catch (Exception)
        {
            CacheSize = "Unknown";
        }

        await Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanCheckForUpdates))]
    [SupportedOSPlatform("windows")]
    private async Task CheckForUpdatesAsync()
    {
        if (_updateService == null)
        {
            UpdateStatus = "Update service not available";
            TraceLog.Write("[SETTINGS] Update service is not available (Windows-only)");
            return;
        }

        TraceLog.Write("[SETTINGS] Checking for updates...");
        IsCheckingForUpdates = true;
        UpdateStatus = "Checking for updates...";

        try
        {
            var newVersion = await _updateService.CheckForUpdatesAsync();

            if (newVersion == null)
            {
                TraceLog.Write("[SETTINGS] No updates available. Already on latest version.");
                UpdateStatus = "You're on the latest version!";

                // Close Settings dialog first to avoid deadlock (DialogManager uses SemaphoreSlim)
                Close(false);

                // Show info dialog
                var infoDialog = new MessageBoxViewModel
                {
                    Title = "No Updates Available",
                    Message = $"You're already running the latest version ({CurrentVersion}).",
                    PrimaryButtonText = "OK",
                };
                await _dialogManager.ShowDialogAsync(infoDialog);
                await ReopenSettingsAsync();
            }
            else
            {
                TraceLog.Write($"[SETTINGS] Update available: v{newVersion}");
                UpdateStatus = $"Update available: v{newVersion}";

                // Close Settings dialog first to avoid deadlock (DialogManager uses SemaphoreSlim)
                Close(false);

                // Show update prompt
                var updatePrompt = new MessageBoxViewModel
                {
                    Title = "Update Available",
                    Message =
                        $"A new version is available!\n\nCurrent version: {CurrentVersion}\nNew version: {newVersion}\n\nWould you like to download and install the update? The application will restart.",
                    PrimaryButtonText = "Yes, Update",
                    SecondaryButtonText = "Not Now",
                };

                var result = await _dialogManager.ShowDialogAsync(updatePrompt);

                if (result == true)
                {
                    TraceLog.Write(
                        $"[SETTINGS] User accepted update. Preparing update v{newVersion}..."
                    );
                    UpdateStatus = "Downloading update...";

                    // Download the update package
                    await _updateService.PrepareUpdateAsync(newVersion);

                    TraceLog.Write(
                        "[SETTINGS] Update downloaded. Launching updater and restarting..."
                    );
                    UpdateStatus = "Restarting...";

                    // Launch updater and restart
                    _updateService.FinalizeUpdate(newVersion, restart: true);
                }
                else
                {
                    TraceLog.Write("[SETTINGS] User declined update.");
                    UpdateStatus = $"Update available: v{newVersion}";
                    await ReopenSettingsAsync();
                }
            }
        }
        catch (Exception ex)
        {
            TraceLog.Write($"[SETTINGS] Error checking for updates: {ex.Message}");
            UpdateStatus = "Failed to check for updates";

            // Close Settings dialog first to avoid deadlock (DialogManager uses SemaphoreSlim)
            Close(false);

            var errorDialog = new MessageBoxViewModel
            {
                Title = "Update Check Failed",
                Message = $"Failed to check for updates.\n\nError: {ex.Message}",
                PrimaryButtonText = "OK",
            };
            await _dialogManager.ShowDialogAsync(errorDialog);
            await ReopenSettingsAsync();
        }
        finally
        {
            IsCheckingForUpdates = false;
        }
    }

    private async Task ReopenSettingsAsync()
    {
        var settings = _viewModelManager.CreateSettingsViewModel();
        settings.SelectedTabIndex = 2;
        await _dialogManager.ShowDialogAsync(settings);
    }

    private bool CanCheckForUpdates() => !IsCheckingForUpdates && _updateService != null;

    [RelayCommand]
    private void GoToUpdateCheck()
    {
        TraceLog.Write("[SETTINGS] Navigating to Advanced tab (Update Check section)");
        SelectedTabIndex = 2; // Advanced tab is index 2
    }
}
