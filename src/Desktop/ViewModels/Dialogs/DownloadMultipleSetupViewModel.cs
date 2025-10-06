using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using EnhancedYoutubeDownloader.Framework;
using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

/// <summary>
/// The view model for the download multiple setup dialog.
/// </summary>
public partial class DownloadMultipleSetupViewModel : DialogViewModelBase
{
    /// <summary>
    /// Gets or sets the title of the dialog.
    /// </summary>
    [ObservableProperty]
    private string _title = string.Empty;

    /// <summary>
    /// Gets or sets the list of videos to download.
    /// </summary>
    [ObservableProperty]
    private List<IVideo> _videos = new();

    /// <summary>
    /// Gets or sets a value indicating whether to select all videos.
    /// </summary>
    [ObservableProperty]
    private bool _selectAll = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadMultipleSetupViewModel"/> class.
    /// </summary>
    public DownloadMultipleSetupViewModel() { }
}