using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Shared.Interfaces;

namespace EnhancedYoutubeDownloader.Services;

/// <summary>
/// Notification service that integrates with SnackbarManager for UI feedback
/// </summary>
public class NotificationService : INotificationService
{
    private readonly SnackbarManager _snackbarManager;

    public NotificationService(SnackbarManager snackbarManager)
    {
        _snackbarManager = snackbarManager;
    }

    public void ShowSuccess(string message)
    {
        _snackbarManager.NotifySuccess(message);
    }

    public void ShowError(string message)
    {
        _snackbarManager.NotifyError(message);
    }

    public void ShowWarning(string message)
    {
        _snackbarManager.NotifyWarning(message);
    }

    public void ShowInfo(string message)
    {
        _snackbarManager.NotifyInfo(message);
    }

    public void ShowProgress(string message, double progress)
    {
        _snackbarManager.NotifyProgress(message, progress);
    }
}
