namespace Core.Utils.Cloners;

/**
 * Disabled as FastCloner library is 15% faster and consumes less memory than Json serialization
 */
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
