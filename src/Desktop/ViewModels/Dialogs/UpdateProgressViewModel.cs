using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnhancedYoutubeDownloader.Framework;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

/// <summary>
/// Shown while an update downloads and installs, so the user sees it happening instead of
/// the app silently disappearing. Driven by <see cref="Services.UpdateCoordinator"/>.
/// </summary>
public partial class UpdateProgressViewModel : DialogViewModelBase
{
    [ObservableProperty]
    private string _title = "Updating";

    [ObservableProperty]
    private string _status = "Preparing update...";

    [ObservableProperty]
    private string _detail = string.Empty;

    /// <summary>The folder being updated (where the app is installed), shown to the user.</summary>
    public string InstallLocation { get; } =
        System.IO.Path.TrimEndingDirectorySeparator(System.AppContext.BaseDirectory);

    /// <summary>0-100 while downloading.</summary>
    [ObservableProperty]
    private double _progress;

    /// <summary>True for the install phase, where no percentage is available.</summary>
    [ObservableProperty]
    private bool _isIndeterminate;

    /// <summary>The download can be canceled; the install phase can't.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool _canCancel = true;

    public bool IsCanceled { get; private set; }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        IsCanceled = true;
        Close(false);
    }
}
