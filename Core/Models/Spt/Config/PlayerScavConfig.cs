using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Spt.Config;

public class PlayerScavConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-playerscav";

    [JsonPropertyName("karmaLevel")]
    public Dictionary<string, KarmaLevel> KarmaLevel { get; set; }
}

public class KarmaLevel
{
    [JsonPropertyName("botTypeForLoot")]
    public string BotTypeForLoot { get; set; }

    [JsonPropertyName("modifiers")]
    public Modifiers Modifiers { get; set; }

    [JsonPropertyName("itemLimits")]
    public ItemLimits ItemLimits { get; set; }

    [JsonPropertyName("equipmentBlacklist")]
    public Dictionary<string, string[]> EquipmentBlacklist { get; set; }

    [JsonPropertyName("labsAccessCardChancePercent")]
    public double? LabsAccessCardChancePercent { get; set; }

    [JsonPropertyName("lootItemsToAddChancePercent")]
    public Dictionary<string, double> LootItemsToAddChancePercent { get; set; }
}

public class Modifiers
{
    [JsonPropertyName("equipment")]
    public Dictionary<string, double> Equipment { get; set; }

    [JsonPropertyName("mod")]
    public Dictionary<string, double> Mod { get; set; }
}

public class ItemLimits
{
    [JsonPropertyName("healing")]
    public GenerationData Healing { get; set; }

    [JsonPropertyName("drugs")]
    public GenerationData Drugs { get; set; }

    [JsonPropertyName("stims")]
    public GenerationData Stims { get; set; }

    [JsonPropertyName("looseLoot")]
    public GenerationData LooseLoot { get; set; }

    [JsonPropertyName("magazines")]
    public GenerationData Magazines { get; set; }

    [JsonPropertyName("grenades")]
    public GenerationData Grenades { get; set; }
}
