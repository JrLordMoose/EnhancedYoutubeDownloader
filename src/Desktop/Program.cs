using System;
using System.Reflection;
using Avalonia;
using Avalonia.WebView.Desktop;
using EnhancedYoutubeDownloader.Utils;

namespace EnhancedYoutubeDownloader;

/// <summary>
/// The main entry point for the application.
/// </summary>
public static class Program
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    /// <summary>
    /// Gets the application name.
    /// </summary>
    public static string Name { get; } = Assembly.GetName().Name ?? "EnhancedYoutubeDownloader";

    /// <summary>
    /// Gets the application version.
    /// </summary>
    public static Version Version { get; } = Assembly.GetName().Version ?? new Version(1, 0, 0);

    /// <summary>
    /// Gets the application version as a string.
    /// </summary>
    public static string VersionString { get; } = Version.ToString(3);

    /// <summary>
    /// Gets a value indicating whether this is a development build.
    /// </summary>
    public static bool IsDevelopmentBuild { get; } = Version.Major is <= 0 or >= 999;

    /// <summary>
    /// Gets the project URL.
    /// </summary>
    public static string ProjectUrl { get; } =
        "https://github.com/EnhancedYoutubeDownloader/EnhancedYoutubeDownloader";

    /// <summary>
    /// Gets the project releases URL.
    /// </summary>
    public static string ProjectReleasesUrl { get; } = $"{ProjectUrl}/releases";

    /// <summary>
    /// Builds the Avalonia application.
    /// </summary>
    /// <returns>An <see cref="AppBuilder"/> instance.</returns>
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().UseDesktopWebView();

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The exit code.</returns>
    [STAThread]
    public static int Main(string[] args)
    {
        // Build and run the app
        var builder = BuildAvaloniaApp();

        try
        {
            return builder.StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            if (OperatingSystem.IsWindows())
                _ = NativeMethods.Windows.MessageBox(0, ex.ToString(), "Fatal Error", 0x10);

            throw;
        }
        finally
        {
            // Clean up after application shutdown
            if (builder.Instance is IDisposable disposableApp)
                disposableApp.Dispose();
        }
    }
}