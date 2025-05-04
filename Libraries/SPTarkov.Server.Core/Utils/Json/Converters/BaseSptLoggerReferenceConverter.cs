using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Utils.Logger;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

public class BaseSptLoggerReferenceConverter : JsonConverter<BaseSptLoggerReference>
{
    public override BaseSptLoggerReference? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        using (var jsonDocument = JsonDocument.ParseValue(ref reader))
        {
            if (!jsonDocument.RootElement.TryGetProperty("type", out var typeElement))
            {
                throw new Exception("One of the loggers doesnt have a type property defined.");
            }

            switch (typeElement.GetString())
            {
                case "File":
                    return jsonDocument.Deserialize<FileSptLoggerReference>();
                case "Console":
                    return jsonDocument.Deserialize<ConsoleSptLoggerReference>();
                default:
                    throw new Exception(
                        $"The logger type '{typeElement.GetString()}' does not exist."
                    );
            }
        }
    }

    public override void Write(
        Utf8JsonWriter writer,
        BaseSptLoggerReference value,
        JsonSerializerOptions options
    ) { }
}
