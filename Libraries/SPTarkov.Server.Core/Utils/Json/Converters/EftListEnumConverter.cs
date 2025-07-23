using System.Text.Json;
using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

public class EftListEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType
            && typeToConvert.GetGenericTypeDefinition() == typeof(List<>)
            && typeToConvert.GenericTypeArguments[0].IsEnum
            && typeToConvert.IsDefined(typeof(EftListEnumConverterAttribute), false);
    }

    public override JsonConverter? CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return (JsonConverter)
            Activator.CreateInstance(
                typeof(EftListEnumConverter<>).MakeGenericType(
                    typeToConvert.GenericTypeArguments[0]
                )
            );
    }
}

public class EftListEnumConverter<T> : JsonConverter<List<T>>
{
    private static readonly JsonSerializerOptions _options = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public override List<T>? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            return JsonSerializer.Deserialize<List<T>>(ref reader, _options);
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var x1 in value)
        {
            JsonSerializer.Serialize(writer, x1, _options);
        }

        writer.WriteEndArray();
    }
}

/// <summary>
/// This attribute should be applied to enums which should be added as a converter to the json converter
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public class EftListEnumConverterAttribute : Attribute { }
