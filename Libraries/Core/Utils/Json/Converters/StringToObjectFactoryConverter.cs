using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Utils.Json.Converters;

public class StringToObjectFactoryConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return true;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter)Activator.CreateInstance(typeof(StringToObjectConverter<>).MakeGenericType(typeToConvert));
    }

    public class StringToObjectConverter<T> : JsonConverter<T>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
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
                value = default;
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
