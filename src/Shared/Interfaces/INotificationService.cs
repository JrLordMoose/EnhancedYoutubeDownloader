namespace EnhancedYoutubeDownloader.Shared.Interfaces;

public interface INotificationService
{
    void ShowSuccess(string message);
    void ShowError(string message);
    void ShowWarning(string message);
    void ShowInfo(string message);
    void ShowProgress(string message, double progress);
}