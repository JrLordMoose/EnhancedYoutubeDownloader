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
            return;

        // Note: In a real implementation, you would get the clipboard from the current window/TopLevel
        // This is a simplified version that just returns without error
        await Task.CompletedTask;
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
        Close?.Invoke();
    }

    [RelayCommand]
    private void CloseDialog()
    {
        Close?.Invoke();
    }

    public Action? Close { get; set; }
}
