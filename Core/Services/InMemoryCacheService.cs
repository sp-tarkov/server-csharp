using Core.Annotations;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class InMemoryCacheService
{
    // Store data into an in-memory object
    // key to store data against
    // Data to store in cache
    public void StoreByKey(string key, object dataToCache)
    {
        throw new NotImplementedException();
    }

    // Retreve data stored by a key
    // key
    // Stored data
    public T GetDataByKey<T>(string key)
    {
        throw new NotImplementedException();
    }

    // Does data exists against the provided key
    // Key to check for data against
    // true if exists
    public bool HasStoredDataByKey(string key)
    {
        throw new NotImplementedException();
    }

    // Remove data stored against key
    // Key to remove data against
    public void ClearDataStoredByKey(string key)
    {
        throw new NotImplementedException();
    }
}
