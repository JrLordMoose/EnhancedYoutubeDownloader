using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Cogwheel;
using EnhancedYoutubeDownloader.Framework;

namespace EnhancedYoutubeDownloader.Services;

/// <summary>
/// Manages application settings.
/// </summary>
public partial class SettingsService : SettingsBase
{
    /// <summary>
    /// Gets or sets the application theme.
    /// </summary>
    [DefaultValue(ThemeVariant.Auto)]
    public ThemeVariant Theme { get; set; } = ThemeVariant.Auto;

    /// <summary>
    /// Gets or sets the maximum number of parallel downloads.
    /// </summary>
    [DefaultValue(3)]
    public int ParallelLimit { get; set; } = 3;

    /// <summary>
    /// Gets or sets a value indicating whether to inject subtitles.
    /// </summary>
    [DefaultValue(true)]
    public bool ShouldInjectSubtitles { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to inject tags.
    /// </summary>
    [DefaultValue(true)]
    public bool ShouldInjectTags { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to inject language-specific audio streams.
    /// </summary>
    [DefaultValue(true)]
    public bool ShouldInjectLanguageSpecificAudioStreams { get; set; } = true;

    /// <summary>
    /// Gets or sets the last used authentication cookies.
    /// </summary>
    [DefaultValue("")]
    public string LastAuthCookies { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the Ukraine support message is enabled.
    /// </summary>
    [DefaultValue(true)]
    public bool IsUkraineSupportMessageEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to automatically start downloads.
    /// </summary>
    [DefaultValue(true)]
    public bool AutoStartDownload { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to open the folder after a download is complete.
    /// </summary>
    [DefaultValue(false)]
    public bool OpenFolderAfterDownload { get; set; } = false;

    /// <summary>
    /// Gets or sets the default download path.
    /// </summary>
    [DefaultValue("")]
    public string DefaultDownloadPath { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsService"/> class.
    /// </summary>
    public SettingsService()
        : base(Program.Name) { }
}