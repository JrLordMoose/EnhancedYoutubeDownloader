using System;
using EnhancedYoutubeDownloader.ViewModels;
using EnhancedYoutubeDownloader.ViewModels.Components;
using EnhancedYoutubeDownloader.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace EnhancedYoutubeDownloader.Framework;

/// <summary>
/// Manages the creation of view models.
/// </summary>
public class ViewModelManager
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewModelManager"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public ViewModelManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates a new <see cref="MainViewModel"/>.
    /// </summary>
    /// <returns>The created view model.</returns>
    public MainViewModel CreateMainViewModel() =>
        _serviceProvider.GetRequiredService<MainViewModel>();

    /// <summary>
    /// Creates a new <see cref="DashboardViewModel"/>.
    /// </summary>
    /// <returns>The created view model.</returns>
    public DashboardViewModel CreateDashboardViewModel() =>
        _serviceProvider.GetRequiredService<DashboardViewModel>();

    /// <summary>
    /// Creates a new <see cref="DownloadViewModel"/>.
    /// </summary>
    /// <returns>The created view model.</returns>
    public DownloadViewModel CreateDownloadViewModel() =>
        _serviceProvider.GetRequiredService<DownloadViewModel>();

    /// <summary>
    /// Creates a new <see cref="AuthSetupViewModel"/>.
    /// </summary>
    /// <returns>The created view model.</returns>
    public AuthSetupViewModel CreateAuthSetupViewModel() =>
        _serviceProvider.GetRequiredService<AuthSetupViewModel>();

    /// <summary>
    /// Creates a new <see cref="DownloadMultipleSetupViewModel"/>.
    /// </summary>
    /// <returns>The created view model.</returns>
    public DownloadMultipleSetupViewModel CreateDownloadMultipleSetupViewModel() =>
        _serviceProvider.GetRequiredService<DownloadMultipleSetupViewModel>();

    /// <summary>
    /// Creates a new <see cref="DownloadSingleSetupViewModel"/>.
    /// </summary>
    /// <returns>The created view model.</returns>
    public DownloadSingleSetupViewModel CreateDownloadSingleSetupViewModel() =>
        _serviceProvider.GetRequiredService<DownloadSingleSetupViewModel>();

    /// <summary>
    /// Creates a new <see cref="MessageBoxViewModel"/>.
    /// </summary>
    /// <returns>The created view model.</returns>
    public MessageBoxViewModel CreateMessageBoxViewModel() =>
        _serviceProvider.GetRequiredService<MessageBoxViewModel>();

    /// <summary>
    /// Creates a new <see cref="SettingsViewModel"/>.
    /// </summary>
    /// <returns>The created view model.</returns>
    public SettingsViewModel CreateSettingsViewModel() =>
        _serviceProvider.GetRequiredService<SettingsViewModel>();

    /// <summary>
    /// Creates a new <see cref="ErrorDialogViewModel"/>.
    /// </summary>
    /// <returns>The created view model.</returns>
    public ErrorDialogViewModel CreateErrorDialogViewModel() =>
        _serviceProvider.GetRequiredService<ErrorDialogViewModel>();
}