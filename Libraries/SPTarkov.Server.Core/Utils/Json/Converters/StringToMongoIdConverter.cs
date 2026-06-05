using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

public sealed class StringToMongoIdConverter : JsonConverter<MongoId>
{
    public override MongoId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new MongoId(reader.GetString());
        }

        throw new JsonException($"The JsonTokenType was not of type string, it was: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, MongoId mongoId, JsonSerializerOptions options)
    {
        Span<char> buffer = stackalloc char[24];
        if (mongoId.TryFormat(buffer, out var charsWritten))
        {
            writer.WriteStringValue(buffer[..charsWritten]);
        }
        else
        {
            throw new JsonException("Failed to format MongoId to stack buffer.");
        }
    }

    // Deserialize MongoId as a dictionary key
    public override MongoId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new MongoId(reader.GetString());
    }

    // Serialize MongoId as a dictionary key
    public override void WriteAsPropertyName(Utf8JsonWriter writer, MongoId value, JsonSerializerOptions options)
    {
        Span<char> buffer = stackalloc char[24];
        if (value.TryFormat(buffer, out var charsWritten))
        {
            if (charsWritten == 0)
            {
                writer.WritePropertyName(string.Empty);
            }
            else
            {
                writer.WritePropertyName(buffer[..charsWritten]);
            }
        }
        else
        {
            throw new JsonException("Failed to format MongoId to stack buffer.");
        }
    }
}
