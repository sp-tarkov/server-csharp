namespace SPTarkov.Server.Core.Utils.Cloners;

/// <summary>
/// Disabled as FastCloner library is 15% faster and consumes less memory than Json serialization
/// </summary>
public class JsonCloner : ICloner
{
    protected JsonUtil _jsonUtil;

    public JsonCloner(JsonUtil jsonUtil)
    {
        _jsonUtil = jsonUtil;
    }

    public T? Clone<T>(T? obj)
    {
        return _jsonUtil.Deserialize<T>(_jsonUtil.Serialize(obj));
    }
}
