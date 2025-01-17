using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Config;

namespace Core.Models.Spt.Bots;

public record GenerateEquipmentProperties
{
    /// <summary>
    /// Root Slot being generated
    /// </summary>
    [JsonPropertyName("rootEquipmentSlot")]
    public EquipmentSlots RootEquipmentSlot { get; set; }

    /// <summary>
    /// Equipment pool for root slot being generated
    /// </summary>
    [JsonPropertyName("rootEquipmentPool")]
    public Dictionary<string, double>? RootEquipmentPool { get; set; }

    [JsonPropertyName("modPool")]
    public GlobalMods? ModPool { get; set; }

    /// <summary>
    /// Dictionary of mod items and their chance to spawn for this bot type
    /// </summary>
    [JsonPropertyName("spawnChances")]
    public Chances? SpawnChances { get; set; }

    /// <summary>
    /// Bot-specific properties
    /// </summary>
    [JsonPropertyName("botData")]
    public BotData? BotData { get; set; }

    [JsonPropertyName("inventory")]
    public BotBaseInventory? Inventory { get; set; }

    [JsonPropertyName("botEquipmentConfig")]
    public EquipmentFilters? BotEquipmentConfig { get; set; }

    /// <summary>
    /// Settings from bot.json to adjust how item is generated
    /// </summary>
    [JsonPropertyName("randomisationDetails")]
    public RandomisationDetails? RandomisationDetails { get; set; }

    /// <summary>
    /// OPTIONAL - Do not generate mods for tpls in this array
    /// </summary>
    [JsonPropertyName("generateModsBlacklist")]
    public List<string>? GenerateModsBlacklist { get; set; }

    [JsonPropertyName("generatingPlayerLevel")]
    public double? GeneratingPlayerLevel { get; set; }
}
