using System;
using CommunityToolkit.Mvvm.ComponentModel;
using EnhancedYoutubeDownloader.Framework;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

/// <summary>
/// The view model for the authentication setup dialog.
/// </summary>
public partial class AuthSetupViewModel : DialogViewModelBase
{
    /// <summary>
    /// Gets or sets the status of the authentication setup.
    /// </summary>
    [ObservableProperty]
    private string _status = "Ready";

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthSetupViewModel"/> class.
    /// </summary>
    public AuthSetupViewModel() { }
}