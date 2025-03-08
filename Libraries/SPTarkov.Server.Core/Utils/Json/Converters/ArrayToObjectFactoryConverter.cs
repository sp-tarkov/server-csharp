using System.Text.Json;
using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

public class ArrayToObjectFactoryConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return true;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter) Activator.CreateInstance(typeof(ArrayToObjectConverter<>).MakeGenericType(typeToConvert));
    }

    private class ArrayToObjectConverter<T> : JsonConverter<T?>
    {
        public override bool HandleNull
        {
            get
            {
                return true;
            }
        }

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartArray:
                    // start array
                    reader.Read();
                    return default;
                case JsonTokenType.StartObject:
                    return JsonSerializer.Deserialize<T>(ref reader, options);
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteStartArray();
                writer.WriteEndArray();
            }
            else
            {
                JsonSerializer.Serialize(writer, value, options);
            }
        }
    }
}
