using System;
using System.Collections.Generic;

namespace EnhancedYoutubeDownloader.Utils;

public class DisposableCollector : IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private bool _disposed;

    public void Add(IDisposable disposable)
    {
        if (_disposed)
        {
            disposable?.Dispose();
            return;
        }

        _disposables.Add(disposable);
    }

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
