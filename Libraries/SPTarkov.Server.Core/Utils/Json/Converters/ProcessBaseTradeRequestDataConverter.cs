using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Trade;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

/// <summary>
/// This is necessary to process buying and selling from traders, those are the only types that do this
/// </summary>
public sealed class ProcessBaseTradeRequestDataConverter : JsonConverter<ProcessBaseTradeRequestData>
{
    public override ProcessBaseTradeRequestData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        var jsonText = jsonDocument.RootElement.GetRawText();

        if (!jsonDocument.RootElement.TryGetProperty("type", out var typeElement))
        {
            throw new JsonException("Could not deserialize trade request. Property 'type' is missing.");
        }

        var type = typeElement.GetString();

        return type switch
        {
            ItemEventActions.BUY_FROM_TRADER => JsonSerializer.Deserialize<ProcessBuyTradeRequestData>(jsonText, options),
            ItemEventActions.SELL_TO_TRADER => JsonSerializer.Deserialize<ProcessSellTradeRequestData>(jsonText, options),
            _ => throw new JsonException($"Unhandled trade request type '{type}'."),
        };
    }

    public override void Write(Utf8JsonWriter writer, ProcessBaseTradeRequestData value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
