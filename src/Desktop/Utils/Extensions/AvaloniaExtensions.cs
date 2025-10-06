using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace EnhancedYoutubeDownloader.Utils.Extensions;

/// <summary>
/// Provides extension methods for Avalonia UI.
/// </summary>
public static class AvaloniaExtensions
{
    /// <summary>
    /// Tries to get the main window from the application instance.
    /// </summary>
    /// <param name="mainWindow">The main window if found, otherwise null.</param>
    /// <returns>True if the main window was found, otherwise false.</returns>
    public static bool TryGetMainWindow([NotNullWhen(true)] out Window? mainWindow)
    {
        mainWindow = null;

        if (
            Application.Current?.ApplicationLifetime
            is not Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
        )
        {
            return false;
        }

        mainWindow = desktop.MainWindow;
        return mainWindow != null;
    }

    /// <summary>
    /// Tries to get the top level control from a visual element.
    /// </summary>
    /// <param name="visual">The visual element.</param>
    /// <param name="topLevel">The top level control if found, otherwise null.</param>
    /// <returns>True if the top level control was found, otherwise false.</returns>
    public static bool TryGetTopLevel(
        this Visual? visual,
        [NotNullWhen(true)] out TopLevel? topLevel
    )
    {
        topLevel = null;

        if (visual == null)
        {
            return false;
        }

        topLevel = TopLevel.GetTopLevel(visual);
        return topLevel != null;
    }

    /// <summary>
    /// Tries to get the local file path from a storage file.
    /// </summary>
    /// <param name="file">The storage file.</param>
    /// <param name="path">The local file path if found, otherwise null.</param>
    /// <returns>True if the local file path was found, otherwise false.</returns>
    public static bool TryGetLocalPath(
        this IStorageFile? file,
        [NotNullWhen(true)] out string? path
    )
    {
        path = null;

        if (file == null)
        {
            return false;
        }

        var uri = file.Path;
        if (!uri.IsFile)
        {
            return false;
        }

        path = uri.LocalPath;
        return true;
    }
}