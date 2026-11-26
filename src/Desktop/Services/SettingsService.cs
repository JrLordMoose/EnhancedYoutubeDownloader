using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Cogwheel;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Shared.Models;

namespace EnhancedYoutubeDownloader.Services;

/// <summary>
/// User settings, persisted as JSON. Every property raises PropertyChanged so an open
/// Settings dialog refreshes after "Reset All" and live bindings (IsVisible etc.) update.
/// </summary>
public partial class SettingsService : SettingsBase, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // ---- General ----

    private ThemeVariant _theme = ThemeVariant.Auto;

    [DefaultValue(ThemeVariant.Auto)]
    public ThemeVariant Theme
    {
        get => _theme;
        set => SetField(ref _theme, value);
    }

    private bool _isAutoUpdateEnabled = true;

    [DefaultValue(true)]
    public bool IsAutoUpdateEnabled
    {
        get => _isAutoUpdateEnabled;
        set => SetField(ref _isAutoUpdateEnabled, value);
    }

    // ---- Downloads ----

    private int _parallelLimit = 3;

    [DefaultValue(3)]
    public int ParallelLimit
    {
        get => _parallelLimit;
        set => SetField(ref _parallelLimit, value);
    }

    private int _defaultQuality;

    /// <summary>0=Best, 1=1080p, 2=720p, 3=480p, 4=360p, 5=Audio only.</summary>
    [DefaultValue(0)]
    public int DefaultQuality
    {
        get => _defaultQuality;
        set => SetField(ref _defaultQuality, value);
    }

    private int _defaultFormat;

    /// <summary>0=MP4, 1=WebM, 2=MP3.</summary>
    [DefaultValue(0)]
    public int DefaultFormat
    {
        get => _defaultFormat;
        set => SetField(ref _defaultFormat, value);
    }

    private int _audioQuality;

    /// <summary>MP3 quality: 0=Best (VBR V0), 1=320 kbps, 2=192 kbps, 3=128 kbps.</summary>
    [DefaultValue(0)]
    public int AudioQuality
    {
        get => _audioQuality;
        set => SetField(ref _audioQuality, value);
    }

    private bool _shouldInjectSubtitles = true;

    [DefaultValue(true)]
    public bool ShouldInjectSubtitles
    {
        get => _shouldInjectSubtitles;
        set => SetField(ref _shouldInjectSubtitles, value);
    }

    private string _subtitleLanguage = "en";

    /// <summary>yt-dlp --sub-langs value, e.g. "en", "es", "en,es" or "all".</summary>
    [DefaultValue("en")]
    public string SubtitleLanguage
    {
        get => _subtitleLanguage;
        set => SetField(ref _subtitleLanguage, string.IsNullOrWhiteSpace(value) ? "en" : value);
    }

    private SubtitleStyle _subtitleStyle = SubtitleStyle.Embedded;

    [DefaultValue(SubtitleStyle.Embedded)]
    public SubtitleStyle SubtitleStyle
    {
        get => _subtitleStyle;
        set => SetField(ref _subtitleStyle, value);
    }

    private bool _enableAiSubtitles;

    [DefaultValue(false)]
    public bool EnableAISubtitles
    {
        get => _enableAiSubtitles;
        set => SetField(ref _enableAiSubtitles, value);
    }

    private string _aiSubtitleLanguage = "auto";

    [DefaultValue("auto")]
    public string AISubtitleLanguage
    {
        get => _aiSubtitleLanguage;
        set => SetField(ref _aiSubtitleLanguage, value);
    }

    private bool _shouldInjectTags = true;

    [DefaultValue(true)]
    public bool ShouldInjectTags
    {
        get => _shouldInjectTags;
        set => SetField(ref _shouldInjectTags, value);
    }

    private bool _autoStartDownload = true;

    [DefaultValue(true)]
    public bool AutoStartDownload
    {
        get => _autoStartDownload;
        set => SetField(ref _autoStartDownload, value);
    }

    private bool _openFolderAfterDownload;

    [DefaultValue(false)]
    public bool OpenFolderAfterDownload
    {
        get => _openFolderAfterDownload;
        set => SetField(ref _openFolderAfterDownload, value);
    }

    private bool _notifyOnComplete = true;

    /// <summary>Show a notification when a download finishes.</summary>
    [DefaultValue(true)]
    public bool NotifyOnComplete
    {
        get => _notifyOnComplete;
        set => SetField(ref _notifyOnComplete, value);
    }

    private string _defaultDownloadPath = string.Empty;

    [DefaultValue("")]
    public string DefaultDownloadPath
    {
        get => _defaultDownloadPath;
        set => SetField(ref _defaultDownloadPath, value ?? string.Empty);
    }

    private string _defaultCachePath = string.Empty;

    [DefaultValue("")]
    public string DefaultCachePath
    {
        get => _defaultCachePath;
        set => SetField(ref _defaultCachePath, value ?? string.Empty);
    }

    // ---- Advanced / authentication ----

    private bool _useBrowserCookies;

    [DefaultValue(false)]
    public bool UseBrowserCookies
    {
        get => _useBrowserCookies;
        set => SetField(ref _useBrowserCookies, value);
    }

    private string _browserForCookies = "chrome";

    [DefaultValue("chrome")]
    public string BrowserForCookies
    {
        get => _browserForCookies;
        set => SetField(ref _browserForCookies, value ?? string.Empty);
    }

    private string _cookiesFilePath = string.Empty;

    /// <summary>
    /// Optional path to a Netscape-format cookies.txt file. When set it takes precedence
    /// over <see cref="UseBrowserCookies"/>. Needed on Windows for Chrome/Edge/Brave, whose
    /// app-bound cookie encryption prevents yt-dlp from reading the browser database.
    /// </summary>
    [DefaultValue("")]
    public string CookiesFilePath
    {
        get => _cookiesFilePath;
        set => SetField(ref _cookiesFilePath, value ?? string.Empty);
    }

    // ---- Internal ----

    /// <summary>
    /// One-time flag: older builds saved AutoStartDownload=false by mistake and the startup
    /// "force it back on" code ran before settings were loaded, so it never took effect.
    /// </summary>
    [DefaultValue(false)]
    public bool AutoStartMigrationDone { get; set; }

    public SettingsService()
        : base(GetSettingsFilePath()) { }

    /// <summary>
    /// Settings live in %AppData%\EnhancedYoutubeDownloader\settings.json. Older builds saved
    /// to a file named after the app in the *current working directory*, which changed with
    /// how the app was launched and could land inside a git checkout. Copy it over once.
    /// </summary>
    internal static string GetSettingsFilePath()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EnhancedYoutubeDownloader"
        );
        var path = Path.Combine(directory, "settings.json");

        try
        {
            Directory.CreateDirectory(directory);
            if (!File.Exists(path))
            {
                var legacy = new[]
                {
                    Path.Combine(AppContext.BaseDirectory, Program.Name),
                    Path.Combine(Environment.CurrentDirectory, Program.Name),
                }.FirstOrDefault(File.Exists);

                if (legacy != null)
                    File.Copy(legacy, path);
            }
        }
        catch (Exception)
        {
            // Fall back to defaults; never block startup over settings migration
        }

        return path;
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        OnPropertyChanged(propertyName);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
