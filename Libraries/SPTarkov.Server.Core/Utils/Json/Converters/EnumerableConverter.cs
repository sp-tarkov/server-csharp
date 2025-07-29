using System.Text.Json;
using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

public class EnumerableConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(IEnumerable<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return Activator.CreateInstance(typeof(EnumerableConverter<>).MakeGenericType(typeToConvert.GenericTypeArguments[0]))
            as JsonConverter;
    }
}

public class EnumerableConverter<T> : JsonConverter<IEnumerable<T>?>
{
    public override IEnumerable<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
            case JsonTokenType.Number:
            case JsonTokenType.StartObject:
                throw new Exception($"Error attempting to deserialize object, its not a valid array. Type {reader.TokenType}");

            case JsonTokenType.StartArray:
                var list = JsonSerializer.Deserialize<List<T>>(ref reader, options);
                return list;
            default:
                throw new Exception($"Unable to translate object type {reader.TokenType} to ListOrT<T>.");
        }
    }

    public override void Write(Utf8JsonWriter writer, IEnumerable<T>? value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value?.ToList(), options);
    }
}
