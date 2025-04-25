using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record UpdBuff
{
    [JsonPropertyName("Rarity")]
    public string? Rarity
    {
        get;
        set;
    }

    [JsonPropertyName("BuffType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BuffType? BuffType
    {
        get;
        set;
    }

    [JsonPropertyName("Value")]
    public double? Value
    {
        get;
        set;
    }

    [JsonPropertyName("ThresholdDurability")]
    public double? ThresholdDurability
    {
        get;
        set;
    }
}
