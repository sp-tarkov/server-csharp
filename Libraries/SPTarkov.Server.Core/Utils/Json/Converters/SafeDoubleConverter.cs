using System.Text.Json;
using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

public class SafeDoubleConverter : JsonConverter<double>
{
    private static double GetFiniteValue(double value)
    {
        return double.IsFinite(value) ? value : 0;
    }

    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                try
                {
                    return GetFiniteValue(reader.GetDouble());
                }
                catch (FormatException)
                {
                    try
                    {
                        var decimalValue = reader.GetDecimal();
                        return decimalValue > 0 ? double.MaxValue : 0;
                    }
                    catch
                    {
                        return double.MaxValue;
                    }
                }

            case JsonTokenType.String:
                if (double.TryParse(reader.GetString(), out var stringParsed))
                {
                    return GetFiniteValue(stringParsed);
                }
                return 0;

            case JsonTokenType.Null:
                return 0;

            default:
                return 0;
        }
    }

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(GetFiniteValue(value));
    }
}
