using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using EnhancedYoutubeDownloader.Framework;
using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

public partial class DownloadMultipleSetupViewModel : DialogViewModelBase
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private List<IVideo> _videos = new();

    [ObservableProperty]
    private bool _selectAll = true;

    public DownloadMultipleSetupViewModel() { }
}
