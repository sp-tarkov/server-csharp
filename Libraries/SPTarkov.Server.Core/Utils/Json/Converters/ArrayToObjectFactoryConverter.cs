using System.Text.Json;
using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

public class ArrayToObjectFactoryConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsClass;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return Activator.CreateInstance(typeof(ArrayToObjectConverter<>).MakeGenericType(typeToConvert)) as JsonConverter;
    }

    private class ArrayToObjectConverter<T> : JsonConverter<T?>
    {
        public override bool HandleNull
        {
            get { return true; }
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
            if (EqualityComparer<T>.Default.Equals(value, default))
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
