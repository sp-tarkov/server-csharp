using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record PlayerScavConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind
    {
        get;
        set;
    } = "spt-playerscav";

    [JsonPropertyName("karmaLevel")]
    public Dictionary<string, KarmaLevel> KarmaLevel
    {
        get;
        set;
    }
}

public record KarmaLevel
{
    [JsonPropertyName("botTypeForLoot")]
    public string BotTypeForLoot
    {
        get;
        set;
    }

    [JsonPropertyName("modifiers")]
    public Modifiers Modifiers
    {
        get;
        set;
    }

    [JsonPropertyName("itemLimits")]
    public Dictionary<string, GenerationData> ItemLimits
    {
        get;
        set;
    }

    [JsonPropertyName("equipmentBlacklist")]
    public Dictionary<EquipmentSlots, List<string>> EquipmentBlacklist
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
    public Dictionary<string, double> LootItemsToAddChancePercent
    {
        get;
        set;
    }
}

public record Modifiers
{
    [JsonPropertyName("equipment")]
    public Dictionary<string, double> Equipment
    {
        get;
        set;
    }

    [JsonPropertyName("mod")]
    public Dictionary<string, double> Mod
    {
        get;
        set;
    }
}

public record ItemLimits
{
    [JsonPropertyName("healing")]
    public GenerationData Healing
    {
        get;
        set;
    }

    [JsonPropertyName("drugs")]
    public GenerationData Drugs
    {
        get;
        set;
    }

    [JsonPropertyName("stims")]
    public GenerationData Stims
    {
        get;
        set;
    }

    [JsonPropertyName("looseLoot")]
    public GenerationData LooseLoot
    {
        get;
        set;
    }

    [JsonPropertyName("magazines")]
    public GenerationData Magazines
    {
        get;
        set;
    }

    [JsonPropertyName("grenades")]
    public GenerationData Grenades
    {
        get;
        set;
    }
}
