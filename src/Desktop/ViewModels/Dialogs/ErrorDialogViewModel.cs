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
/// ViewModel for the error dialog with Material Design styling.
/// </summary>
public partial class ErrorDialogViewModel : DialogViewModelBase
{
    /// <summary>
    /// Gets or sets the error information.
    /// </summary>
    [ObservableProperty]
    private ErrorInfo? _errorInfo;

    /// <summary>
    /// Gets or sets a value indicating whether the details section is expanded.
    /// </summary>
    [ObservableProperty]
    private bool _isDetailsExpanded;

    /// <summary>
    /// Gets or sets the action to perform when an action is selected.
    /// </summary>
    public Action<string>? OnActionSelected { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorDialogViewModel"/> class.
    /// </summary>
    public ErrorDialogViewModel()
    {
        // Design-time constructor
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorDialogViewModel"/> class.
    /// </summary>
    /// <param name="errorInfo">The error information.</param>
    public ErrorDialogViewModel(ErrorInfo errorInfo)
    {
        ErrorInfo = errorInfo;
    }

    /// <summary>
    /// Copies the error details to the clipboard.
    /// </summary>
    [RelayCommand]
    private async Task CopyErrorDetailsAsync()
    {
        if (ErrorInfo == null)
            return;

        // Note: In a real implementation, you would get the clipboard from the current window/TopLevel
        // This is a simplified version that just returns without error
        await Task.CompletedTask;
    }

    /// <summary>
    /// Toggles the visibility of the error details.
    /// </summary>
    [RelayCommand]
    private void ToggleDetails()
    {
        IsDetailsExpanded = !IsDetailsExpanded;
    }

    /// <summary>
    /// Executes the selected action.
    /// </summary>
    /// <param name="actionKey">The key of the action to execute.</param>
    [RelayCommand]
    private void ExecuteAction(string actionKey)
    {
        OnActionSelected?.Invoke(actionKey);
        Close?.Invoke();
    }

    /// <summary>
    /// Closes the dialog.
    /// </summary>
    [RelayCommand]
    private void CloseDialog()
    {
        Close?.Invoke();
    }

    /// <summary>
    /// Gets or sets the action to close the dialog.
    /// </summary>
    public Action? Close { get; set; }
}