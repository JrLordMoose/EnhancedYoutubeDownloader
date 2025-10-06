using System;
using CommunityToolkit.Mvvm.ComponentModel;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Shared.Models;

namespace EnhancedYoutubeDownloader.ViewModels.Components;

/// <summary>
/// The view model for a single download item.
/// </summary>
public partial class DownloadViewModel : ViewModelBase
{
    /// <summary>
    /// Gets or sets the download item.
    /// </summary>
    [ObservableProperty]
    private DownloadItem _downloadItem = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadViewModel"/> class.
    /// </summary>
    public DownloadViewModel() { }
}