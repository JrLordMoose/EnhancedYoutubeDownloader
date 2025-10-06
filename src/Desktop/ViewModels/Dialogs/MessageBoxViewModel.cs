using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnhancedYoutubeDownloader.Framework;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

public partial class MessageBoxViewModel : DialogViewModelBase
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private string _primaryButtonText = "OK";

    [ObservableProperty]
    private string _secondaryButtonText = string.Empty;

    public MessageBoxViewModel() { }

    [RelayCommand]
    private void ClickPrimary()
    {
        Close(true);
    }

    [RelayCommand]
    private void ClickSecondary()
    {
        Close(false);
    }
}
