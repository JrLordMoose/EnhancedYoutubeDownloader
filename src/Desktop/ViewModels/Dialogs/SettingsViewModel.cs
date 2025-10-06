using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Services;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

/// <summary>
/// The view model for the settings dialog.
/// </summary>
public partial class SettingsViewModel : DialogViewModelBase
{
    /// <summary>
    /// Gets or sets the settings service.
    /// </summary>
    [ObservableProperty]
    private SettingsService _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
    /// </summary>
    /// <param name="settingsService">The settings service.</param>
    public SettingsViewModel(SettingsService settingsService)
    {
        _settings = settingsService;
    }

    /// <summary>
    /// Saves the settings and closes the dialog.
    /// </summary>
    [RelayCommand]
    private void Save()
    {
        Settings.Save();
        Close(true);
    }

    /// <summary>
    /// Cancels the changes and closes the dialog.
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        Close(false);
    }
}