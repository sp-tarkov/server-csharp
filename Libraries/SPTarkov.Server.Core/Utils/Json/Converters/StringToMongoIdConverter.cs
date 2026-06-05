using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

public sealed class StringToMongoIdConverter : JsonConverter<MongoId>
{
    public override MongoId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not JsonTokenType.String)
        {
            throw new JsonException($"The JsonTokenType was not of type string, it was: {reader.TokenType}");
        }

        return Read(ref reader);
    }

    // Deserialize MongoId as a dictionary key
    public override MongoId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Read(ref reader);
    }

    private static MongoId Read(ref Utf8JsonReader reader)
    {
        if (!reader.HasValueSequence && !reader.ValueIsEscaped)
        {
            return new MongoId(reader.ValueSpan);
        }

        Span<char> buffer = stackalloc char[24];
        var written = reader.CopyString(buffer);
        return new MongoId(buffer[..written]);
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
