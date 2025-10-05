using CommunityToolkit.Mvvm.ComponentModel;

namespace EnhancedYoutubeDownloader.Shared.Models;

public partial class FormatProfile : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _container = "mp4";

    [ObservableProperty]
    private string _quality = "best";

    [ObservableProperty]
    private bool _includeSubtitles = true;

    [ObservableProperty]
    private bool _includeTags = true;

    [ObservableProperty]
    private bool _isDefault;

    [ObservableProperty]
    private DateTime _createdAt = DateTime.UtcNow;

    [ObservableProperty]
    private DateTime _updatedAt = DateTime.UtcNow;
}