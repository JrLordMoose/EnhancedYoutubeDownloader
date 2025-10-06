using System;
using CommunityToolkit.Mvvm.ComponentModel;
using EnhancedYoutubeDownloader.Framework;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

/// <summary>
/// The view model for the message box dialog.
/// </summary>
public partial class MessageBoxViewModel : DialogViewModelBase
{
    /// <summary>
    /// Gets or sets the title of the message box.
    /// </summary>
    [ObservableProperty]
    private string _title = string.Empty;

    /// <summary>
    /// Gets or sets the message of the message box.
    /// </summary>
    [ObservableProperty]
    private string _message = string.Empty;

    /// <summary>
    /// Gets or sets the text of the primary button.
    /// </summary>
    [ObservableProperty]
    private string _primaryButtonText = "OK";

    /// <summary>
    /// Gets or sets the text of the secondary button.
    /// </summary>
    [ObservableProperty]
    private string _secondaryButtonText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBoxViewModel"/> class.
    /// </summary>
    public MessageBoxViewModel() { }
}