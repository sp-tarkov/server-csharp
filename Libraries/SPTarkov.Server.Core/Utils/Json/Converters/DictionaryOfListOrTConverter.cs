using System.Text.Json;
using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

public class DictionaryOfListOrTConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
               typeToConvert.GenericTypeArguments[1].IsGenericType &&
               typeToConvert.GenericTypeArguments[1].GetGenericTypeDefinition() == typeof(ListOrT<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter) Activator.CreateInstance(
            typeof(DictionaryOfListOrTConverter<,>).MakeGenericType(
                typeToConvert.GenericTypeArguments[0],
                typeToConvert.GenericTypeArguments[1].GenericTypeArguments[0]
            )
        );
    }
}

public class DictionaryOfListOrTConverter<T, K> : JsonConverter<Dictionary<T, ListOrT<K>>?>
{
    public override Dictionary<T, ListOrT<K>>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            reader.Read();
            return default;
        }

        using (var jsonDocument = JsonDocument.ParseValue(ref reader))
        {
            var jsonText = jsonDocument.RootElement.GetRawText();
            return JsonSerializer.Deserialize<Dictionary<T, ListOrT<K>>>(jsonText, options);
        }
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<T, ListOrT<K>> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}
