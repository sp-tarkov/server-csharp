namespace SPTarkov.Server.Core.Utils.Cloners;

/// <summary>
///     Disabled as FastCloner library is 15% faster and consumes less memory than Json serialization
/// </summary>
public class JsonCloner(JsonUtil jsonUtil) : ICloner
{
    public T? Clone<T>(T? obj)
    {
        return jsonUtil.Deserialize<T>(jsonUtil.Serialize(obj));
    }
}
