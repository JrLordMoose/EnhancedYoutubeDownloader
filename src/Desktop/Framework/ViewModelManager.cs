using System;
using EnhancedYoutubeDownloader.ViewModels;
using EnhancedYoutubeDownloader.ViewModels.Components;
using EnhancedYoutubeDownloader.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace EnhancedYoutubeDownloader.Framework;

public class ViewModelManager
{
    private readonly IServiceProvider _serviceProvider;

    public ViewModelManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public MainViewModel CreateMainViewModel() =>
        _serviceProvider.GetRequiredService<MainViewModel>();

    public DashboardViewModel CreateDashboardViewModel() =>
        _serviceProvider.GetRequiredService<DashboardViewModel>();

    public DownloadViewModel CreateDownloadViewModel() =>
        _serviceProvider.GetRequiredService<DownloadViewModel>();

    public AuthSetupViewModel CreateAuthSetupViewModel() =>
        _serviceProvider.GetRequiredService<AuthSetupViewModel>();

    public DownloadMultipleSetupViewModel CreateDownloadMultipleSetupViewModel() =>
        _serviceProvider.GetRequiredService<DownloadMultipleSetupViewModel>();

    public DownloadSingleSetupViewModel CreateDownloadSingleSetupViewModel() =>
        _serviceProvider.GetRequiredService<DownloadSingleSetupViewModel>();

    public MessageBoxViewModel CreateMessageBoxViewModel() =>
        _serviceProvider.GetRequiredService<MessageBoxViewModel>();

    public SettingsViewModel CreateSettingsViewModel() =>
        _serviceProvider.GetRequiredService<SettingsViewModel>();

    public TutorialViewModel CreateTutorialViewModel() =>
        _serviceProvider.GetRequiredService<TutorialViewModel>();

    public ErrorDialogViewModel CreateErrorDialogViewModel() =>
        _serviceProvider.GetRequiredService<ErrorDialogViewModel>();
}
