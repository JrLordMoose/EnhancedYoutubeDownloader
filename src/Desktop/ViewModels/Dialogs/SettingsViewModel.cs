using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Services;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

public partial class SettingsViewModel : DialogViewModelBase
{
    private readonly DialogManager _dialogManager;

    [ObservableProperty]
    private SettingsService _settings;

    public SettingsViewModel(SettingsService settingsService, DialogManager dialogManager)
    {
        _settings = settingsService;
        _dialogManager = dialogManager;
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

    [RelayCommand]
    private async Task BrowseDefaultLocationAsync()
    {
        var selectedPath = await _dialogManager.PromptFolderPathAsync(
            "Select Default Download Location",
            Settings.DefaultDownloadPath
        );

        if (!string.IsNullOrWhiteSpace(selectedPath))
        {
            Settings.DefaultDownloadPath = selectedPath;
        }
    }
}
