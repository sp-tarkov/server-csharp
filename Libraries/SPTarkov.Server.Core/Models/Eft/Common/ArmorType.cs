using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ArmorType
{
    [JsonPropertyName("Destructibility")]
    public double? Destructibility
    {
        get;
        set;
    }

    [JsonPropertyName("MinRepairDegradation")]
    public double? MinRepairDegradation
    {
        get;
        set;
    }

    [JsonPropertyName("MaxRepairDegradation")]
    public double? MaxRepairDegradation
    {
        get;
        set;
    }

    [JsonPropertyName("ExplosionDestructibility")]
    public double? ExplosionDestructibility
    {
        get;
        set;
    }

    [JsonPropertyName("MinRepairKitDegradation")]
    public double? MinRepairKitDegradation
    {
        get;
        set;
    }

    [JsonPropertyName("MaxRepairKitDegradation")]
    public double? MaxRepairKitDegradation
    {
        get;
        set;
    }
}
