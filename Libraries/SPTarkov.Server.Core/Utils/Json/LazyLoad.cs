namespace SPTarkov.Server.Core.Utils.Json;

public class LazyLoad<T>(Func<T> deserialize)
{
    private readonly List<Func<T?, T?>> _lazyLoadTransformers = [];
    private readonly ReaderWriterLockSlim _lazyLoadTransformersLock = new();
    private static readonly TimeSpan _autoCleanerTimeout = TimeSpan.FromSeconds(30);
    private bool _isLoaded;
    private T? _result;

    private Timer? autoCleanerTimeout;

    /// <summary>
    /// Adds a transformer to modify the value during lazy loading. Transformers execute
    /// in registration order and the final result is cached until auto-cleanup.
    /// </summary>
    /// <param name="transformer">Function that transforms the value</param>
    public void AddTransformer(Func<T?, T?> transformer)
    {
        _lazyLoadTransformersLock.EnterWriteLock();

        try
        {
            _lazyLoadTransformers.Add(transformer);
        }
        finally
        {
            _lazyLoadTransformersLock.ExitWriteLock();
        }
    }

    public T? Value
    {
        get
        {
            if (!_isLoaded)
            {
                _result = deserialize();
                _isLoaded = true;

                _lazyLoadTransformersLock.EnterReadLock();
                try
                {
                    foreach (var transform in _lazyLoadTransformers)
                    {
                        _result = transform(_result);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    _lazyLoadTransformersLock.ExitReadLock();
                }

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
