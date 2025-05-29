namespace SPTarkov.Server.Core.Utils.Json;

public class LazyLoad<T>(Func<T> deserialize)
{
    private static readonly TimeSpan _autoCleanerTimeout = TimeSpan.FromSeconds(30);
    private bool _isLoaded;
    private T? _result;

    private Timer? autoCleanerTimeout;

    /// <summary>
    /// <see cref="OnLazyLoad" /> can be subscribed to for mods to modify. It is fired right after lazy loading is complete
    /// and any modification passed to <see cref="OnLazyLoadEventArgs.Value" /> will stay for the duration of this <see cref="LazyLoad{T}"/>'s lifecycle
    /// </summary>
    public event EventHandler<OnLazyLoadEventArgs<T>>? OnLazyLoad;

    public T? Value
    {
        get
        {
            if (!_isLoaded)
            {
                _result = deserialize();
                _isLoaded = true;

                OnLazyLoadEventArgs<T> args = new(_result);
                OnLazyLoad?.Invoke(this, args);

                _result = args.Value;

                autoCleanerTimeout = new Timer(
                    _ =>
                    {
                        _result = default;
                        _isLoaded = false;
                        autoCleanerTimeout?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                        autoCleanerTimeout = null;
                    },
                    null,
                    _autoCleanerTimeout,
                    Timeout.InfiniteTimeSpan
                );
            }

            autoCleanerTimeout?.Change(_autoCleanerTimeout, Timeout.InfiniteTimeSpan);
            return _result;
        }
    }
}

public class OnLazyLoadEventArgs<T>(T value) : EventArgs
{
    public T Value { get; set; } = value;
}
