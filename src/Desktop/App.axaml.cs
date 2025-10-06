using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using AvaloniaWebView;
using EnhancedYoutubeDownloader.Core.Services;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Services;
using EnhancedYoutubeDownloader.Shared.Interfaces;
using EnhancedYoutubeDownloader.Shared.Models;
using EnhancedYoutubeDownloader.Utils;
using EnhancedYoutubeDownloader.Utils.Extensions;
using EnhancedYoutubeDownloader.ViewModels;
using EnhancedYoutubeDownloader.ViewModels.Components;
using EnhancedYoutubeDownloader.ViewModels.Dialogs;
using EnhancedYoutubeDownloader.Views;
using Material.Styles.Themes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EnhancedYoutubeDownloader;

public class App : Application, IDisposable
{
    private readonly DisposableCollector _eventRoot = new();
    private readonly IHost _host;
    private readonly SettingsService _settingsService;
    private readonly MainViewModel _mainViewModel;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(ConfigureServices)
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        _settingsService = _host.Services.GetRequiredService<SettingsService>();

        // Ensure AutoStartDownload is enabled (fixes issue where it was persisted as false)
        Console.WriteLine(
            $"[APP] Initial AutoStartDownload value: {_settingsService.AutoStartDownload}"
        );
        if (!_settingsService.AutoStartDownload)
        {
            Console.WriteLine("[APP] AutoStartDownload is false, setting to true");
            _settingsService.AutoStartDownload = true;
            _settingsService.Save();
            Console.WriteLine("[APP] AutoStartDownload reset to true and saved");
        }
        else
        {
            Console.WriteLine("[APP] AutoStartDownload is already true");
        }

        _mainViewModel = _host.Services.GetRequiredService<MainViewModel>();

        // Re-initialize the theme when the user changes it
        _eventRoot.Add(
            _settingsService.WatchProperty(
                o => o.Theme,
                () =>
                {
                    RequestedThemeVariant = _settingsService.Theme switch
                    {
                        ThemeVariant.Light => Avalonia.Styling.ThemeVariant.Light,
                        ThemeVariant.Dark => Avalonia.Styling.ThemeVariant.Dark,
                        _ => Avalonia.Styling.ThemeVariant.Default,
                    };

                    InitializeTheme();
                }
            )
        );
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Framework
        services.AddSingleton<DialogManager>();
        services.AddSingleton<SnackbarManager>();
        services.AddSingleton<ViewManager>();
        services.AddSingleton<ViewModelManager>();

        // Application Services (must be registered first for cache path resolution)
        services.AddSingleton<SettingsService>();

        // Core Services (with cache path from settings)
        services.AddSingleton<ICacheService>(sp =>
        {
            var settings = sp.GetRequiredService<SettingsService>();
            var cachePath = string.IsNullOrWhiteSpace(settings.DefaultCachePath)
                ? null
                : settings.DefaultCachePath;
            return new CacheService(cachePath);
        });

        services.AddSingleton<DownloadStateRepository>(sp =>
        {
            var settings = sp.GetRequiredService<SettingsService>();
            var cachePath = string.IsNullOrWhiteSpace(settings.DefaultCachePath)
                ? null
                : settings.DefaultCachePath;
            return new DownloadStateRepository(cachePath);
        });

        services.AddSingleton<IDownloadService, YtDlpDownloadService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IQueryResolver, QueryResolver>();
        services.AddSingleton<IDependencyValidator, DependencyValidator>();

        // UpdateService is Windows-only
        if (OperatingSystem.IsWindows())
        {
            services.AddSingleton<UpdateService>();
        }

        // View models
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DownloadViewModel>();
        services.AddTransient<AuthSetupViewModel>();
        services.AddTransient<DownloadMultipleSetupViewModel>();
        services.AddTransient<DownloadSingleSetupViewModel>();
        services.AddTransient<MessageBoxViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<TutorialViewModel>();
        services.AddTransient<ErrorDialogViewModel>();
    }

    public override void Initialize()
    {
        base.Initialize();

        AvaloniaXamlLoader.Load(this);
    }

    public override void RegisterServices()
    {
        base.RegisterServices();

        AvaloniaWebViewBuilder.Initialize(config => config.IsInPrivateModeEnabled = true);
    }

    private void InitializeTheme()
    {
        var actualTheme = RequestedThemeVariant?.Key switch
        {
            "Light" => PlatformThemeVariant.Light,
            "Dark" => PlatformThemeVariant.Dark,
            _ => PlatformSettings?.GetColorValues().ThemeVariant ?? PlatformThemeVariant.Light,
        };

        this.LocateMaterialTheme<MaterialThemeBase>().CurrentTheme =
            actualTheme == PlatformThemeVariant.Light
                ? Theme.Create(Theme.Light, Color.Parse("#FAFAFA"), Color.Parse("#F9A825"))
                : Theme.Create(Theme.Dark, Color.Parse("#E8E8E8"), Color.Parse("#F9A825"));
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainView { DataContext = _mainViewModel };

        base.OnFrameworkInitializationCompleted();

        // Set up custom theme colors
        InitializeTheme();

        // Load settings
        _settingsService.Load();

        // Validate dependencies asynchronously (don't block startup)
        _ = ValidateDependenciesAsync();
    }

    private async System.Threading.Tasks.Task ValidateDependenciesAsync()
    {
        try
        {
            Console.WriteLine("[APP] Starting dependency validation...");

            // Get dependency validator service
            var dependencyValidator = _host.Services.GetRequiredService<IDependencyValidator>();

            // Validate dependencies
            var validationResult = await dependencyValidator.ValidateAsync();

            // If validation failed, show error dialog
            if (!validationResult.IsValid)
            {
                Console.WriteLine(
                    $"[APP] Dependency validation failed: {validationResult.MissingDependencies.Count} missing"
                );

                // Show error dialog on UI thread
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await ShowDependencyErrorDialogAsync(validationResult);
                });
            }
            else
            {
                Console.WriteLine("[APP] Dependency validation completed successfully");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[APP] Error during dependency validation: {ex.Message}");
        }
    }

    private async System.Threading.Tasks.Task ShowDependencyErrorDialogAsync(
        DependencyValidationResult validationResult
    )
    {
        try
        {
            // Get required services
            var dialogManager = _host.Services.GetRequiredService<DialogManager>();
            var viewModelManager = _host.Services.GetRequiredService<ViewModelManager>();

            // Build error message
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine(
                "The following required dependencies are missing. Please download and place them in the application directory:"
            );
            messageBuilder.AppendLine();

            foreach (var missing in validationResult.MissingDependencies)
            {
                messageBuilder.AppendLine($"• {missing.Name}");
                messageBuilder.AppendLine($"  Expected location: {missing.ExpectedPath}");
                messageBuilder.AppendLine($"  Download from: {missing.DownloadUrl}");
                messageBuilder.AppendLine();
            }

            messageBuilder.AppendLine(
                "The application may not function correctly without these dependencies."
            );

            // Build suggested actions
            var actions = validationResult
                .MissingDependencies.Select(dep => new ErrorAction
                {
                    Text = $"Download {dep.Name}",
                    ActionKey = $"open_{dep.Name.ToLower().Replace(" ", "_")}",
                    Description = $"Open {dep.Name} download page in browser",
                })
                .ToList();

            // Create error info
            var errorInfo = new ErrorInfo
            {
                Message = "Missing Required Dependencies",
                Details = messageBuilder.ToString(),
                Category = ErrorCategory.FileSystem,
                SuggestedActions = actions,
            };

            // Create error dialog
            var errorDialog = viewModelManager.CreateErrorDialogViewModel();
            errorDialog.ErrorInfo = errorInfo;

            // Handle action clicks (open URLs in browser)
            errorDialog.OnActionSelected = actionKey =>
            {
                try
                {
                    var missingDep = validationResult.MissingDependencies.FirstOrDefault(d =>
                        actionKey.Contains(d.Name.ToLower().Replace(" ", "_"))
                    );

                    if (missingDep != null)
                    {
                        Console.WriteLine(
                            $"[APP] Opening download URL for {missingDep.Name}: {missingDep.DownloadUrl}"
                        );

                        // Open URL in default browser
                        var psi = new ProcessStartInfo
                        {
                            FileName = missingDep.DownloadUrl,
                            UseShellExecute = true,
                        };
                        Process.Start(psi);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[APP] Failed to open URL: {ex.Message}");
                }
            };

            // Show dialog
            await dialogManager.ShowDialogAsync(errorDialog);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[APP] Failed to show dependency error dialog: {ex.Message}");

            // Fallback: show console message
            Console.WriteLine("\n========================================");
            Console.WriteLine("MISSING DEPENDENCIES:");
            Console.WriteLine("========================================");
            foreach (var missing in validationResult.MissingDependencies)
            {
                Console.WriteLine($"\n{missing.Name}:");
                Console.WriteLine($"  Location: {missing.ExpectedPath}");
                Console.WriteLine($"  Download: {missing.DownloadUrl}");
            }
            Console.WriteLine("========================================\n");
        }
    }

    private void Application_OnActualThemeVariantChanged(object? sender, EventArgs args) =>
        // Re-initialize the theme when the system theme changes
        InitializeTheme();

    public void Dispose()
    {
        _eventRoot.Dispose();
        _host?.Dispose();
    }
}
