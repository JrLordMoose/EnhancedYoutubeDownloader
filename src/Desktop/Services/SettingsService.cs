using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Cogwheel;
using EnhancedYoutubeDownloader.Framework;

namespace EnhancedYoutubeDownloader.Services;

public partial class SettingsService : SettingsBase
{
    [DefaultValue(ThemeVariant.Auto)]
    public ThemeVariant Theme { get; set; } = ThemeVariant.Auto;

    [DefaultValue(3)]
    public int ParallelLimit { get; set; } = 3;

    [DefaultValue(true)]
    public bool ShouldInjectSubtitles { get; set; } = true;

    [DefaultValue(true)]
    public bool ShouldInjectTags { get; set; } = true;

    [DefaultValue(true)]
    public bool ShouldInjectLanguageSpecificAudioStreams { get; set; } = true;

    [DefaultValue("")]
    public string LastAuthCookies { get; set; } = string.Empty;

    [DefaultValue(true)]
    public bool IsUkraineSupportMessageEnabled { get; set; } = true;

    [DefaultValue(true)]
    public bool AutoStartDownload { get; set; } = true;

    [DefaultValue(false)]
    public bool OpenFolderAfterDownload { get; set; } = false;

    [DefaultValue("")]
    public string DefaultDownloadPath { get; set; } = string.Empty;

    public SettingsService()
        : base(Program.Name) { }
}
