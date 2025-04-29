using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

/// <summary>
/// Allow the parsing of up to three different basic value. You can parse an enum, a string, or a number
/// </summary>
public class ObjectToTupleFactoryConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType) return false;
        var generic = typeToConvert.GetGenericTypeDefinition();
        return generic == typeof(Tuple<,>) || generic == typeof(Tuple<,,>) || generic == typeof(Tuple<,,,>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter) Activator.CreateInstance(typeof(ObjectToTupleConverter<>).MakeGenericType(typeToConvert));
    }

    public static IEnumerable<PropertyInfo> GetProperties(Type typeToConvert) => typeToConvert
        .GetProperties()
        .Where(p => p.CanRead)
        .Where(p => !p.GetIndexParameters().Any())
        .Where(p => Regex.IsMatch(p.Name, "^Item[0-9]+$"));

    private class ObjectToTupleConverter<T> : JsonConverter<T>
    {

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var underlyingType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
            if (!underlyingType.Name.Contains("Tuple"))
                throw new JsonException("invalid tuple passed");
            var constructors = typeToConvert.GetConstructors();
            if (constructors.Length == 0) throw new JsonException("invalid tuple passed");
            var types = GetProperties(typeToConvert)
                .Select(p => p.PropertyType)
                .ToArray();
            if (types is not { Length: >= 1 }) throw new JsonException("invalid tuple passed");

            var values = new object?[types.Length];
            var success = false;
            for (int i = 0; i < types.Length; i++)
            {
                try
                {
                    // if we already have success, following values should be null
                    if (success)
                    {
                        values[i] = null;
                        continue;
                    }
                    var val = TryParseType(reader, types[i], options);
                    if (val is not null) success = true;
                    values[i] = val;
                }
                catch (Exception ex)
                {
                    values[i] = null;
                }
            }
            return (T)constructors[0].Invoke(values);
        }

        private object TryParseType(Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var underlyingType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;

            // handle enum from string
            if (
                underlyingType.IsEnum &&
                reader.TokenType == JsonTokenType.String &&
                Enum.TryParse(underlyingType, reader.GetString(), out var result)
            )
            {
                return result;
            }

            if (reader.TokenType == JsonTokenType.String && underlyingType.Name == "String")
                return reader.GetString();
            if (reader.TokenType == JsonTokenType.Number && (underlyingType.Name.Contains("Int"))) return reader.GetInt32();
            if (reader.TokenType == JsonTokenType.Number && underlyingType.Name.Contains("Decimal")) return reader.GetDecimal();
            return null;
        }

        public record ParseType
        {
            [JsonPropertyName("Item1")]
            public object? First { get; set; }
            [JsonPropertyName("Item2")]
            public object? Second { get; set; }
            [JsonPropertyName("Item3")]
            public object? Third { get; set; }
        }

        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
        {
            var type = value.GetType();
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            if (!underlyingType.Name.Contains("Tuple"))
                throw new JsonException("invalid tuple passed");
            var possibleValue = GetProperties(type)
                .Select(p => p.GetValue(value))
                .Where(p => p is not null)
                .ToArray()
                .First();
            JsonSerializer.Serialize(writer, possibleValue, options);
        }
    }
}
