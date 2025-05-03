using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

public class EftEnumConverter<T> : JsonConverter<T>
{
    private static readonly JsonSerializerOptions _options = new()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String || reader.TokenType == JsonTokenType.PropertyName)
        {
            var str = reader.GetString();
            return (T) Enum.Parse(typeof(T), str, true);
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            var str = reader.GetInt32().ToString();
            return (T) Enum.Parse(typeof(T), str, true);
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (typeof(T).GetFields().Any(f =>
        {
            return f.FieldType == typeof(string);
        }))
        {
            JsonSerializer.Serialize(writer, value as string, _options);
        }
        else
        {
            if (typeof(T).GetFields().Any(f =>
            {
                return f.FieldType == typeof(int);
            }))
            {
                JsonSerializer.Serialize(writer, Convert.ToInt32(value), _options);
            }
            else if (typeof(T).GetFields().Any(f =>
            {
                return f.FieldType == typeof(byte);
            }))
            {
                JsonSerializer.Serialize(writer, Convert.ToByte(value), _options);
            }
            else
            {
                throw new Exception($"Could not convert enum {value.GetType()} with value {value}");
            }
        }
    }

    public override T ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Read(ref reader, typeToConvert, options);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] T value, JsonSerializerOptions options)
    {
        object propertyValue = null;
        if (typeof(T).GetFields().Any(f =>
        {
            return f.FieldType == typeof(string);
        }))
        {
            propertyValue = value as string;
        }
        else
        {
            if (typeof(T).GetFields().Any(f =>
            {
                return f.FieldType == typeof(int);
            }))
            {
                propertyValue = Convert.ToInt32(value);
            }
            else if (typeof(T).GetFields().Any(f =>
            {
                return f.FieldType == typeof(byte);
            }))
            {
                propertyValue = Convert.ToByte(value);
            }
            else
            {
                throw new Exception($"Could not convert enum {value.GetType()} with value {value}");
            }
        }

        writer.WritePropertyName(propertyValue.ToString());
    }
}
