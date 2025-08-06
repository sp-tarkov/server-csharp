namespace SPTarkov.Server.Core.Utils.Json;

public class LazyLoad<T>(Func<T> deserialize)
{
    private readonly List<Func<T?, T?>> _lazyLoadTransformers = [];
    private readonly ReaderWriterLockSlim _lazyLoadTransformersLock = new();

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
            var result = deserialize();

            _lazyLoadTransformersLock.EnterReadLock();
            try
            {
                foreach (var transform in _lazyLoadTransformers)
                {
                    result = transform(result);
                }
            }
            finally
            {
                _lazyLoadTransformersLock.ExitReadLock();
            }

            return result;
        }
    }
}
