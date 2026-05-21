using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Services;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

public partial class AuthSetupViewModel : DialogViewModelBase
{
    private readonly DialogManager _dialogManager;
    private readonly ViewModelManager _viewModelManager;
    private readonly SettingsService _settings;

    [ObservableProperty]
    private string _status;

    public bool IsBrowserCookiesEnabled => _settings.UseBrowserCookies;

    public string BrowserLabel =>
        string.IsNullOrWhiteSpace(_settings.BrowserForCookies)
            ? "Not selected"
            : _settings.BrowserForCookies;

    public AuthSetupViewModel(
        DialogManager dialogManager,
        ViewModelManager viewModelManager,
        SettingsService settings
    )
    {
        _dialogManager = dialogManager;
        _viewModelManager = viewModelManager;
        _settings = settings;
        _status = RefreshStatus();
    }

    private string RefreshStatus() =>
        _settings.UseBrowserCookies
            ? $"Browser cookies enabled ({BrowserLabel})"
            : "Browser cookies are disabled";

    [RelayCommand]
    private async Task OpenAuthenticationSettingsAsync()
    {
        Close(false);

        var settings = _viewModelManager.CreateSettingsViewModel();
        settings.SelectedTabIndex = 2;
        await _dialogManager.ShowDialogAsync(settings);
    }

    [RelayCommand]
    private void CloseDialog() => Close(true);

    [RelayCommand]
    private void ClearBrowserCookies()
    {
        _settings.UseBrowserCookies = false;
        _settings.BrowserForCookies = string.Empty;
        _settings.Save();
        Status = RefreshStatus();
        OnPropertyChanged(nameof(IsBrowserCookiesEnabled));
        OnPropertyChanged(nameof(BrowserLabel));
    }
}
