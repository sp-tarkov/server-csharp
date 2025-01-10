using Core.Annotations;

namespace Core.Utils.Cloners;

[Injectable]
public class JsonCloner : ICloner
{
    protected JsonUtil _jsonUtil;
    public JsonCloner(JsonUtil jsonUtil)
    {
        _jsonUtil = jsonUtil;
    }
    public T Clone<T>(T obj)
    {
        return _jsonUtil.Deserialize<T>(_jsonUtil.Serialize(obj));
    }
}
