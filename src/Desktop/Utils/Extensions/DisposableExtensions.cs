using System;

namespace EnhancedYoutubeDownloader.Utils.Extensions;

public static class DisposableExtensions
{
    public static T AddTo<T>(this T disposable, DisposableCollector collector)
        where T : IDisposable
    {
        collector.Add(disposable);
        return disposable;
    }
}
