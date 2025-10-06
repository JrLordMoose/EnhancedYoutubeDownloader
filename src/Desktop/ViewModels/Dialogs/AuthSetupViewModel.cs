using System;
using CommunityToolkit.Mvvm.ComponentModel;
using EnhancedYoutubeDownloader.Framework;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

public partial class AuthSetupViewModel : DialogViewModelBase
{
    [ObservableProperty]
    private string _status = "Ready";

    public AuthSetupViewModel() { }
}
