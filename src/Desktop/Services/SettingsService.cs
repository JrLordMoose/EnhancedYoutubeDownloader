using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Cogwheel;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Shared.Models;

namespace EnhancedYoutubeDownloader.Services;

public partial class SettingsService : SettingsBase, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private ThemeVariant _theme = ThemeVariant.Auto;

    [DefaultValue(ThemeVariant.Auto)]
    public ThemeVariant Theme
    {
        get => _theme;
        set
        {
            if (_theme != value)
            {
                _theme = value;
                OnPropertyChanged();
            }
        }
    }

    [DefaultValue(true)]
    public bool IsAutoUpdateEnabled { get; set; } = true;

    [DefaultValue(3)]
    public int ParallelLimit { get; set; } = 3;

    [DefaultValue(0)]
    public int DefaultQuality { get; set; } = 0; // 0=Best, 1=1080p, 2=720p, 3=480p

    [DefaultValue(0)]
    public int DefaultFormat { get; set; } = 0; // 0=MP4, 1=WebM, 2=MP3

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

    [DefaultValue("")]
    public string DefaultCachePath { get; set; } = string.Empty;

    [DefaultValue(false)]
    public bool EnableAISubtitles { get; set; } = false;

    [DefaultValue("auto")]
    public string AISubtitleLanguage { get; set; } = "auto";

    [DefaultValue(true)]
    public bool AskBeforeGeneratingSubtitles { get; set; } = true;

    [DefaultValue(SubtitleStyle.Embedded)]
    public SubtitleStyle SubtitleStyle { get; set; } = SubtitleStyle.Embedded;

    [DefaultValue(24)]
    public int SubtitleFontSize { get; set; } = 24;

    [DefaultValue(0.75)]
    public double SubtitleBackgroundOpacity { get; set; } = 0.75;

    public SettingsService()
        : base(Program.Name) { }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
