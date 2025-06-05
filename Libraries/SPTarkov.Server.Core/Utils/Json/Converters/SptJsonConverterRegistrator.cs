using System.Reflection;
using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

[Injectable]
public class SptJsonConverterRegistrator : IJsonConverterRegistrator
{
    public IEnumerable<JsonConverter> GetJsonConverters()
    {
        return [
            new BaseSptLoggerReferenceConverter(),
            new ListOrTConverterFactory(),
            new DictionaryOrListConverter(),

            new EftEnumConverter<LogLevel>(), // Special case, this belongs to a lib.
            new BaseInteractionRequestDataConverter(),

            ..GetGenericJsonConverters()
        ];
    }

    private static List<JsonConverter> GetGenericJsonConverters()
    {
        var enums = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsEnum && type.GetCustomAttribute<EftEnumConverterAttribute>() != null);

        var listEnums = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsEnum && type.GetCustomAttribute<EftListEnumConverterAttribute>() != null);

        var result = enums.Select(e => (JsonConverter) Activator.CreateInstance(typeof(EftEnumConverter<>).MakeGenericType(e))!).ToList();
        result.AddRange(listEnums.Select(e => (JsonConverter) Activator.CreateInstance(typeof(EftListEnumConverter<>).MakeGenericType(e))!));

        return result;
    }
}
