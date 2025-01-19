using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Utils.Json.Converters;

public class ListOrTConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(ListOrT<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter)Activator.CreateInstance(typeof(ListOrTConverter<>).MakeGenericType(typeToConvert.GenericTypeArguments[0]));
    }
}

public class ListOrTConverter<T> : JsonConverter<ListOrT<T>?>
{
    public override ListOrT<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.StartArray:
                using (var jsonDocument = JsonDocument.ParseValue(ref reader))
                {
                    var jsonText = jsonDocument.RootElement.GetRawText();
                    var list = JsonSerializer.Deserialize<List<T>>(jsonText, options);
                    return new ListOrT<T>(list, default);
                }
            case JsonTokenType.StartObject:
                using (var jsonDocument = JsonDocument.ParseValue(ref reader))
                {
                    var jsonText = jsonDocument.RootElement.GetRawText();
                    var obj = JsonSerializer.Deserialize<T>(jsonText, options);
                    return new ListOrT<T?>(null, obj);
                }
            default:
                throw new Exception($"Unable to translate object type {reader.TokenType} to ListOrT<T>.");
        }
    }

    public override void Write(Utf8JsonWriter writer, ListOrT<T> value, JsonSerializerOptions options)
    {
        if (value.IsItem)
        {
            JsonSerializer.Serialize(writer, value.Item, options);
        }
        else
        {
            JsonSerializer.Serialize(writer, value.List, options);
        }
    }
}
