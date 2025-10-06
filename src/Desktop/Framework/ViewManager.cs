using Avalonia.Controls;
using Avalonia.Controls.Templates;
using EnhancedYoutubeDownloader.ViewModels;
using EnhancedYoutubeDownloader.ViewModels.Components;
using EnhancedYoutubeDownloader.ViewModels.Dialogs;
using EnhancedYoutubeDownloader.Views;
using EnhancedYoutubeDownloader.Views.Dialogs;

namespace EnhancedYoutubeDownloader.Framework;

/// <summary>
/// Manages the creation and binding of views to view models.
/// </summary>
public partial class ViewManager
{
    private Control? TryCreateView(ViewModelBase viewModel) =>
        viewModel switch
        {
            MainViewModel => new MainView(),
            DashboardViewModel => null, // Dashboard is embedded in MainView
            AuthSetupViewModel => new AuthSetupDialog(),
            DownloadMultipleSetupViewModel => new DownloadMultipleSetupDialog(),
            DownloadSingleSetupViewModel => new DownloadSingleSetupDialog(),
            MessageBoxViewModel => new MessageBoxDialog(),
            SettingsViewModel => new SettingsDialog(),
            ErrorDialogViewModel => new ErrorDialog(),
            _ => null,
        };

    /// <summary>
    /// Tries to create and bind a view to a view model.
    /// </summary>
    /// <param name="viewModel">The view model to bind.</param>
    /// <returns>The created view, or null if no view is found for the view model.</returns>
    public Control? TryBindView(ViewModelBase viewModel)
    {
        var view = TryCreateView(viewModel);
        if (view is null)
            return null;

        view.DataContext ??= viewModel;

        return view;
    }
}

/// <summary>
/// Implements the <see cref="IDataTemplate"/> interface for the <see cref="ViewManager"/>.
/// </summary>
public partial class ViewManager : IDataTemplate
{
    /// <summary>
    /// Determines whether the data template can be applied to the specified data.
    /// </summary>
    /// <param name="data">The data to check.</param>
    /// <returns>True if the data is a <see cref="ViewModelBase"/>, otherwise false.</returns>
    bool IDataTemplate.Match(object? data) => data is ViewModelBase;

    /// <summary>
    /// Creates a control based on the specified data.
    /// </summary>
    /// <param name="data">The data to create the control from.</param>
    /// <returns>The created control, or null if the data is not a <see cref="ViewModelBase"/>.</returns>
    Control? ITemplate<object?, Control?>.Build(object? data) =>
        data is ViewModelBase viewModel ? TryBindView(viewModel) : null;
}