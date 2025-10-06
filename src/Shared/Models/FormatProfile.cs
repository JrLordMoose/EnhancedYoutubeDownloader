using CommunityToolkit.Mvvm.ComponentModel;

namespace EnhancedYoutubeDownloader.Shared.Models;

/// <summary>
/// Represents a format profile for downloading videos.
/// </summary>
public partial class FormatProfile : ObservableObject
{
    /// <summary>
    /// Gets or sets the unique identifier for the format profile.
    /// </summary>
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the name of the format profile.
    /// </summary>
    [ObservableProperty]
    private string _name = string.Empty;

    /// <summary>
    /// Gets or sets the description of the format profile.
    /// </summary>
    [ObservableProperty]
    private string _description = string.Empty;

    /// <summary>
    /// Gets or sets the container format (e.g., "mp4", "webm").
    /// </summary>
    [ObservableProperty]
    private string _container = "mp4";

    /// <summary>
    /// Gets or sets the desired video quality (e.g., "best", "1080p").
    /// </summary>
    [ObservableProperty]
    private string _quality = "best";

    /// <summary>
    /// Gets or sets a value indicating whether to include subtitles.
    /// </summary>
    [ObservableProperty]
    private bool _includeSubtitles = true;

    /// <summary>
    /// Gets or sets a value indicating whether to include metadata tags.
    /// </summary>
    [ObservableProperty]
    private bool _includeTags = true;

    /// <summary>
    /// Gets or sets a value indicating whether this is the default profile.
    /// </summary>
    [ObservableProperty]
    private bool _isDefault;

    /// <summary>
    /// Gets or sets the timestamp when the profile was created.
    /// </summary>
    [ObservableProperty]
    private DateTime _createdAt = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when the profile was last updated.
    /// </summary>
    [ObservableProperty]
    private DateTime _updatedAt = DateTime.UtcNow;
}