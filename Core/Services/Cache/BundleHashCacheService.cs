namespace Core.Services.Cache;

public class BundleHashCacheService
{
    public int GetStoredValue(string key)
    {
        throw new NotImplementedException();
    }

    public void StoreValue(string key, int value)
    {
        throw new NotImplementedException();
    }

    public bool MatchWithStoredHash(string bundlePath, int hash)
    {
        throw new NotImplementedException();
    }

    public bool CalculateAndMatchHash(string bundlePath)
    {
        throw new NotImplementedException();
    }

    public void CalculateAndStoreHash(string bundlePath)
    {
        throw new NotImplementedException();
    }
}
