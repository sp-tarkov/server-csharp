using System.Text.Json;
using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

public class StringOrIntConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(StringOrInt);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return new StringOrIntConverter();
    }
}

public class StringOrIntConverter : JsonConverter<StringOrInt>
{
    public override StringOrInt? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return new StringOrInt(reader.GetString(), null);

            case JsonTokenType.Number:
                if (reader.TryGetInt32(out var intValue))
                {
                    return new StringOrInt(null, intValue);
                }
                break;

            case JsonTokenType.Null:
                return new StringOrInt(null, null);

            default:
                throw new Exception($"Unable to translate object type {reader.TokenType} to StringOrInt.");
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, StringOrInt value, JsonSerializerOptions options)
    {
        if (value.IsString)
        {
            JsonSerializer.Serialize(writer, value.String, options);
        }
        else
        {
            JsonSerializer.Serialize(writer, value.Int!.Value, options);
        }
    }
}
