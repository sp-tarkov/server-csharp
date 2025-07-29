using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Utils.Json;

public interface IJsonConverterRegistrator
{
    public IEnumerable<JsonConverter> GetJsonConverters();
}
