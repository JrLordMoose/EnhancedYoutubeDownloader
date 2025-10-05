using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Services;
using EnhancedYoutubeDownloader.Shared.Interfaces;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

public partial class SettingsViewModel : DialogViewModelBase
{
    private readonly DialogManager _dialogManager;
    private readonly ICacheService _cacheService;

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
        ICacheService cacheService
    )
    {
        _settings = settingsService;
        _dialogManager = dialogManager;
        _cacheService = cacheService;

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

        Console.WriteLine(
            $"[SETTINGS] Settings dialog initialized. Current values snapshot taken."
        );

        // Calculate cache size on startup
        _ = CalculateCacheSizeAsync();
    }

    [RelayCommand]
    private void Save()
    {
        Console.WriteLine($"[SETTINGS] Saving settings to disk.");
        Settings.Save();
        Console.WriteLine($"[SETTINGS] Settings saved successfully.");
        Close(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        Console.WriteLine($"[SETTINGS] Cancel clicked. Rolling back changes to original values.");

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

        Console.WriteLine($"[SETTINGS] All settings rolled back to original state.");

        Close(false);
    }

    [RelayCommand]
    private async Task BrowseDefaultLocationAsync()
    {
        Console.WriteLine($"[SETTINGS] Browse Default Download Location clicked.");
        var selectedPath = await _dialogManager.PromptFolderPathAsync(
            "Select Default Download Location",
            Settings.DefaultDownloadPath
        );

        if (!string.IsNullOrWhiteSpace(selectedPath))
        {
            Console.WriteLine($"[SETTINGS] Default download path set to: {selectedPath}");
            Settings.DefaultDownloadPath = selectedPath;
        }
        else
        {
            Console.WriteLine($"[SETTINGS] Browse canceled, no path selected.");
        }
    }

    [RelayCommand]
    private async Task BrowseCacheLocationAsync()
    {
        Console.WriteLine($"[SETTINGS] Browse Cache Location clicked.");
        var selectedPath = await _dialogManager.PromptFolderPathAsync(
            "Select Cache Location",
            Settings.DefaultCachePath
        );

        if (!string.IsNullOrWhiteSpace(selectedPath))
        {
            Console.WriteLine($"[SETTINGS] Cache path set to: {selectedPath}");
            Settings.DefaultCachePath = selectedPath;
        }
        else
        {
            Console.WriteLine($"[SETTINGS] Browse canceled, no path selected.");
        }
    }

    [RelayCommand]
    private void ResetCacheLocation()
    {
        Console.WriteLine($"[SETTINGS] Reset Cache Location to default.");
        Settings.DefaultCachePath = string.Empty;
        OnPropertyChanged(nameof(ActualCachePath));
        Console.WriteLine($"[SETTINGS] Cache path reset to: {ActualCachePath}");
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
            Console.WriteLine(
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

            Console.WriteLine($"[SETTINGS] All settings reset to defaults.");

            // Save settings
            Settings.Save();
            Console.WriteLine($"[SETTINGS] Reset settings saved to disk.");

            // Update computed properties
            OnPropertyChanged(nameof(ActualDownloadPath));
            OnPropertyChanged(nameof(ActualCachePath));

            // Recalculate cache size
            await CalculateCacheSizeAsync();
        }
        else
        {
            Console.WriteLine($"[SETTINGS] Reset All Settings canceled by user.");
        }
    }

    [RelayCommand]
    private async Task ClearCacheAsync()
    {
        Console.WriteLine($"[SETTINGS] Clear Cache clicked. Starting cache clear operation.");
        CacheSize = "Clearing...";
        await _cacheService.ClearCacheAsync();
        Console.WriteLine($"[SETTINGS] Cache cleared successfully.");
        await CalculateCacheSizeAsync();
        Console.WriteLine($"[SETTINGS] Cache size recalculated: {CacheSize}");
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
}
