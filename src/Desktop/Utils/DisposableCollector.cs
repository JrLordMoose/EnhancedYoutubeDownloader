using System;
using System.Collections.Generic;

namespace EnhancedYoutubeDownloader.Utils;

/// <summary>
/// A helper class that collects disposable objects and disposes them all at once.
/// </summary>
public class DisposableCollector : IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private bool _disposed;

    /// <summary>
    /// Adds a disposable object to the collection.
    /// </summary>
    /// <param name="disposable">The disposable object to add.</param>
    public void Add(IDisposable disposable)
    {
        if (_disposed)
        {
            disposable?.Dispose();
            return;
        }

        _disposables.Add(disposable);
    }

    /// <summary>
    /// Disposes all the collected disposable objects.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        foreach (var disposable in _disposables)
        {
            try
            {
                disposable?.Dispose();
            }
            catch
            {
                // Ignore disposal errors
            }
        }

        _disposables.Clear();
    }
}