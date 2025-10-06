using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Shared.Interfaces;

namespace EnhancedYoutubeDownloader.Services;

/// <summary>
/// Notification service that integrates with SnackbarManager for UI feedback.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly SnackbarManager _snackbarManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class.
    /// </summary>
    /// <param name="snackbarManager">The snackbar manager.</param>
    public NotificationService(SnackbarManager snackbarManager)
    {
        _snackbarManager = snackbarManager;
    }

    /// <inheritdoc />
    public void ShowSuccess(string message)
    {
        _snackbarManager.NotifySuccess(message);
    }

    /// <inheritdoc />
    public void ShowError(string message)
    {
        _snackbarManager.NotifyError(message);
    }

    /// <inheritdoc />
    public void ShowWarning(string message)
    {
        _snackbarManager.NotifyWarning(message);
    }

    /// <inheritdoc />
    public void ShowInfo(string message)
    {
        _snackbarManager.NotifyInfo(message);
    }

    /// <inheritdoc />
    public void ShowProgress(string message, double progress)
    {
        _snackbarManager.NotifyProgress(message, progress);
    }
}