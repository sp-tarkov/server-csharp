using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Utils.Json.Converters;

public class DictionaryOrListConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(DictionaryOrList<,>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter) Activator.CreateInstance(
            typeof(DictionaryOrListConverter<,>).MakeGenericType(typeToConvert.GenericTypeArguments[0], typeToConvert.GenericTypeArguments[1])
        );
    }
}

public class DictionaryOrListConverter<K, V> : JsonConverter<DictionaryOrList<K, V>?>
{
    public override DictionaryOrList<K, V>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.StartArray:
                using (var jsonDocument = JsonDocument.ParseValue(ref reader))
                {
                    var jsonText = jsonDocument.RootElement.GetRawText();
                    var list = JsonSerializer.Deserialize<List<V>>(jsonText, options);
                    return new DictionaryOrList<K, V>(null, list);
                }
            case JsonTokenType.StartObject:
                using (var jsonDocument = JsonDocument.ParseValue(ref reader))
                {
                    var jsonText = jsonDocument.RootElement.GetRawText();
                    var dictionary = JsonSerializer.Deserialize<Dictionary<K, V>>(jsonText, options);
                    return new DictionaryOrList<K, V>(dictionary, null);
                }
            default:
                throw new Exception($"Unable to translate object type {reader.TokenType} to ListOrT<T>.");
        }
    }

    public override void Write(Utf8JsonWriter writer, DictionaryOrList<K, V> value, JsonSerializerOptions options)
    {
        if (value.IsList)
        {
            JsonSerializer.Serialize(writer, value.List, options);
        }
        else
        {
            JsonSerializer.Serialize(writer, value.Dictionary, options);
        }
    }
}
