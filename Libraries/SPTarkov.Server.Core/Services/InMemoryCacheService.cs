using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class InMemoryCacheService(
    ICloner _cloner
)
{
    protected Dictionary<string, object?> _cacheData = new();

    /// <summary>
    ///     Store data into an in-memory object
    /// </summary>
    /// <param name="key"> Key to store data against </param>
    /// <param name="dataToCache"> Data to store in cache </param>
    public void StoreByKey<T>(string key, T dataToCache)
    {
        _cacheData[key] = _cloner.Clone(dataToCache);
    }

    /// <summary>
    ///     Retrieve data stored by a key
    /// </summary>
    /// <param name="key"> key</param>
    /// <returns> Stored data </returns>
    public T? GetDataByKey<T>(string key)
    {
        if (_cacheData.ContainsKey(key))
        {
            return (T) _cacheData[key];
        }

        return default;
    }

    /// <summary>
    ///     Does data exist against the provided key
    /// </summary>
    /// <param name="key"> Key to check for data against </param>
    /// <returns> True if exists </returns>
    public bool HasStoredDataByKey(string key)
    {
        return _cacheData.ContainsKey(key);
    }

    /// <summary>
    ///     Remove data stored against key
    /// </summary>
    /// <param name="key"> Key to remove data against </param>
    public void ClearDataStoredByKey(string key)
    {
        _cacheData.Remove(key);
    }

    /// <summary>
    ///     Remove all data stored
    /// </summary>
    public void ClearCache()
    {
        _cacheData.Clear();
    }
}
