using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Utils.Json.Converters;

public class ArrayToObjectFactoryConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return true;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter)Activator.CreateInstance(typeof(ArrayToObjectConverter<>).MakeGenericType(typeToConvert));
    }

    private class ArrayToObjectConverter<T> : JsonConverter<T?>
    {
        public override bool HandleNull => true;
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartArray:
                    // start array
                    reader.Read();
                    return default;
                case JsonTokenType.StartObject:
                    using (var jsonDocument = JsonDocument.ParseValue(ref reader))
                    {
                        var jsonText = jsonDocument.RootElement.GetRawText();
                        return JsonSerializer.Deserialize<T>(jsonText);
                    }
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
        {
            if (value == null)
                JsonSerializer.Serialize(writer, new List<object>(), options);
            else
                JsonSerializer.Serialize(writer, value, options);
        }
    }
}
