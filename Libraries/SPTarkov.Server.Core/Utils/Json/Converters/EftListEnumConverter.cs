using System.Text.Json;
using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

public class EftListEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType
            && typeToConvert.GetGenericTypeDefinition() == typeof(List<>)
            && typeToConvert.GenericTypeArguments[0].IsEnum;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return Activator.CreateInstance(typeof(EftListEnumConverter<>).MakeGenericType(typeToConvert.GenericTypeArguments[0]))
            as JsonConverter;
    }
}

public class EftListEnumConverter<T> : JsonConverter<List<T>>
{
    // We have to use these options here, because down below if we use the options passed we create a stack overflow
    // Due to the converter trying to use itself
    private static readonly JsonSerializerOptions _options = new()
    {
        Converters = { new JsonStringEnumConverter(), new EftEnumConverterFactory() },
    };

    public override List<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions _)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            return JsonSerializer.Deserialize<List<T>>(ref reader, _options);
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions _)
    {
        writer.WriteStartArray();
        foreach (var x1 in value)
        {
            JsonSerializer.Serialize(writer, x1, _options);
        }

        writer.WriteEndArray();
    }
}
