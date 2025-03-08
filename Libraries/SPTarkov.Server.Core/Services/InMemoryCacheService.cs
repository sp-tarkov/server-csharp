using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class InMemoryCacheService(
    ICloner _cloner
)
{
    protected Dictionary<string, object?> _cacheData = new();

    // Store data into an in-memory object
    // key to store data against
    // Data to store in cache
    public void StoreByKey<T>(string key, T dataToCache)
    {
        _cacheData[key] = _cloner.Clone(dataToCache);
    }

    // Retrieve data stored by a key
    // key
    // Stored data
    public T? GetDataByKey<T>(string key)
    {
        if (_cacheData.ContainsKey(key))
        {
            return (T) _cacheData[key];
        }

        return default;
    }

    // Does data exist against the provided key
    // Key to check for data against
    // true if exists
    public bool HasStoredDataByKey(string key)
    {
        return _cacheData.ContainsKey(key);
    }

    // Remove data stored against key
    // Key to remove data against
    public void ClearDataStoredByKey(string key)
    {
        _cacheData.Remove(key);
    }

    // Remove all data stored
    public void ClearCache()
    {
        _cacheData.Clear();
    }
}
