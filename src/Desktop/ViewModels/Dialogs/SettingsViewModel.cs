using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private readonly UpdateCoordinator _updateCoordinator;

    [ObservableProperty]
    private SettingsService _settings;

    // Snapshot for rollback on Cancel
    // Snapshot of every persisted setting, restored on Cancel. Reflection-based so a newly
    // added setting can never be forgotten here again.
    private readonly Dictionary<PropertyInfo, object?> _snapshot = new();
    private readonly string _originalCachePath;

    // Order must match the ComboBoxItems in SettingsDialog.axaml
    private static readonly string[] BrowserOptions =
    [
        "chrome",
        "firefox",
        "edge",
        "opera",
        "brave",
        "chromium",
    ];

    private static readonly string[] AiLanguageOptions =
    [
        "auto",
        "en",
        "es",
        "fr",
        "de",
        "it",
        "pt",
        "ja",
        "ko",
        "zh",
    ];

    /// <summary>
    /// ComboBox index for <see cref="SettingsService.BrowserForCookies"/>. The dialog's items are
    /// ComboBoxItems, so binding SelectedValue straight to the string never stored a browser name.
    /// </summary>
    public int BrowserIndex
    {
        get =>
            Math.Max(
                0,
                Array.IndexOf(BrowserOptions, Settings.BrowserForCookies?.ToLowerInvariant())
            );
        set
        {
            if (value < 0 || value >= BrowserOptions.Length)
                return;
            Settings.BrowserForCookies = BrowserOptions[value];
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Maps the Theme ComboBox (Auto, Light, Dark) to <see cref="ThemeVariant"/>
    /// (Light=0, Dark=1, Auto=2). Binding SelectedIndex straight to the enum made
    /// "Auto" pick Light and "Light" pick Dark.
    /// </summary>
    public int ThemeIndex
    {
        get => ThemeToIndex(Settings.Theme);
        set
        {
            Settings.Theme = IndexToTheme(value);
            OnPropertyChanged();
        }
    }

    internal static int ThemeToIndex(ThemeVariant theme) =>
        theme switch
        {
            ThemeVariant.Light => 1,
            ThemeVariant.Dark => 2,
            _ => 0,
        };

    internal static ThemeVariant IndexToTheme(int index) =>
        index switch
        {
            1 => ThemeVariant.Light,
            2 => ThemeVariant.Dark,
            _ => ThemeVariant.Auto,
        };

    // Order must match the Subtitle Language ComboBoxItems in SettingsDialog.axaml
    private static readonly string[] SubtitleLanguageOptions =
    [
        "en",
        "es",
        "fr",
        "de",
        "it",
        "pt",
        "ja",
        "ko",
        "zh",
        "all",
    ];

    /// <summary>ComboBox index for <see cref="SettingsService.SubtitleLanguage"/>.</summary>
    public int SubtitleLanguageIndex
    {
        get => Math.Max(0, Array.IndexOf(SubtitleLanguageOptions, Settings.SubtitleLanguage));
        set
        {
            if (value < 0 || value >= SubtitleLanguageOptions.Length)
                return;
            Settings.SubtitleLanguage = SubtitleLanguageOptions[value];
            OnPropertyChanged();
        }
    }

    /// <summary>ComboBox index for <see cref="SettingsService.AISubtitleLanguage"/>.</summary>
    public int AiSubtitleLanguageIndex
    {
        get => Math.Max(0, Array.IndexOf(AiLanguageOptions, Settings.AISubtitleLanguage));
        set
        {
            if (value < 0 || value >= AiLanguageOptions.Length)
                return;
            Settings.AISubtitleLanguage = AiLanguageOptions[value];
            OnPropertyChanged();
        }
    }

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
        UpdateCoordinator updateCoordinator
    )
    {
        _settings = settingsService;
        _dialogManager = dialogManager;
        _viewModelManager = viewModelManager;
        _cacheService = cacheService;
        _updateCoordinator = updateCoordinator;

        // Snapshot current settings for rollback on Cancel
        _originalCachePath = Settings.DefaultCachePath;
        foreach (var property in PersistedSettings)
            _snapshot[property] = property.GetValue(Settings);

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

        RestoreSnapshot();
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
            OnPropertyChanged(nameof(ActualDownloadPath));
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
            OnPropertyChanged(nameof(ActualCachePath));
        }
        else
        {
            TraceLog.Write($"[SETTINGS] Browse canceled, no path selected.");
        }
    }

    /// <summary>The folder the app runs from; updates are installed here.</summary>
    public string InstallLocation { get; } =
        Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory);

    // Inno Setup puts the uninstaller next to the app (UninstallFilesDir={app}, renamed
    // to uninstall.exe). Dev builds have none, so the button is hidden there.
    private string UninstallerPath => Path.Combine(InstallLocation, "uninstall.exe");

    public bool CanUninstall => OperatingSystem.IsWindows() && File.Exists(UninstallerPath);

    [RelayCommand]
    private void OpenInstallFolder()
    {
        try
        {
            if (OperatingSystem.IsWindows())
                System.Diagnostics.Process.Start("explorer.exe", $"\"{InstallLocation}\"");
            else if (OperatingSystem.IsMacOS())
                System.Diagnostics.Process.Start("open", $"\"{InstallLocation}\"");
            else
                System.Diagnostics.Process.Start("xdg-open", $"\"{InstallLocation}\"");
        }
        catch (Exception ex)
        {
            TraceLog.Write($"[SETTINGS] Could not open install folder: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task UninstallAsync()
    {
        if (!CanUninstall)
            return;

        // One dialog at a time: close Settings before asking
        Close(false);

        var confirm = new MessageBoxViewModel
        {
            Title = "Uninstall Enhanced YouTube Downloader?",
            Message =
                "The uninstaller will open and this app will close.\n\n"
                + "Your downloaded videos are not deleted. The uninstaller asks whether to "
                + "also remove your settings and cache.",
            PrimaryButtonText = "Uninstall",
            SecondaryButtonText = "Cancel",
        };

        if (await _dialogManager.ShowDialogAsync(confirm) != true)
        {
            await ReopenSettingsAsync(2);
            return;
        }

        System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo(UninstallerPath) { UseShellExecute = true }
        );

        // The uninstaller can't remove files while the app is running
        if (
            Avalonia.Application.Current?.ApplicationLifetime
            is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
        )
            desktop.Shutdown(0);
        else
            Environment.Exit(0);
    }

    [RelayCommand]
    private async Task BrowseCookiesFileAsync()
    {
        var path = await _dialogManager.PromptOpenFilePathAsync(
            "Choose cookies.txt",
            "Cookies file",
            "*.txt"
        );

        if (!string.IsNullOrWhiteSpace(path))
        {
            // Never log the file's contents; the path alone is enough to debug
            TraceLog.Write($"[SETTINGS] Cookies file set: {path}");
            Settings.CookiesFilePath = path;
        }
    }

    [RelayCommand]
    private void ClearCookiesFile()
    {
        TraceLog.Write("[SETTINGS] Cookies file cleared");
        Settings.CookiesFilePath = string.Empty;
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

        // DialogManager shows one dialog at a time: asking for confirmation while Settings is
        // open waited forever. Discard unsaved edits, close, confirm, then reopen Settings.
        RestoreSnapshot();
        var tab = SelectedTabIndex;
        Close(false);

        var result = await _dialogManager.ShowDialogAsync(messageBox);

        if (result == true)
        {
            TraceLog.Write("[SETTINGS] Reset All Settings confirmed. Resetting to defaults.");

            // Cogwheel resets every property to its default; keep the one-time migration flag
            Settings.Reset();
            Settings.AutoStartMigrationDone = true;
            Settings.Save();
            TraceLog.Write("[SETTINGS] Reset settings saved to disk.");
        }
        else
        {
            TraceLog.Write($"[SETTINGS] Reset All Settings canceled by user.");
        }

        await ReopenSettingsAsync(tab);
    }

    /// <summary>
    /// Every persisted setting: public, read/write, declared on SettingsService.
    /// </summary>
    private static readonly PropertyInfo[] PersistedSettings = typeof(SettingsService)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .Where(p => p.CanRead && p.CanWrite)
        .ToArray();

    private void RestoreSnapshot()
    {
        foreach (var (property, value) in _snapshot)
            property.SetValue(Settings, value);

        OnPropertyChanged(nameof(ThemeIndex));
        OnPropertyChanged(nameof(BrowserIndex));
        OnPropertyChanged(nameof(AiSubtitleLanguageIndex));
        OnPropertyChanged(nameof(SubtitleLanguageIndex));
        OnPropertyChanged(nameof(ActualDownloadPath));
        OnPropertyChanged(nameof(ActualCachePath));
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
            // The cache service opened its database at startup from the then-saved
            // cache location (changes need a restart), so measure that file.
            var cacheDir = string.IsNullOrWhiteSpace(_originalCachePath)
                ? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "EnhancedYoutubeDownloader"
                )
                : _originalCachePath;
            var cacheDbPath = Path.Combine(cacheDir, "cache.db");

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
        if (!_updateCoordinator.IsSupported)
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
            var newVersion = await _updateCoordinator.CheckForUpdatesAsync();

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

                // Same flow as the startup check. v0.4.7 launched the updater here but never
                // exited, so the updater timed out after ~15s and nothing was installed.
                if (await _updateCoordinator.PromptAsync(newVersion))
                {
                    TraceLog.Write($"[SETTINGS] User accepted update v{newVersion}");
                    await _updateCoordinator.InstallNowAsync(newVersion);
                }
                else
                {
                    TraceLog.Write("[SETTINGS] User chose Later.");
                    UpdateStatus =
                        $"Update v{newVersion.ToString(3)} will install when you close the app";
                    _ = _updateCoordinator.ScheduleInstallOnExitAsync(newVersion);
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

    private async Task ReopenSettingsAsync(int tabIndex = 2)
    {
        var settings = _viewModelManager.CreateSettingsViewModel();
        settings.SelectedTabIndex = tabIndex;
        await _dialogManager.ShowDialogAsync(settings);
    }

    private bool CanCheckForUpdates() => !IsCheckingForUpdates && _updateCoordinator.IsSupported;

    [RelayCommand]
    private void GoToUpdateCheck()
    {
        TraceLog.Write("[SETTINGS] Navigating to Advanced tab (Update Check section)");
        SelectedTabIndex = 2; // Advanced tab is index 2
    }
}
