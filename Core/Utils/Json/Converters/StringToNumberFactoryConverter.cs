using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Utils.Json.Converters;

public class StringToNumberFactoryConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return true;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter)Activator.CreateInstance(typeof(StringToNumberConverter<>).MakeGenericType(typeToConvert));
    }

    private class StringToNumberConverter<T> : JsonConverter<T>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    var value = reader.GetString();
                    if (string.IsNullOrWhiteSpace(value))
                        return default;
                    var type = typeToConvert;
                    try
                    {
                        if (typeToConvert.IsGenericType &&
                            typeToConvert.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            type = typeToConvert.GenericTypeArguments[0];
                        }
                        return (T) type.GetMethods().First(m =>
                            m.Name == "Parse" && m.GetParameters().Length == 1 &&
                            m.GetParameters().First().ParameterType == typeof(string)).Invoke(null, [value]);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Tried to convert {value} into {type.Name} but failed to parse it, null value will be used instead.");
                    }

                    return default;
                case JsonTokenType.Number:
                    using (var jsonDocument = JsonDocument.ParseValue(ref reader))
                    {
                        var jsonText = jsonDocument.RootElement.GetRawText().Replace("\"", "");
                        return JsonSerializer.Deserialize<T>(jsonText);
                    }
                case JsonTokenType.Null:
                    return default;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
        {
            if (value == null)
                value = default;
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
