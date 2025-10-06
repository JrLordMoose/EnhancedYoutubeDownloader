using System;

namespace EnhancedYoutubeDownloader.Utils.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IDisposable"/> objects.
/// </summary>
public static class DisposableExtensions
{
    /// <summary>
    /// Adds an <see cref="IDisposable"/> object to a <see cref="DisposableCollector"/>.
    /// </summary>
    /// <typeparam name="T">The type of the disposable object.</typeparam>
    /// <param name="disposable">The disposable object to add.</param>
    /// <param name="collector">The collector to add the disposable object to.</param>
    /// <returns>The disposable object.</returns>
    public static T AddTo<T>(this T disposable, DisposableCollector collector)
        where T : IDisposable
    {
        collector.Add(disposable);
        return disposable;
    }
}