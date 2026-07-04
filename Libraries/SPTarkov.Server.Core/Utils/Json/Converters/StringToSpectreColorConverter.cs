using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

public class StringToSpectreColorConverter : JsonConverter<Color>
{
    // Colours the client has as an enum
    private static readonly string[] _acceptableColors = ["Black", "Red", "Green", "Yellow", "Blue", "Magenta", "Cyan", "White", "Default", "Gray"];

    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();

            if (_acceptableColors.Contains(value))
            {
                switch (value)
                {
                    case "Black":
                        return Color.Black;
                    case "Red":
                        return Color.Red;
                    case "Green":
                        return Color.Green;
                    case "Yellow":
                        return Color.Yellow;
                    case "Blue":
                        return Color.Blue;
                    case "Magenta":
                        return Color.Magenta;
                    case "Cyan":
                        return Color.Cyan;
                    case "White":
                        return Color.White;
                    case "Gray":
                        return Color.Gray;
                    default:
                        return Color.Default;
                }
            }

            throw new JsonException($"The Color given was not converted: {value}");
        }

        throw new JsonException($"The JsonTokenType was not of type string, it was: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToString(), options);
    }
}
