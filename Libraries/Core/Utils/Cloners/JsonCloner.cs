using System.Text.Json;
using SptCommon.Annotations;

namespace Core.Utils.Cloners;

[Injectable]
public class JsonCloner : ICloner
{
    public T? Clone<T>(T? obj)
    {
        using (MemoryStream ms = new())
        {
            JsonSerializer.Serialize(ms, obj);
            ms.Seek(0, SeekOrigin.Begin);

            return JsonSerializer.Deserialize<T>(ms);
        }
    }
}
