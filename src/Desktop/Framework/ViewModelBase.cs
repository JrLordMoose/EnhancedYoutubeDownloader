using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace EnhancedYoutubeDownloader.Framework;

/// <summary>
/// Base class for all view models.
/// </summary>
public abstract class ViewModelBase : ObservableObject, IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Disposes the view model.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the view model.
    /// </summary>
    /// <param name="disposing">True if called from <see cref="Dispose()"/>, false otherwise.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Dispose managed resources
        }

        _disposed = true;
    }
}