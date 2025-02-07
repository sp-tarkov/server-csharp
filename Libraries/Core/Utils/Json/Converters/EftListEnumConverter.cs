using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Utils.Json.Converters;

public class EftListEnumConverter<T> : JsonConverter<List<T>>
{
    private static readonly JsonSerializerOptions _options = new() { Converters = { new JsonStringEnumConverter() } };

    public override List<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            using (var jsonDocument = JsonDocument.ParseValue(ref reader))
            {
                var jsonText = jsonDocument.RootElement.GetRawText();
                jsonText = jsonText.Replace("[", "").Replace("]", "");
                var list = new List<T>();
                if (!string.IsNullOrEmpty(jsonText))
                {
                    foreach (var str in jsonText.Split(","))
                    {
                        var newStr = str.Replace("\r", "").Replace("\n", "").Trim();
                        list.Add(JsonSerializer.Deserialize<T>(newStr, options));
                    }
                }
                return list;
            }
        }
        
        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var x1 in value)
        {
            JsonSerializer.Serialize(writer, x1, _options);
        }
        
        writer.WriteEndArray();
    }
}
