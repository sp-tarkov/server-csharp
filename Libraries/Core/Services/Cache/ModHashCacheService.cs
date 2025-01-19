namespace Core.Services.Cache;

public class ModHashCacheService
{
    public string GetStoredValue(string key)
    {
        throw new NotImplementedException();
    }

    public void StoreValue(string key, string value)
    {
        throw new NotImplementedException();
    }

    public bool MatchWithStoredHash(string modName, string hash)
    {
        throw new NotImplementedException();
    }

    public bool CalculateAndCompareHash(string modName, string modContent)
    {
        throw new NotImplementedException();
    }

    public void CalculateAndStoreHash(string modName, string modContent)
    {
        throw new NotImplementedException();
    }
}
