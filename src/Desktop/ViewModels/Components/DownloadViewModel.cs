using System;
using CommunityToolkit.Mvvm.ComponentModel;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Shared.Models;

namespace EnhancedYoutubeDownloader.ViewModels.Components;

public partial class DownloadViewModel : ViewModelBase
{
    [ObservableProperty]
    private DownloadItem _downloadItem = new();

    public DownloadViewModel() { }
}
