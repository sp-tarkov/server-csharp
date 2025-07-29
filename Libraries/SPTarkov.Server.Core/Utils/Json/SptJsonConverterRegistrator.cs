using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Utils.Json;

[Injectable]
public class SptJsonConverterRegistrator : IJsonConverterRegistrator
{
    public IEnumerable<JsonConverter> GetJsonConverters()
    {
        return
        [
            new BaseSptLoggerReferenceConverter(),
            new ListOrTConverterFactory(),
            new DictionaryOrListConverter(),
            new BaseInteractionRequestDataConverter(),
            new StringToMongoIdConverter(),
            new EftEnumConverterFactory(),
            new EftListEnumConverterFactory(),
            new EnumerableConverterFactory(),
        ];
    }
}
