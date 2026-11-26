using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnhancedYoutubeDownloader.Core.Models;
using EnhancedYoutubeDownloader.Core.Utils;
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
        TraceLog.Write($"[DASHBOARD] Constructor: IsBusy initialized to {IsBusy}");

        // Force property change notification to ensure UI updates
        OnPropertyChanged(nameof(IsBusy));

        // Subscribe to download status changes. Events are raised on background (thread
        // pool) threads by the download service; anything that touches UI (dialogs, file
        // pickers, notifications) must be marshalled to the UI thread or Avalonia throws
        // and the fire-and-forget task swallows it (the sign-in prompt never appeared).
        _eventRoot.Add(
            _downloadService.DownloadStatusChanged.Subscribe(downloadItem =>
                Dispatcher.UIThread.Post(() => OnDownloadStatusChanged(downloadItem))
            )
        );
    }

    // Completed events can repeat (post-processing re-raises them); act on each item once
    private readonly HashSet<string> _completionHandled = new();

    private void OnDownloadStatusChanged(DownloadItem downloadItem)
    {
        TraceLog.Write(
            $"[DASHBOARD] Download status changed: {downloadItem.Id} - {downloadItem.Status} - {downloadItem.Progress:F1}%"
        );

        if (downloadItem.Status == DownloadStatus.Completed)
        {
            if (!_completionHandled.Add(downloadItem.Id))
                return;

            if (_settingsService.NotifyOnComplete)
                _snackbarManager.NotifySuccess($"Downloaded: {downloadItem.Title}");

            if (_settingsService.OpenFolderAfterDownload)
            {
                TraceLog.Write("[DASHBOARD] Download completed, auto-opening folder");
                OpenDownloadFolder(downloadItem);
            }

            return;
        }

        // A restarted item may complete again later
        _completionHandled.Remove(downloadItem.Id);

        // Sign-in walls (YouTube bot check, Instagram/Reddit login, 403) and unreadable
        // browser cookies: offer a way to authenticate and retry. Only one prompt at a time,
        // so a failing playlist doesn't stack dialogs.
        if (
            downloadItem.Status == DownloadStatus.Failed
            && YtDlpErrorClassifier.NeedsAuthentication(downloadItem.ErrorMessage)
            && Interlocked.CompareExchange(ref _authPromptOpen, 1, 0) == 0
        )
        {
            TraceLog.Write("[DASHBOARD] Sign-in error detected, showing cookie prompt");
            _ = ShowAuthenticationPromptAsync(downloadItem);
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProgressIndeterminate))]
    [NotifyCanExecuteChangedFor(nameof(ProcessQueryCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowTutorialCommand))]
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

        TraceLog.Write("[DEBUG] ProcessQueryAsync: Starting");
        TraceLog.Write($"[DEBUG] IsBusy before: {IsBusy}");
        IsBusy = true;
        TraceLog.Write($"[DEBUG] IsBusy after setting true: {IsBusy}");

        try
        {
            // Clear any previous errors
            QueryError = null;

            TraceLog.Write($"[DEBUG] Resolving query: {Query}");
            _snackbarManager.Notify($"Resolving: {Query}");

            // Resolve the query using QueryResolver
            var result = await _queryResolver.ResolveAsync(Query);
            TraceLog.Write($"[DEBUG] Query resolved: Kind={result.Kind}");

            // Handle the result based on its kind
            switch (result.Kind)
            {
                case QueryResultKind.Video:
                    TraceLog.Write("[DEBUG] Processing single video");
                    await ProcessSingleVideoAsync(result);
                    break;

                case QueryResultKind.Playlist:
                case QueryResultKind.Channel:
                case QueryResultKind.Search:
                    TraceLog.Write("[DEBUG] Processing multiple videos");
                    await ProcessMultipleVideosAsync(result);
                    break;
            }

            // Clear the query after successful processing
            Query = null;
            TraceLog.Write("[DEBUG] Query cleared successfully");
        }
        catch (Exception ex)
        {
            TraceLog.Write($"[DEBUG] Exception caught: {ex.GetType().Name} - {ex.Message}");
            TraceLog.Write($"[DEBUG] Stack trace: {ex.StackTrace}");
            _snackbarManager.NotifyError($"Failed to resolve query: {ex.Message}");
            await ShowErrorDialogAsync(ex, "Query Resolution Failed");
        }
        finally
        {
            TraceLog.Write($"[DEBUG] Finally block: Setting IsBusy to false");
            IsBusy = false;
            TraceLog.Write($"[DEBUG] IsBusy after finally: {IsBusy}");
        }
    }

    private async Task ProcessSingleVideoAsync(QueryResult result)
    {
        try
        {
            TraceLog.Write("[DEBUG] ProcessSingleVideoAsync: Starting");

            if (result.Video == null)
            {
                TraceLog.Write("[DEBUG] result.Video is null, returning");
                return;
            }

            // Placeholder metadata ("YouTube video", "Video from TikTok"): ask yt-dlp for the
            // real title/channel/thumbnail. It uses the user's cookies, so it also works when
            // YouTube's bot check blocks YoutubeExplode.
            var video = result.Video;
            if (video is GenericVideo)
            {
                _snackbarManager.NotifyInfo("Fetching video details...");
                var info = await _downloadService.TryFetchVideoInfoAsync(video.Url);
                if (info != null)
                {
                    video = info;
                }
            }

            TraceLog.Write($"[DEBUG] Video: {video.Title}");

            // Show download setup dialog
            var setupDialog = _viewModelManager.CreateDownloadSingleSetupViewModel();
            setupDialog.Video = video;

            // Apply the user's defaults from Settings > Downloads (before FilePath, whose
            // extension follows SelectedFormat)
            setupDialog.SelectedQuality = GetDefaultQualityOption(_settingsService.DefaultQuality);
            setupDialog.SelectedFormat = GetDefaultFormatOption(_settingsService.DefaultFormat);
            setupDialog.DownloadSubtitles = _settingsService.ShouldInjectSubtitles;
            setupDialog.InjectTags = _settingsService.ShouldInjectTags;

            // Set default download path
            var sanitizedTitle = SanitizeFileName(video.Title);
            var defaultPath = !string.IsNullOrWhiteSpace(_settingsService.DefaultDownloadPath)
                ? _settingsService.DefaultDownloadPath
                : Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

            // Use default format extension from setupDialog (defaults to "MP4")
            var defaultExtension = setupDialog.SelectedFormat.ToLower();
            setupDialog.FilePath = Path.Combine(
                defaultPath,
                $"{sanitizedTitle}.{defaultExtension}"
            );
            TraceLog.Write($"[DEBUG] Default download path set to: {setupDialog.FilePath}");

            var dialogResult = await _dialogManager.ShowDialogAsync(setupDialog);

            // Check if user cancelled the dialog
            if (dialogResult != true || string.IsNullOrWhiteSpace(setupDialog.FilePath))
            {
                TraceLog.Write("[DEBUG] User cancelled download dialog");
                _snackbarManager.NotifyInfo("Download cancelled");
                return;
            }

            TraceLog.Write($"[DEBUG] Download path: {setupDialog.FilePath}");

            // Create FormatProfile from dialog settings
            var formatProfile = CreateFormatProfile(
                setupDialog.SelectedQuality,
                setupDialog.SelectedFormat,
                setupDialog.DownloadSubtitles,
                setupDialog.InjectTags
            );

            // The extension follows the actual output ("Audio Only" always saves .mp3), and
            // the path must be unique on disk AND among downloads still in the queue (adding
            // the same video twice used to give both items the same file).
            var requestedPath = Path.ChangeExtension(
                setupDialog.FilePath,
                "." + formatProfile.Container
            );
            var uniqueFilePath = GetUniqueBatchFilePath(requestedPath, GetActiveDownloadPaths());
            if (uniqueFilePath != setupDialog.FilePath)
            {
                TraceLog.Write($"[DEBUG] Using unique path: {uniqueFilePath}");
            }
            TraceLog.Write(
                $"[DEBUG] Format profile: {formatProfile.Quality} {formatProfile.Container}, Subs={formatProfile.IncludeSubtitles}, Tags={formatProfile.IncludeTags}"
            );

            // Create download item
            TraceLog.Write("[DEBUG] Creating download item");
            var downloadItem = await _downloadService.CreateDownloadAsync(
                video,
                uniqueFilePath,
                formatProfile,
                result.Platform
            );
            TraceLog.Write($"[DEBUG] Download item created: {downloadItem.Id}");

            Downloads.Add(downloadItem);
            TraceLog.Write("[DEBUG] Added to Downloads collection");

            // Start download based on settings
            TraceLog.Write(
                $"[DEBUG] AutoStartDownload setting: {_settingsService.AutoStartDownload}"
            );
            if (_settingsService.AutoStartDownload)
            {
                TraceLog.Write("[DEBUG] Calling StartDownloadAsync...");
                await _downloadService.StartDownloadAsync(downloadItem);
                TraceLog.Write("[DEBUG] Download started automatically");
                _snackbarManager.Notify($"Downloading: {video.Title}");
            }
            else
            {
                TraceLog.Write("[DEBUG] Download queued, waiting for manual start");
                _snackbarManager.Notify($"Ready to download: {video.Title}");
            }
        }
        catch (Exception ex)
        {
            TraceLog.Write(
                $"[DEBUG] Exception in ProcessSingleVideoAsync: {ex.GetType().Name} - {ex.Message}"
            );
            TraceLog.Write($"[DEBUG] Stack trace: {ex.StackTrace}");
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
            dialog.Title = result.Title ?? "Select videos to download";
            dialog.DownloadDirectory = !string.IsNullOrWhiteSpace(
                _settingsService.DefaultDownloadPath
            )
                ? _settingsService.DefaultDownloadPath
                : Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            dialog.DownloadSubtitles = _settingsService.ShouldInjectSubtitles;
            dialog.InjectTags = _settingsService.ShouldInjectTags;
            dialog.SelectedQuality = GetDefaultQualityOption(_settingsService.DefaultQuality);
            dialog.SelectedFormat = GetDefaultFormatOption(_settingsService.DefaultFormat);
            dialog.SetVideos(result.Videos);

            var dialogResult = await _dialogManager.ShowDialogAsync(dialog);

            if (dialogResult == true)
            {
                var formatProfile = CreateFormatProfile(
                    dialog.SelectedQuality,
                    dialog.SelectedFormat,
                    dialog.DownloadSubtitles,
                    dialog.InjectTags
                );
                var reservedPaths = GetActiveDownloadPaths();

                // User confirmed, create downloads for selected videos
                foreach (var video in dialog.SelectedVideos)
                {
                    var extension = formatProfile.Container;
                    var requestedPath = Path.Combine(
                        dialog.DownloadDirectory,
                        $"{SanitizeFileName(video.Title)}.{extension}"
                    );
                    var downloadPath = GetUniqueBatchFilePath(requestedPath, reservedPaths);

                    var downloadItem = await _downloadService.CreateDownloadAsync(
                        video,
                        downloadPath,
                        formatProfile,
                        result.Platform
                    );
                    Downloads.Add(downloadItem);
                    if (_settingsService.AutoStartDownload)
                    {
                        await _downloadService.StartDownloadAsync(downloadItem);
                    }
                }

                _snackbarManager.Notify($"Added {dialog.SelectedVideos.Count} videos to queue");
            }
        }
        catch (Exception ex)
        {
            _snackbarManager.NotifyError($"Failed to process videos: {ex.Message}");
            await ShowErrorDialogAsync(ex, "Multiple Download Failed");
        }
    }

    /// <summary>Output paths of downloads that haven't finished, so new ones never collide.</summary>
    private HashSet<string> GetActiveDownloadPaths() =>
        new(
            Downloads
                .Where(d =>
                    d.Status
                        is DownloadStatus.Queued
                            or DownloadStatus.Started
                            or DownloadStatus.Paused
                    && !string.IsNullOrEmpty(d.FilePath)
                )
                // Reserve by name WITHOUT extension: yt-dlp temp files (X.temp.mp4, X.f137.mp4,
                // X.webm before MP3 extraction) collide even when X.mp4 and X.mp3 differ.
                .Select(d => Path.ChangeExtension(d.FilePath!, null)),
            StringComparer.OrdinalIgnoreCase
        );

    private static string GetUniqueBatchFilePath(
        string requestedPath,
        HashSet<string> reservedPaths
    )
    {
        var candidate = DownloadSingleSetupViewModel.GetUniqueFilePath(requestedPath);
        var directory = Path.GetDirectoryName(candidate) ?? string.Empty;
        var fileName = Path.GetFileNameWithoutExtension(candidate);
        var extension = Path.GetExtension(candidate);
        var suffix = 1;

        while (
            !reservedPaths.Add(Path.ChangeExtension(candidate, null))
            || (candidate != requestedPath && File.Exists(candidate))
        )
        {
            candidate = Path.Combine(directory, $"{fileName} ({suffix++}){extension}");
        }

        return candidate;
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

    /// <summary>
    /// Maps Settings > Downloads "Default Quality" index to the setup dialogs' option text.
    /// </summary>
    internal static string GetDefaultQualityOption(int index) =>
        index switch
        {
            1 => "1080p (Full HD)",
            2 => "720p (HD)",
            3 => "480p (SD)",
            4 => "360p",
            5 => "Audio Only (Best)",
            _ => "Best Quality",
        };

    /// <summary>
    /// Maps Settings > Downloads "Default Format" index to the setup dialogs' option text.
    /// </summary>
    internal static string GetDefaultFormatOption(int index) =>
        index switch
        {
            1 => "WebM",
            2 => "MP3",
            _ => "MP4",
        };

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

        // "Audio Only" with MP4/WebM produced Opus/M4A audio under a .mp4/.webm name;
        // audio-only downloads are always MP3.
        if (qualityValue == "audio-only")
        {
            containerValue = "mp3";
        }

        var profile = new FormatProfile
        {
            Quality = qualityValue,
            Container = containerValue,
            IncludeSubtitles = includeSubtitles,
            IncludeTags = includeTags,
            // Copy subtitle style setting from SettingsService
            SubtitleStyle = _settingsService.SubtitleStyle,
        };

        TraceLog.Write(
            $"[FORMAT] Created profile with subtitle settings: Style={profile.SubtitleStyle}"
        );

        return profile;
    }

    [RelayCommand]
    private void OpenDownloadFolder(DownloadItem download)
    {
        TraceLog.Write($"[OPEN] OpenDownloadFolder called for {download.Id}");
        TraceLog.Write($"[OPEN] FilePath: {download.FilePath}");
        TraceLog.Write($"[OPEN] CanOpen: {download.CanOpen}");
        TraceLog.Write($"[OPEN] Status: {download.Status}");

        if (string.IsNullOrWhiteSpace(download.FilePath) || !File.Exists(download.FilePath))
        {
            TraceLog.Write($"[OPEN] File not found or FilePath is null/empty");
            _snackbarManager.NotifyWarning("File not found");
            return;
        }

        try
        {
            TraceLog.Write($"[OPEN] Opening file location: {download.FilePath}");

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
                TraceLog.Write("[OPEN] Explorer launched successfully");
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
            TraceLog.Write($"[OPEN] Error: {ex.GetType().Name}: {ex.Message}");
            TraceLog.Write($"[OPEN] Stack trace: {ex.StackTrace}");
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
            {
                // Settings > Downloads holds the default download location
                var settings = _viewModelManager.CreateSettingsViewModel();
                settings.SelectedTabIndex = 1;
                await _dialogManager.ShowDialogAsync(settings);
                break;
            }

            case "check_video":
                if (!string.IsNullOrWhiteSpace(Query) && Query.Contains("://"))
                    OpenUrl(Query);
                else
                    _snackbarManager.NotifyInfo("Open the video in your browser to check it");
                break;

            case "try_format":
                // Re-open the download dialog so a different quality/format can be picked
                await ProcessQueryAsync();
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

    private int _authPromptOpen;

    private async Task ShowAuthenticationPromptAsync(DownloadItem downloadItem)
    {
        try
        {
            var kind = YtDlpErrorClassifier.Classify(downloadItem.ErrorMessage);
            var firefoxInstalled = BrowserCookieHelper.IsFirefoxInstalled();
            TraceLog.Write(
                $"[AUTH-PROMPT] {kind} for {downloadItem.Id}; Firefox installed: {firefoxInstalled}"
            );

            var dialog = _viewModelManager.CreateMessageBoxViewModel();
            dialog.Title =
                kind == YtDlpErrorKind.CookieReadFailed
                    ? "Couldn't Read Browser Cookies"
                    : "Sign-In Required";

            var reason =
                kind == YtDlpErrorKind.CookieReadFailed
                    ? "Your browser's cookies couldn't be read. On Windows, Chrome, Edge and Brave "
                        + "encrypt them so other apps can't use them."
                    : "This site blocked the download until you're signed in "
                        + "(YouTube shows this as \"confirm you're not a bot\").";

            var useFirefox = firefoxInstalled && kind != YtDlpErrorKind.CookieReadFailed;
            dialog.Message = useFirefox
                ? reason
                    + "\n\nYou have Firefox installed. Sign in to the site in Firefox, then retry "
                    + "using Firefox's cookies. You can also pick a cookies.txt file in "
                    + "Settings > Advanced > Authentication."
                : reason
                    + "\n\nExport a cookies.txt file from a browser where you're signed in "
                    + "(for example with the \"Get cookies.txt LOCALLY\" extension), then choose "
                    + "that file to retry. Your cookies stay on this computer.";
            dialog.PrimaryButtonText = useFirefox ? "Use Firefox & Retry" : "Choose cookies.txt";
            dialog.SecondaryButtonText = "Not Now";
            dialog.HelpLinkText = "How do I get a cookies.txt file? (step-by-step guide)";
            dialog.HelpLinkUrl = BrowserCookieHelper.GuideUrl;

            var result = await _dialogManager.ShowDialogAsync(dialog);
            if (result != true)
            {
                TraceLog.Write("[AUTH-PROMPT] User declined");
                _snackbarManager.NotifyInfo(
                    "You can set this up later in Settings > Advanced > Authentication"
                );
                return;
            }

            if (useFirefox)
            {
                _settingsService.UseBrowserCookies = true;
                _settingsService.BrowserForCookies = "firefox";
            }
            else
            {
                var path = await _dialogManager.PromptOpenFilePathAsync(
                    "Choose cookies.txt",
                    "Cookies file",
                    "*.txt"
                );
                if (string.IsNullOrWhiteSpace(path))
                {
                    _snackbarManager.NotifyInfo("No cookies file selected");
                    return;
                }

                _settingsService.CookiesFilePath = path;
            }

            _settingsService.Save();
            _snackbarManager.NotifySuccess("Authentication saved. Retrying download...");
            await _downloadService.RestartDownloadAsync(downloadItem);
        }
        finally
        {
            Interlocked.Exchange(ref _authPromptOpen, 0);
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
            TraceLog.Write($"[DASHBOARD] Failed to open URL: {ex.Message}");
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
