using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace EnhancedYoutubeDownloader.Framework;

/// <summary>
/// Manages Material Design snackbar notifications with queue support
/// </summary>
public partial class SnackbarManager : ObservableObject
{
    private readonly SemaphoreSlim _queueLock = new(1, 1);
    private bool _isProcessingQueue;

    /// <summary>
    /// Collection of active snackbar items. The first item is currently displayed.
    /// </summary>
    public ObservableCollection<SnackbarItem> SnackbarQueue { get; } = new();

    /// <summary>
    /// Shows a general notification snackbar
    /// </summary>
    public void Notify(string message, string? actionText = null, Action? action = null)
    {
        NotifyInfo(message, actionText, action);
    }

    /// <summary>
    /// Shows a success notification
    /// </summary>
    public void NotifySuccess(string message, string? actionText = null, Action? action = null)
    {
        EnqueueSnackbar(
            new SnackbarItem
            {
                Message = message,
                Severity = SnackbarSeverity.Success,
                ActionText = actionText,
                Action = action,
                Duration = TimeSpan.FromSeconds(3),
            }
        );
    }

    /// <summary>
    /// Shows an error notification
    /// </summary>
    public void NotifyError(string message, string? actionText = null, Action? action = null)
    {
        EnqueueSnackbar(
            new SnackbarItem
            {
                Message = message,
                Severity = SnackbarSeverity.Error,
                ActionText = actionText,
                Action = action,
                Duration = TimeSpan.FromSeconds(5),
            }
        );
    }

    /// <summary>
    /// Shows a warning notification
    /// </summary>
    public void NotifyWarning(string message, string? actionText = null, Action? action = null)
    {
        EnqueueSnackbar(
            new SnackbarItem
            {
                Message = message,
                Severity = SnackbarSeverity.Warning,
                ActionText = actionText,
                Action = action,
                Duration = TimeSpan.FromSeconds(4),
            }
        );
    }

    /// <summary>
    /// Shows an info notification
    /// </summary>
    public void NotifyInfo(string message, string? actionText = null, Action? action = null)
    {
        EnqueueSnackbar(
            new SnackbarItem
            {
                Message = message,
                Severity = SnackbarSeverity.Info,
                ActionText = actionText,
                Action = action,
                Duration = TimeSpan.FromSeconds(3),
            }
        );
    }

    /// <summary>
    /// Shows a progress notification that persists until dismissed
    /// </summary>
    public void NotifyProgress(string message, double progress)
    {
        // Check if there's already a progress notification and update it
        var existingProgress =
            SnackbarQueue.Count > 0 && SnackbarQueue[0].IsProgress ? SnackbarQueue[0] : null;

        if (existingProgress != null)
        {
            existingProgress.Message = message;
            existingProgress.Progress = progress;
        }
        else
        {
            EnqueueSnackbar(
                new SnackbarItem
                {
                    Message = message,
                    Severity = SnackbarSeverity.Info,
                    IsProgress = true,
                    Progress = progress,
                    Duration = null, // Persists until dismissed
                }
            );
        }
    }

    /// <summary>
    /// Dismisses the currently displayed snackbar
    /// </summary>
    public void DismissCurrent()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            if (SnackbarQueue.Count > 0)
                SnackbarQueue.RemoveAt(0);
        }
        else
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (SnackbarQueue.Count > 0)
                    SnackbarQueue.RemoveAt(0);
            });
        }
    }

    private void EnqueueSnackbar(SnackbarItem item)
    {
        // Ensure we're on the UI thread
        if (Dispatcher.UIThread.CheckAccess())
        {
            SnackbarQueue.Add(item);
            ProcessQueueAsync().ConfigureAwait(false);
        }
        else
        {
            Dispatcher.UIThread.Post(() =>
            {
                SnackbarQueue.Add(item);
                ProcessQueueAsync().ConfigureAwait(false);
            });
        }
    }

    private async Task ProcessQueueAsync()
    {
        // Prevent multiple queue processors
        if (_isProcessingQueue)
            return;

        await _queueLock.WaitAsync();
        try
        {
            _isProcessingQueue = true;

            while (SnackbarQueue.Count > 0)
            {
                var current = SnackbarQueue[0];

                // If no duration specified (progress notification), wait indefinitely
                if (current.Duration == null)
                {
                    // Progress notifications are dismissed manually
                    return;
                }

                // Wait for the duration
                await Task.Delay(current.Duration.Value);

                // Remove from queue (on UI thread)
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (SnackbarQueue.Count > 0 && SnackbarQueue[0] == current)
                        SnackbarQueue.RemoveAt(0);
                });
            }
        }
        finally
        {
            _isProcessingQueue = false;
            _queueLock.Release();
        }
    }
}

/// <summary>
/// Represents a snackbar notification item
/// </summary>
public partial class SnackbarItem : ObservableObject
{
    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private SnackbarSeverity _severity;

    [ObservableProperty]
    private string? _actionText;

    [ObservableProperty]
    private Action? _action;

    [ObservableProperty]
    private TimeSpan? _duration;

    [ObservableProperty]
    private bool _isProgress;

    [ObservableProperty]
    private double _progress;
}

/// <summary>
/// Snackbar severity levels with corresponding Material Design colors
/// </summary>
public enum SnackbarSeverity
{
    Info,
    Success,
    Warning,
    Error,
}
