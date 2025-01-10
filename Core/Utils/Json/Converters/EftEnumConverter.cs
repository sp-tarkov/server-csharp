using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Utils.Json.Converters;

public class EftEnumConverter<T> : JsonConverter<T>
{
    private static readonly JsonSerializerOptions _options = new() {Converters = { new JsonStringEnumConverter() }};
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String || reader.TokenType == JsonTokenType.PropertyName)
        {
            var str = reader.GetString();
            return (T)Enum.Parse(typeof(T), str, true);
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            var str = reader.GetInt32().ToString();
            return (T)Enum.Parse(typeof(T), str, true);
        }
        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, _options);
    }

    public override T ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Read(ref reader, typeToConvert, options);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, _options);
    }
}
