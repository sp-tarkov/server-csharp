using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record PlayerScavConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public override string Kind
    {
        get;
        set;
    } = "spt-playerscav";

    [JsonPropertyName("karmaLevel")]
    public required Dictionary<string, KarmaLevel> KarmaLevel
    {
        get;
        set;
    }
}

public record KarmaLevel
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("botTypeForLoot")]
    public required string BotTypeForLoot
    {
        get;
        set;
    }

    [JsonPropertyName("modifiers")]
    public required Modifiers Modifiers
    {
        get;
        set;
    }

    [JsonPropertyName("itemLimits")]
    public required Dictionary<string, GenerationData> ItemLimits
    {
        get;
        set;
    }

    [JsonPropertyName("equipmentBlacklist")]
    public required Dictionary<EquipmentSlots, List<string>> EquipmentBlacklist
    {
        get;
        set;
    }

    [JsonPropertyName("labsAccessCardChancePercent")]
    public double? LabsAccessCardChancePercent
    {
        get;
        set;
    }

    [JsonPropertyName("lootItemsToAddChancePercent")]
    public required Dictionary<string, double> LootItemsToAddChancePercent
    {
        get;
        set;
    }
}

public record Modifiers
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("equipment")]
    public required Dictionary<string, double> Equipment
    {
        get;
        set;
    }

    [JsonPropertyName("mod")]
    public required Dictionary<string, double> Mod
    {
        get;
        set;
    }
}
