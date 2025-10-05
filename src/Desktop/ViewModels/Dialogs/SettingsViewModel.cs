using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Services;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

public partial class SettingsViewModel : DialogViewModelBase
{
    [ObservableProperty]
    private SettingsService _settings;

    public SettingsViewModel(SettingsService settingsService)
    {
        _settings = settingsService;
    }

    [RelayCommand]
    private void Save()
    {
        Settings.Save();
        Close(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        Close(false);
    }
}
