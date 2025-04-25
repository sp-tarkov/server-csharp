using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record RepairSettings
{
    [JsonPropertyName("ItemEnhancementSettings")]
    public ItemEnhancementSettings? ItemEnhancementSettings
    {
        get;
        set;
    }

    [JsonPropertyName("MinimumLevelToApplyBuff")]
    public double? MinimumLevelToApplyBuff
    {
        get;
        set;
    }

    [JsonPropertyName("RepairStrategies")]
    public RepairStrategies? RepairStrategies
    {
        get;
        set;
    }

    [JsonPropertyName("armorClassDivisor")]
    public double? ArmorClassDivisor
    {
        get;
        set;
    }

    [JsonPropertyName("durabilityPointCostArmor")]
    public double? DurabilityPointCostArmor
    {
        get;
        set;
    }

    [JsonPropertyName("durabilityPointCostGuns")]
    public double? DurabilityPointCostGuns
    {
        get;
        set;
    }
}
