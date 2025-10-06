using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using DialogHostAvalonia;
using EnhancedYoutubeDownloader.Utils.Extensions;

namespace EnhancedYoutubeDownloader.Framework;

/// <summary>
/// Manages showing dialogs and file pickers.
/// </summary>
public class DialogManager : IDisposable
{
    private readonly SemaphoreSlim _dialogLock = new(1, 1);

    /// <summary>
    /// Shows a dialog and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the dialog result.</typeparam>
    /// <param name="dialog">The dialog view model.</param>
    /// <returns>The dialog result.</returns>
    public async Task<T?> ShowDialogAsync<T>(DialogViewModelBase<T> dialog)
    {
        await _dialogLock.WaitAsync();
        try
        {
            await DialogHost.Show(
                dialog,
                "RootDialog",
                async (object _, DialogOpenedEventArgs args) =>
                {
                    await dialog.WaitForCloseAsync();

                    try
                    {
                        args.Session.Close();
                    }
                    catch (InvalidOperationException)
                    {
                        // Dialog host is already processing a close operation
                    }
                }
            );

            return dialog.DialogResult;
        }
        finally
        {
            _dialogLock.Release();
        }
    }

    /// <summary>
    /// Shows a dialog.
    /// </summary>
    /// <param name="dialog">The dialog view model.</param>
    /// <returns>The dialog result.</returns>
    public async Task<bool?> ShowDialogAsync(DialogViewModelBase dialog)
    {
        return await ShowDialogAsync<bool?>(dialog);
    }

    /// <summary>
    /// Shows an open file dialog.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="filters">The file filters.</param>
    /// <returns>The selected file path.</returns>
    public Task<string?> ShowOpenFileDialogAsync(string title, string[] filters)
    {
        // This is a simplified implementation
        // In a real implementation, you'd use Avalonia's file dialogs
        return Task.FromResult<string?>(null);
    }

    /// <summary>
    /// Shows a save file dialog.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="filters">The file filters.</param>
    /// <param name="defaultName">The default file name.</param>
    /// <returns>The selected file path.</returns>
    public Task<string?> ShowSaveFileDialogAsync(string title, string[] filters, string defaultName)
    {
        // This is a simplified implementation
        // In a real implementation, you'd use Avalonia's file dialogs
        return Task.FromResult<string?>(null);
    }

    /// <summary>
    /// Shows a folder dialog.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <returns>The selected folder path.</returns>
    public Task<string?> ShowFolderDialogAsync(string title)
    {
        // This is a simplified implementation
        // In a real implementation, you'd use Avalonia's folder dialogs
        return Task.FromResult<string?>(null);
    }

    /// <summary>
    /// Prompts the user to select a file save location using the native file picker.
    /// </summary>
    /// <param name="defaultFileName">Default file name with extension</param>
    /// <param name="fileTypeChoices">File type filters (e.g., "MP4 Video" -> [".mp4"])</param>
    /// <returns>Selected file path, or null if cancelled</returns>
    public async Task<string?> PromptSaveFilePathAsync(
        string? defaultFileName = null,
        Dictionary<string, string[]>? fileTypeChoices = null
    )
    {
        // Get the main window
        if (!AvaloniaExtensions.TryGetMainWindow(out var mainWindow))
        {
            return null;
        }

        // Get the storage provider from the main window
        var storageProvider = mainWindow.StorageProvider;
        if (storageProvider == null)
        {
            return null;
        }

        // Build file type filters
        var fileTypes = new List<FilePickerFileType>();

        if (fileTypeChoices != null && fileTypeChoices.Count > 0)
        {
            foreach (var (name, extensions) in fileTypeChoices)
            {
                fileTypes.Add(
                    new FilePickerFileType(name)
                    {
                        Patterns = extensions.Select(ext => $"*{ext}").ToArray(),
                    }
                );
            }
        }
        else
        {
            // Default to all files if no filters specified
            fileTypes.Add(FilePickerFileTypes.All);
        }

        // Configure save file picker options
        var options = new FilePickerSaveOptions
        {
            Title = "Save File",
            SuggestedFileName = defaultFileName,
            FileTypeChoices = fileTypes,
            DefaultExtension = fileTypeChoices?.FirstOrDefault().Value?.FirstOrDefault(),
        };

        // If a default filename is provided, try to set the suggested start location
        if (
            !string.IsNullOrWhiteSpace(defaultFileName)
            && await TryGetSuggestedFolderAsync(storageProvider) is { } suggestedFolder
        )
        {
            options.SuggestedStartLocation = suggestedFolder;
        }

        // Show the save file picker
        var result = await storageProvider.SaveFilePickerAsync(options);

        // Extract and return the local path
        return result.TryGetLocalPath(out var path) ? path : null;
    }

    private static async Task<IStorageFolder?> TryGetSuggestedFolderAsync(
        IStorageProvider storageProvider
    )
    {
        try
        {
            // Try Videos folder first
            var videosFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            if (Directory.Exists(videosFolder))
            {
                return await storageProvider.TryGetFolderFromPathAsync(videosFolder);
            }

            // Fallback to Documents folder
            var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (Directory.Exists(documentsFolder))
            {
                return await storageProvider.TryGetFolderFromPathAsync(documentsFolder);
            }
        }
        catch
        {
            // Ignore errors, return null
        }

        return null;
    }

    /// <summary>
    /// Disposes the dialog manager.
    /// </summary>
    public void Dispose() => _dialogLock.Dispose();
}