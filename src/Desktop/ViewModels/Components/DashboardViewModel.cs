using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Services;
using EnhancedYoutubeDownloader.Shared.Interfaces;
using EnhancedYoutubeDownloader.Shared.Models;
using EnhancedYoutubeDownloader.Utils;
using EnhancedYoutubeDownloader.Utils.Extensions;
using EnhancedYoutubeDownloader.ViewModels.Dialogs;

namespace EnhancedYoutubeDownloader.ViewModels.Components;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly ViewModelManager _viewModelManager;
    private readonly SnackbarManager _snackbarManager;
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;
    private readonly IDownloadService _downloadService;
    private readonly IQueryResolver _queryResolver;
    private readonly DisposableCollector _eventRoot = new();

    public DashboardViewModel(
        ViewModelManager viewModelManager,
        SnackbarManager snackbarManager,
        DialogManager dialogManager,
        SettingsService settingsService,
        IDownloadService downloadService,
        IQueryResolver queryResolver
    )
    {
        _viewModelManager = viewModelManager;
        _snackbarManager = snackbarManager;
        _dialogManager = dialogManager;
        _settingsService = settingsService;
        _downloadService = downloadService;
        _queryResolver = queryResolver;

        // Explicitly initialize IsBusy to false
        IsBusy = false;
        Console.WriteLine($"[DASHBOARD] Constructor: IsBusy initialized to {IsBusy}");

        // Force property change notification to ensure UI updates
        OnPropertyChanged(nameof(IsBusy));

        _eventRoot.Add(
            _settingsService.WatchProperty(
                o => o.ParallelLimit,
                () => OnPropertyChanged(nameof(IsProgressIndeterminate)),
                true
            )
        );

        // Subscribe to download status changes
        _eventRoot.Add(
            _downloadService.DownloadStatusChanged.Subscribe(downloadItem =>
            {
                Console.WriteLine(
                    $"[DASHBOARD] Download status changed: {downloadItem.Id} - {downloadItem.Status} - {downloadItem.Progress:F1}%"
                );
                // No need to trigger collection change - DownloadItem already implements INotifyPropertyChanged
                // and individual property changes will automatically update the UI

                // Check if download completed and auto-open folder is enabled
                if (
                    downloadItem.Status == DownloadStatus.Completed
                    && _settingsService.OpenFolderAfterDownload
                )
                {
                    Console.WriteLine(
                        $"[DASHBOARD] Download completed, auto-opening folder (setting enabled)"
                    );
                    OpenDownloadFolder(downloadItem);
                }

                // Check for 403 errors and offer to enable browser cookies
                if (
                    downloadItem.Status == DownloadStatus.Failed
                    && !string.IsNullOrEmpty(downloadItem.ErrorMessage)
                    && (
                        downloadItem.ErrorMessage.Contains("403")
                        || downloadItem.ErrorMessage.Contains("Forbidden")
                    )
                    && !_settingsService.UseBrowserCookies
                )
                {
                    Console.WriteLine(
                        $"[DASHBOARD] 403 error detected, showing browser cookie prompt"
                    );
                    _ = Show403BrowserCookiePromptAsync(downloadItem);
                }
            })
        );
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProgressIndeterminate))]
    [NotifyCanExecuteChangedFor(nameof(ProcessQueryCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowTutorialCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowAuthSetupCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowSettingsCommand))]
    public partial bool IsBusy { get; set; }

    public bool IsProgressIndeterminate =>
        IsBusy && Downloads.Any(d => d.Status == DownloadStatus.Started);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ProcessQueryCommand))]
    public partial string? Query { get; set; }

    [ObservableProperty]
    public partial string? QueryError { get; set; }

    public ObservableCollection<DownloadItem> Downloads { get; } = [];

    private bool CanShowTutorial() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanShowTutorial))]
    private async Task ShowTutorialAsync() =>
        await _dialogManager.ShowDialogAsync(_viewModelManager.CreateTutorialViewModel());

    private bool CanShowAuthSetup() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanShowAuthSetup))]
    private async Task ShowAuthSetupAsync() =>
        await _dialogManager.ShowDialogAsync(_viewModelManager.CreateAuthSetupViewModel());

    private bool CanShowSettings() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanShowSettings))]
    private async Task ShowSettingsAsync() =>
        await _dialogManager.ShowDialogAsync(_viewModelManager.CreateSettingsViewModel());

    private bool CanProcessQuery() => !IsBusy && !string.IsNullOrWhiteSpace(Query);

    private bool IsValidQuery(string query)
    {
        // Clear previous error
        QueryError = null;

        // Check if it's a URL-like string or potential search query
        if (
            query.StartsWith("#")
            || query.StartsWith("@")
                && !query.Contains("youtube.com")
                && !query.Contains("instagram.com")
                && !query.Contains("tiktok.com")
        )
        {
            QueryError = "Invalid URL. Please enter a valid video URL or search term.";
            return false;
        }

        // Check for very short random strings (less than 3 chars and no spaces)
        if (query.Length < 3 && !query.Contains(" "))
        {
            QueryError = "Invalid input. Please enter a valid video URL or search term.";
            return false;
        }

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanProcessQuery))]
    private async Task ProcessQueryAsync()
    {
        if (string.IsNullOrWhiteSpace(Query))
            return;

        // Validate query before processing
        if (!IsValidQuery(Query))
        {
            _snackbarManager.NotifyError(QueryError!);
            return;
        }

        Console.WriteLine("[DEBUG] ProcessQueryAsync: Starting");
        Console.WriteLine($"[DEBUG] IsBusy before: {IsBusy}");
        IsBusy = true;
        Console.WriteLine($"[DEBUG] IsBusy after setting true: {IsBusy}");

        try
        {
            // Clear any previous errors
            QueryError = null;

            Console.WriteLine($"[DEBUG] Resolving query: {Query}");
            _snackbarManager.Notify($"Resolving: {Query}");

            // Resolve the query using QueryResolver
            var result = await _queryResolver.ResolveAsync(Query);
            Console.WriteLine($"[DEBUG] Query resolved: Kind={result.Kind}");

            // Handle the result based on its kind
            switch (result.Kind)
            {
                case QueryResultKind.Video:
                    Console.WriteLine("[DEBUG] Processing single video");
                    await ProcessSingleVideoAsync(result);
                    break;

                case QueryResultKind.Playlist:
                case QueryResultKind.Channel:
                case QueryResultKind.Search:
                    Console.WriteLine("[DEBUG] Processing multiple videos");
                    await ProcessMultipleVideosAsync(result);
                    break;
            }

            // Clear the query after successful processing
            Query = null;
            Console.WriteLine("[DEBUG] Query cleared successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Exception caught: {ex.GetType().Name} - {ex.Message}");
            Console.WriteLine($"[DEBUG] Stack trace: {ex.StackTrace}");
            _snackbarManager.NotifyError($"Failed to resolve query: {ex.Message}");
            await ShowErrorDialogAsync(ex, "Query Resolution Failed");
        }
        finally
        {
            Console.WriteLine($"[DEBUG] Finally block: Setting IsBusy to false");
            IsBusy = false;
            Console.WriteLine($"[DEBUG] IsBusy after finally: {IsBusy}");
        }
    }

    private async Task ProcessSingleVideoAsync(QueryResult result)
    {
        try
        {
            Console.WriteLine("[DEBUG] ProcessSingleVideoAsync: Starting");

            if (result.Video == null)
            {
                Console.WriteLine("[DEBUG] result.Video is null, returning");
                return;
            }

            Console.WriteLine($"[DEBUG] Video: {result.Video.Title}");

            // Show download setup dialog
            var setupDialog = _viewModelManager.CreateDownloadSingleSetupViewModel();
            setupDialog.Video = result.Video;

            // Set default download path
            var sanitizedTitle = SanitizeFileName(result.Video.Title);
            var defaultPath = !string.IsNullOrWhiteSpace(_settingsService.DefaultDownloadPath)
                ? _settingsService.DefaultDownloadPath
                : Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

            // Use default format extension from setupDialog (defaults to "MP4")
            var defaultExtension = setupDialog.SelectedFormat.ToLower();
            setupDialog.FilePath = Path.Combine(
                defaultPath,
                $"{sanitizedTitle}.{defaultExtension}"
            );
            Console.WriteLine($"[DEBUG] Default download path set to: {setupDialog.FilePath}");

            var dialogResult = await _dialogManager.ShowDialogAsync(setupDialog);

            // Check if user cancelled the dialog
            if (dialogResult != true || string.IsNullOrWhiteSpace(setupDialog.FilePath))
            {
                Console.WriteLine("[DEBUG] User cancelled download dialog");
                _snackbarManager.NotifyInfo("Download cancelled");
                return;
            }

            Console.WriteLine($"[DEBUG] Download path: {setupDialog.FilePath}");

            // Ensure unique file path to prevent overwriting existing files
            var uniqueFilePath = DownloadSingleSetupViewModel.GetUniqueFilePath(
                setupDialog.FilePath
            );
            if (uniqueFilePath != setupDialog.FilePath)
            {
                Console.WriteLine(
                    $"[DEBUG] File already exists, using unique path: {uniqueFilePath}"
                );
            }

            // Create FormatProfile from dialog settings
            var formatProfile = CreateFormatProfile(
                setupDialog.SelectedQuality,
                setupDialog.SelectedFormat,
                setupDialog.DownloadSubtitles,
                setupDialog.InjectTags
            );
            Console.WriteLine(
                $"[DEBUG] Format profile: {formatProfile.Quality} {formatProfile.Container}, Subs={formatProfile.IncludeSubtitles}, Tags={formatProfile.IncludeTags}"
            );

            // Create download item
            Console.WriteLine("[DEBUG] Creating download item");
            var downloadItem = await _downloadService.CreateDownloadAsync(
                result.Video,
                uniqueFilePath,
                formatProfile,
                result.Platform
            );
            Console.WriteLine($"[DEBUG] Download item created: {downloadItem.Id}");

            Downloads.Add(downloadItem);
            Console.WriteLine("[DEBUG] Added to Downloads collection");

            // Start download based on settings
            Console.WriteLine(
                $"[DEBUG] AutoStartDownload setting: {_settingsService.AutoStartDownload}"
            );
            if (_settingsService.AutoStartDownload)
            {
                Console.WriteLine("[DEBUG] Calling StartDownloadAsync...");
                await _downloadService.StartDownloadAsync(downloadItem);
                Console.WriteLine("[DEBUG] Download started automatically");
                _snackbarManager.Notify($"Downloading: {result.Video.Title}");
            }
            else
            {
                Console.WriteLine("[DEBUG] Download queued, waiting for manual start");
                _snackbarManager.Notify($"Ready to download: {result.Video.Title}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[DEBUG] Exception in ProcessSingleVideoAsync: {ex.GetType().Name} - {ex.Message}"
            );
            Console.WriteLine($"[DEBUG] Stack trace: {ex.StackTrace}");
            _snackbarManager.NotifyError($"Failed to start download: {ex.Message}");
            await ShowErrorDialogAsync(ex, "Download Failed");
        }
    }

    private async Task ProcessMultipleVideosAsync(QueryResult result)
    {
        try
        {
            // Check if we have any videos BEFORE showing the dialog
            if (result.Videos == null || !result.Videos.Any())
            {
                _snackbarManager.NotifyError("No videos found for this query");
                return;
            }

            // Show dialog for user to select videos and configure settings
            var dialog = _viewModelManager.CreateDownloadMultipleSetupViewModel();
            // Set dialog properties with result.Videos, result.Title, etc.

            var dialogResult = await _dialogManager.ShowDialogAsync(dialog);

            if (dialogResult == true)
            {
                // User confirmed, create downloads for selected videos
                foreach (var video in result.Videos)
                {
                    var downloadPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                        $"{video.Title}.mp4"
                    );

                    var downloadItem = await _downloadService.CreateDownloadAsync(
                        video,
                        downloadPath,
                        null,
                        result.Platform
                    );
                    Downloads.Add(downloadItem);
                    await _downloadService.StartDownloadAsync(downloadItem);
                }

                _snackbarManager.Notify($"Added {result.Videos.Count} videos to queue");
            }
        }
        catch (Exception ex)
        {
            _snackbarManager.NotifyError($"Failed to process videos: {ex.Message}");
            await ShowErrorDialogAsync(ex, "Multiple Download Failed");
        }
    }

    [RelayCommand]
    private void RemoveSuccessfulDownloads()
    {
        var completedDownloads = Downloads
            .Where(d => d.Status == DownloadStatus.Completed)
            .ToList();
        foreach (var download in completedDownloads)
        {
            Downloads.Remove(download);
        }
    }

    [RelayCommand]
    private void RemoveInactiveDownloads()
    {
        var inactiveDownloads = Downloads
            .Where(d =>
                d.Status == DownloadStatus.Completed
                || d.Status == DownloadStatus.Failed
                || d.Status == DownloadStatus.Canceled
            )
            .ToList();

        foreach (var download in inactiveDownloads)
        {
            Downloads.Remove(download);
        }
    }

    [RelayCommand]
    private async Task RestartDownloadAsync(DownloadItem download)
    {
        await _downloadService.RestartDownloadAsync(download);
    }

    [RelayCommand]
    private async Task PauseDownloadAsync(DownloadItem download)
    {
        await _downloadService.PauseDownloadAsync(download);
    }

    [RelayCommand]
    private async Task StartDownloadAsync(DownloadItem download)
    {
        await _downloadService.StartDownloadAsync(download);
    }

    [RelayCommand]
    private async Task ResumeDownloadAsync(DownloadItem download)
    {
        await _downloadService.ResumeDownloadAsync(download);
    }

    [RelayCommand]
    private async Task CancelDownloadAsync(DownloadItem download)
    {
        await _downloadService.CancelDownloadAsync(download);
    }

    [RelayCommand]
    private async Task DeleteDownloadAsync(DownloadItem download)
    {
        await _downloadService.DeleteDownloadAsync(download);
        Downloads.Remove(download);
    }

    private FormatProfile CreateFormatProfile(
        string quality,
        string format,
        bool includeSubtitles,
        bool includeTags
    )
    {
        // Map UI quality strings to internal values
        var qualityValue = quality switch
        {
            "Best Quality" => "highest",
            "1080p (Full HD)" => "1080p",
            "720p (HD)" => "720p",
            "480p (SD)" => "480p",
            "360p" => "360p",
            "Audio Only (Best)" => "audio-only",
            _ => "highest",
        };

        // Map UI format strings to container values
        var containerValue = format switch
        {
            "MP4" => "mp4",
            "WebM" => "webm",
            "MP3" => "mp3",
            _ => "mp4",
        };

        var profile = new FormatProfile
        {
            Quality = qualityValue,
            Container = containerValue,
            IncludeSubtitles = includeSubtitles,
            IncludeTags = includeTags,
            // Copy subtitle style setting from SettingsService
            SubtitleStyle = _settingsService.SubtitleStyle,
        };

        Console.WriteLine(
            $"[FORMAT] Created profile with subtitle settings: Style={profile.SubtitleStyle}"
        );

        return profile;
    }

    [RelayCommand]
    private void OpenDownloadFolder(DownloadItem download)
    {
        Console.WriteLine($"[OPEN] OpenDownloadFolder called for {download.Id}");
        Console.WriteLine($"[OPEN] FilePath: {download.FilePath}");
        Console.WriteLine($"[OPEN] CanOpen: {download.CanOpen}");
        Console.WriteLine($"[OPEN] Status: {download.Status}");

        if (string.IsNullOrWhiteSpace(download.FilePath) || !File.Exists(download.FilePath))
        {
            Console.WriteLine($"[OPEN] File not found or FilePath is null/empty");
            _snackbarManager.NotifyWarning("File not found");
            return;
        }

        try
        {
            Console.WriteLine($"[OPEN] Opening file location: {download.FilePath}");

            // Cross-platform folder opening with proper ProcessStartInfo
            if (OperatingSystem.IsWindows())
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{download.FilePath}\"",
                    UseShellExecute = true,
                };
                System.Diagnostics.Process.Start(startInfo);
                Console.WriteLine("[OPEN] Explorer launched successfully");
            }
            else if (OperatingSystem.IsMacOS())
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "open",
                    Arguments = $"-R \"{download.FilePath}\"",
                    UseShellExecute = true,
                };
                System.Diagnostics.Process.Start(startInfo);
            }
            else if (OperatingSystem.IsLinux())
            {
                var directory = Path.GetDirectoryName(download.FilePath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "xdg-open",
                        Arguments = $"\"{directory}\"",
                        UseShellExecute = true,
                    };
                    System.Diagnostics.Process.Start(startInfo);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OPEN] Error: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"[OPEN] Stack trace: {ex.StackTrace}");
            _snackbarManager.NotifyError($"Failed to open folder: {ex.Message}");
        }
    }

    /// <summary>
    /// Shows an error dialog with categorized error information
    /// </summary>
    private async Task ShowErrorDialogAsync(Exception exception, string context)
    {
        var errorInfo = CategorizeError(exception, context);
        var errorDialog = _viewModelManager.CreateErrorDialogViewModel();

        // Use reflection or a property setter to set the ErrorInfo
        if (errorDialog is ViewModels.Dialogs.ErrorDialogViewModel typedDialog)
        {
            typedDialog.ErrorInfo = errorInfo;
            typedDialog.OnActionSelected = async (actionKey) =>
            {
                await HandleErrorActionAsync(actionKey, exception);
            };
        }

        await _dialogManager.ShowDialogAsync(errorDialog);
    }

    /// <summary>
    /// Categorizes exceptions into user-friendly error types
    /// </summary>
    private ErrorInfo CategorizeError(Exception exception, string context)
    {
        var category = exception switch
        {
            HttpRequestException => ErrorCategory.Network,
            UnauthorizedAccessException => ErrorCategory.Permission,
            ArgumentException when exception.Message.Contains("URL") => ErrorCategory.InvalidUrl,
            IOException => ErrorCategory.FileSystem,
            _ when exception.Message.Contains("YouTube") => ErrorCategory.YouTube,
            _
                when exception.Message.Contains("video")
                    && exception.Message.Contains("not available") =>
                ErrorCategory.VideoNotAvailable,
            _ => ErrorCategory.Unknown,
        };

        var suggestedActions = GetSuggestedActions(category);

        return new ErrorInfo
        {
            Message = $"{context}: {exception.Message}",
            Details = exception.ToString(),
            Category = category,
            SuggestedActions = suggestedActions,
        };
    }

    /// <summary>
    /// Gets suggested actions based on error category
    /// </summary>
    private List<ErrorAction> GetSuggestedActions(ErrorCategory category)
    {
        return category switch
        {
            ErrorCategory.Network =>
            [
                new ErrorAction
                {
                    Text = "Check Connection",
                    ActionKey = "check_connection",
                    Description = "Verify your internet connection",
                },
                new ErrorAction
                {
                    Text = "Retry",
                    ActionKey = "retry",
                    Description = "Try the operation again",
                },
            ],
            ErrorCategory.Permission =>
            [
                new ErrorAction
                {
                    Text = "Change Path",
                    ActionKey = "change_path",
                    Description = "Select a different download location",
                },
                new ErrorAction
                {
                    Text = "Run as Admin",
                    ActionKey = "run_admin",
                    Description = "Restart with administrator privileges",
                },
            ],
            ErrorCategory.InvalidUrl =>
            [
                new ErrorAction
                {
                    Text = "Correct URL",
                    ActionKey = "correct_url",
                    Description = "Check and correct the URL format",
                },
            ],
            ErrorCategory.YouTube or ErrorCategory.VideoNotAvailable =>
            [
                new ErrorAction
                {
                    Text = "Check Video",
                    ActionKey = "check_video",
                    Description = "Verify the video is available on YouTube",
                },
                new ErrorAction
                {
                    Text = "Try Different Format",
                    ActionKey = "try_format",
                    Description = "Select a different quality or format",
                },
            ],
            _ =>
            [
                new ErrorAction
                {
                    Text = "Submit Feedback",
                    ActionKey = "feedback_form",
                    Description = "Submit feedback via our simple form",
                },
                new ErrorAction
                {
                    Text = "Report Issue",
                    ActionKey = "report_github",
                    Description = "Report on GitHub (for developers)",
                },
            ],
        };
    }

    /// <summary>
    /// Sanitizes a filename by removing invalid characters.
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        // Get invalid filename characters
        var invalidChars = Path.GetInvalidFileNameChars();

        // Replace invalid characters with underscore
        foreach (var c in invalidChars)
        {
            fileName = fileName.Replace(c, '_');
        }

        // Also replace some additional problematic characters
        fileName = fileName
            .Replace(":", "_")
            .Replace("/", "_")
            .Replace("\\", "_")
            .Replace("|", "_")
            .Replace("?", "_")
            .Replace("*", "_")
            .Replace("\"", "_")
            .Replace("<", "_")
            .Replace(">", "_");

        // Trim whitespace and dots from the end
        fileName = fileName.TrimEnd('.', ' ');

        // Ensure the filename is not empty
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = "video";
        }

        // Truncate if too long (max 255 chars for most filesystems, leave room for extension)
        const int maxLength = 240;
        if (fileName.Length > maxLength)
        {
            fileName = fileName.Substring(0, maxLength);
        }

        return fileName;
    }

    /// <summary>
    /// Handles user-selected error actions
    /// </summary>
    private async Task HandleErrorActionAsync(string actionKey, Exception originalException)
    {
        switch (actionKey)
        {
            case "retry":
                _snackbarManager.NotifyInfo("Retrying operation...");
                await ProcessQueryAsync();
                break;

            case "check_connection":
                _snackbarManager.NotifyInfo("Please check your internet connection and try again");
                break;

            case "change_path":
                _snackbarManager.NotifyInfo("Path selection feature coming soon");
                break;

            case "correct_url":
                _snackbarManager.NotifyWarning("Please verify the URL and try again");
                break;

            case "feedback_form":
                OpenUrl(
                    "https://docs.google.com/forms/d/e/1FAIpQLSfgvSuqvusqpGBs6UUbUImzUB10YzROqomsvGFZhnp41VTeRg/viewform"
                );
                _snackbarManager.NotifyInfo("Opening feedback form in browser...");
                break;

            case "report_github":
                OpenUrl("https://github.com/JrLordMoose/EnhancedYoutubeDownloader/issues");
                _snackbarManager.NotifyInfo("Opening GitHub issues in browser...");
                break;

            case "report":
                // Legacy action key - redirect to feedback form
                OpenUrl(
                    "https://docs.google.com/forms/d/e/1FAIpQLSfgvSuqvusqpGBs6UUbUImzUB10YzROqomsvGFZhnp41VTeRg/viewform"
                );
                _snackbarManager.NotifyInfo("Opening feedback form in browser...");
                break;

            default:
                _snackbarManager.NotifyInfo($"Action '{actionKey}' not implemented yet");
                break;
        }
    }

    private async Task Show403BrowserCookiePromptAsync(DownloadItem downloadItem)
    {
        Console.WriteLine($"[403-PROMPT] Showing browser cookie prompt for {downloadItem.Id}");

        var dialog = _viewModelManager.CreateMessageBoxViewModel();
        dialog.Title = "Access Restricted (403 Forbidden)";
        dialog.Message =
            "This video requires authentication to download.\n\n"
            + "Would you like to enable browser cookie authentication?\n"
            + "This will use your logged-in YouTube session from Chrome.\n\n"
            + "Make sure you're logged into YouTube in Chrome and close all browser windows before retrying.";
        dialog.PrimaryButtonText = "Enable & Retry";
        dialog.SecondaryButtonText = "Not Now";

        var result = await _dialogManager.ShowDialogAsync(dialog);

        if (result == true)
        {
            // User clicked "Enable & Retry"
            Console.WriteLine($"[403-PROMPT] User chose to enable browser cookies");

            _settingsService.UseBrowserCookies = true;
            _settingsService.BrowserForCookies = "chrome"; // Default to Chrome
            _settingsService.Save();

            _snackbarManager.NotifySuccess("Browser cookies enabled! Retrying download...");

            // Retry the download
            await _downloadService.RestartDownloadAsync(downloadItem);
        }
        else
        {
            // User clicked "Not Now" or closed dialog
            Console.WriteLine($"[403-PROMPT] User declined browser cookies");
            _snackbarManager.NotifyInfo(
                "You can enable this later in Settings > Advanced > Authentication"
            );
        }
    }

    /// <summary>
    /// Opens a URL in the default browser (cross-platform)
    /// </summary>
    private static void OpenUrl(string url)
    {
        try
        {
            // Cross-platform URL opening
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DASHBOARD] Failed to open URL: {ex.Message}");
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _eventRoot.Dispose();
        }

        base.Dispose(disposing);
    }
}
