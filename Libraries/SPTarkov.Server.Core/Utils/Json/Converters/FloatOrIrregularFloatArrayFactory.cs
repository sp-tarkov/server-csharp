using System.Text.Json;
using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

public class FloatOrIrregularFloatArrayFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(FloatOrIrregularFloatArray);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return new FloatOrIrregularFloatArrayConverter();
    }
}

public class FloatOrIrregularFloatArrayConverter : JsonConverter<FloatOrIrregularFloatArray>
{
    public override FloatOrIrregularFloatArray? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return new FloatOrIrregularFloatArray(null, null);

            case JsonTokenType.Number:
                return new FloatOrIrregularFloatArray(reader.GetSingle(), null);

            case JsonTokenType.StartArray:
                var outer = new List<float[]>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        return new FloatOrIrregularFloatArray(null, outer.ToArray());
                    }

                    if (reader.TokenType == JsonTokenType.Null)
                    {
                        outer.Add(null!);
                        continue;
                    }

                    if (reader.TokenType != JsonTokenType.StartArray)
                    {
                        throw new JsonException(
                            $"Expected StartArray or Null when reading inner array of FloatOrIrregularFloatArray, got {reader.TokenType}."
                        );
                    }

                    var inner = new List<float>();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        if (reader.TokenType != JsonTokenType.Number)
                        {
                            throw new JsonException(
                                $"Expected Number when reading inner array of FloatOrIrregularFloatArray, got {reader.TokenType}."
                            );
                        }

                        inner.Add(reader.GetSingle());
                    }

                    outer.Add(inner.ToArray());
                }

                throw new JsonException("Unexpected end of JSON when reading FloatOrIrregularFloatArray.");

            default:
                throw new JsonException($"Unable to translate token type {reader.TokenType} to FloatOrIrregularFloatArray.");
        }
    }

    public override void Write(Utf8JsonWriter writer, FloatOrIrregularFloatArray value, JsonSerializerOptions options)
    {
        if (value.IsFloat)
        {
            writer.WriteNumberValue(value.Float!.Value);
            return;
        }

        if (value.IsIrregularFloatArray)
        {
            writer.WriteStartArray();
            foreach (var inner in value.IrregularFloatArray!)
            {
                if (inner is null)
                {
                    writer.WriteNullValue();
                    continue;
                }

                writer.WriteStartArray();
                foreach (var f in inner)
                {
                    writer.WriteNumberValue(f);
                }
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
            return;
        }

        writer.WriteNullValue();
    }
}
