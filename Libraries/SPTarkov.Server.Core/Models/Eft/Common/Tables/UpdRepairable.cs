using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record UpdRepairable
{
    [JsonPropertyName("Durability")]
    public double? Durability
    {
        get;
        set;
    }

    [JsonPropertyName("MaxDurability")]
    public double? MaxDurability
    {
        get;
        set;
    }
}
