using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Utils.Json.Converters;

public class NotNullObjectToIntConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        int result;
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var value = reader.GetString();
                if (string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out result))
                    return 0;
                break;
            case JsonTokenType.Number:
                result = reader.GetInt32();
                break;
            case JsonTokenType.Null:
                return 0;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return result;
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        if (value == null)
            writer.WriteStringValue("");
        else
            writer.WriteStringValue($"{value}");
    }
}