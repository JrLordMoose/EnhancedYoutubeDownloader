namespace EnhancedYoutubeDownloader.Shared.Interfaces;

/// <summary>
/// Defines the contract for a service that displays notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Shows a success notification.
    /// </summary>
    /// <param name="message">The message to display.</param>
    void ShowSuccess(string message);

    /// <summary>
    /// Shows an error notification.
    /// </summary>
    /// <param name="message">The message to display.</param>
    void ShowError(string message);

    /// <summary>
    /// Shows a warning notification.
    /// </summary>
    /// <param name="message">The message to display.</param>
    void ShowWarning(string message);

    /// <summary>
    /// Shows an informational notification.
    /// </summary>
    /// <param name="message">The message to display.</param>
    void ShowInfo(string message);

    /// <summary>
    /// Shows a progress notification.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="progress">The progress value (0-100).</param>
    void ShowProgress(string message, double progress);
}