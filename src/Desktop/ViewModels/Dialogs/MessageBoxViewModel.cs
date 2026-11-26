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

    /// <summary>Optional help link shown under the message (hidden when empty).</summary>
    [ObservableProperty]
    private string _helpLinkUrl = string.Empty;

    [ObservableProperty]
    private string _helpLinkText = "Learn more";

    public MessageBoxViewModel() { }

    [RelayCommand]
    private void OpenHelpLink()
    {
        if (string.IsNullOrWhiteSpace(HelpLinkUrl))
            return;

        try
        {
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(HelpLinkUrl) { UseShellExecute = true }
            );
        }
        catch (Exception)
        {
            // No default browser: nothing else to do
        }
    }

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
