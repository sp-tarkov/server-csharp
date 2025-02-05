namespace Core.Utils.Json;

public class LazyLoad<T>(Func<T> deserialize)
{
    private static readonly TimeSpan _autoCleanerTimeout = TimeSpan.FromSeconds(30);
    private bool _isLoaded;
    private T? _result;

    private Timer? autoCleanerTimeout;

    public T? Value
    {
        get
        {
            if (!_isLoaded)
            {
                _result = deserialize();
                _isLoaded = true;
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
        set => _result = value;
    }
}
