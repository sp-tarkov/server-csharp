using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Utils.Json.Converters;

public class NullableObjectToLongConverter : JsonConverter<long?>
{
    public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        long result;
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var value = reader.GetString();
                if (string.IsNullOrWhiteSpace(value) || !long.TryParse(value, out result))
                    return null;
                break;
            case JsonTokenType.Number:
                result = reader.GetInt64();
                break;
            case JsonTokenType.Null:
                return null;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return result;
    }

    public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
    {
        if (value == null)
            writer.WriteStringValue("");
        else if (value is long longValue)
            writer.WriteNumberValue(longValue);
        else
            throw new Exception("Cannot convert the object valur to a long.");
    }
}