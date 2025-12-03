using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Shared.Models;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

/// <summary>
/// ViewModel for the error dialog with Material Design styling
/// </summary>
public partial class ErrorDialogViewModel : DialogViewModelBase
{
    [ObservableProperty]
    private ErrorInfo? _errorInfo;

    [ObservableProperty]
    private bool _isDetailsExpanded;

    public Action<string>? OnActionSelected { get; set; }

    public ErrorDialogViewModel()
    {
        // Design-time constructor
    }

    public ErrorDialogViewModel(ErrorInfo errorInfo)
    {
        ErrorInfo = errorInfo;
    }

    [RelayCommand]
    private async Task CopyErrorDetailsAsync()
    {
        if (ErrorInfo == null)
        {
            Console.WriteLine("[ERROR_DIALOG] CopyErrorDetails called but ErrorInfo is null.");
            return;
        }

        try
        {
            Console.WriteLine("[ERROR_DIALOG] Attempting to copy error details to clipboard.");

            // Get the main window from Application
            var mainWindow = Application.Current?.ApplicationLifetime
                is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow == null)
            {
                Console.WriteLine("[ERROR_DIALOG] Main window is not available.");
                return;
            }

            var clipboard = mainWindow.Clipboard;
            if (clipboard == null)
            {
                Console.WriteLine("[ERROR_DIALOG] Clipboard is not available.");
                return;
            }

            // Format error details for clipboard
            var errorText = $"Error: {ErrorInfo.Message}\n";
            if (!string.IsNullOrWhiteSpace(ErrorInfo.Details))
            {
                errorText += $"\nDetails:\n{ErrorInfo.Details}";
            }
            errorText += $"\nCategory: {ErrorInfo.Category}";

            // Copy to clipboard
            await clipboard.SetTextAsync(errorText);
            Console.WriteLine("[ERROR_DIALOG] Error details copied to clipboard successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR_DIALOG] Failed to copy to clipboard: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ToggleDetails()
    {
        IsDetailsExpanded = !IsDetailsExpanded;
    }

    [RelayCommand]
    private void ExecuteAction(string actionKey)
    {
        OnActionSelected?.Invoke(actionKey);
        Close(true);
    }

    [RelayCommand]
    private void CloseDialog()
    {
        Close(true);
    }
}
