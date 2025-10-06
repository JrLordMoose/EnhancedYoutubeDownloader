using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EnhancedYoutubeDownloader.Framework;

/// <summary>
/// Base class for dialog view models that return a result.
/// </summary>
/// <typeparam name="T">The type of the dialog result.</typeparam>
public abstract partial class DialogViewModelBase<T> : ViewModelBase
{
    private readonly TaskCompletionSource<T> _closeTcs = new(
        TaskCreationOptions.RunContinuationsAsynchronously
    );

    /// <summary>
    /// Gets or sets the dialog result.
    /// </summary>
    [ObservableProperty]
    public partial T? DialogResult { get; set; }

    /// <summary>
    /// Closes the dialog with a result.
    /// </summary>
    /// <param name="dialogResult">The dialog result.</param>
    [RelayCommand]
    protected void Close(T dialogResult)
    {
        DialogResult = dialogResult;
        _closeTcs.TrySetResult(dialogResult);
    }

    /// <summary>
    /// Waits for the dialog to close.
    /// </summary>
    /// <returns>The dialog result.</returns>
    public async Task<T> WaitForCloseAsync() => await _closeTcs.Task;
}

/// <summary>
/// Base class for dialog view models that return a boolean result.
/// </summary>
public abstract class DialogViewModelBase : DialogViewModelBase<bool?>;