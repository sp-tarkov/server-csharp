using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

public class DogtagSideConverter : JsonConverter<DogtagSide>
{
    public override DogtagSide Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return DogtagSide.NotApplicable;
        }

        var value = reader.GetString();
        return value != null ? Enum.Parse<DogtagSide>(value) : DogtagSide.NotApplicable;
    }

    public override void Write(
        Utf8JsonWriter writer,
        DogtagSide value,
        JsonSerializerOptions options
    )
    {
        switch (value)
        {
            case DogtagSide.NotApplicable:
                writer.WriteNumberValue(0);
                break;
            case DogtagSide.Usec:
            case DogtagSide.Bear:
                writer.WriteStringValue(value.ToString());
                break;
        }
    }
}
