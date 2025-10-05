using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EnhancedYoutubeDownloader.Utils.Extensions;

public static class NotifyPropertyChangedExtensions
{
    public static IDisposable WatchProperty<TSource, TProperty>(
        this TSource source,
        Func<TSource, TProperty> propertySelector,
        Action callback,
        bool callCallbackImmediately = false
    )
    {
        if (callCallbackImmediately)
            callback();

        // Check if source implements INotifyPropertyChanged
        if (source is INotifyPropertyChanged notifySource)
        {
            return new PropertyWatcher<TSource, TProperty>(
                notifySource,
                source,
                propertySelector,
                callback
            );
        }

        // If not, return a dummy disposable
        return new DummyDisposable();
    }

    private class PropertyWatcher<TSource, TProperty> : IDisposable
    {
        private readonly INotifyPropertyChanged _notifySource;
        private readonly TSource _source;
        private readonly Func<TSource, TProperty> _propertySelector;
        private readonly Action _callback;
        private TProperty? _lastValue;

        public PropertyWatcher(
            INotifyPropertyChanged notifySource,
            TSource source,
            Func<TSource, TProperty> propertySelector,
            Action callback
        )
        {
            _notifySource = notifySource;
            _source = source;
            _propertySelector = propertySelector;
            _callback = callback;
            _lastValue = propertySelector(source);

            _notifySource.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var newValue = _propertySelector(_source);
            if (!Equals(_lastValue, newValue))
            {
                _lastValue = newValue;
                _callback();
            }
        }

        public void Dispose()
        {
            _notifySource.PropertyChanged -= OnPropertyChanged;
        }
    }

    private class DummyDisposable : IDisposable
    {
        public void Dispose() { }
    }
}
