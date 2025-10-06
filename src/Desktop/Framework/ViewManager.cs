using Avalonia.Controls;
using Avalonia.Controls.Templates;
using EnhancedYoutubeDownloader.ViewModels;
using EnhancedYoutubeDownloader.ViewModels.Components;
using EnhancedYoutubeDownloader.ViewModels.Dialogs;
using EnhancedYoutubeDownloader.Views;
using EnhancedYoutubeDownloader.Views.Dialogs;

namespace EnhancedYoutubeDownloader.Framework;

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
            TutorialViewModel => new TutorialDialog(),
            ErrorDialogViewModel => new ErrorDialog(),
            _ => null,
        };

    public Control? TryBindView(ViewModelBase viewModel)
    {
        var view = TryCreateView(viewModel);
        if (view is null)
            return null;

        view.DataContext ??= viewModel;

        return view;
    }
}

public partial class ViewManager : IDataTemplate
{
    bool IDataTemplate.Match(object? data) => data is ViewModelBase;

    Control? ITemplate<object?, Control?>.Build(object? data) =>
        data is ViewModelBase viewModel ? TryBindView(viewModel) : null;
}
