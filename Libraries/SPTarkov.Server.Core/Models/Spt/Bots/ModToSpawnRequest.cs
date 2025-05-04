using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;

namespace SPTarkov.Server.Core.Models.Spt.Bots;

public record ModToSpawnRequest
{
    /// <summary>
    ///     Slot mod will fit into
    /// </summary>
    [JsonPropertyName("modSlot")]
    public string? ModSlot { get; set; }

    /// <summary>
    ///     Will generate a randomised mod pool if true
    /// </summary>
    [JsonPropertyName("isRandomisableSlot")]
    public bool? IsRandomisableSlot { get; set; }

    [JsonPropertyName("randomisationSettings")]
    public RandomisationDetails? RandomisationSettings { get; set; }

    /// <summary>
    ///     Parent slot the item will be a part of
    /// </summary>
    [JsonPropertyName("botWeaponSightWhitelist")]
    public Dictionary<string, List<string>>? BotWeaponSightWhitelist { get; set; }

    /// <summary>
    ///     Blacklist to prevent mods from being picked
    /// </summary>
    [JsonPropertyName("botEquipBlacklist")]
    public EquipmentFilterDetails? BotEquipBlacklist { get; set; }

    /// <summary>
    ///     Pool of items to pick from
    /// </summary>
    [JsonPropertyName("itemModPool")]
    public Dictionary<string, HashSet<string>>? ItemModPool { get; set; }

    /// <summary>
    ///     List with only weapon tpl in it, ready for mods to be added
    /// </summary>
    [JsonPropertyName("weapon")]
    public List<Item>? Weapon { get; set; }

    /// <summary>
    ///     Ammo tpl to use if slot requires a cartridge to be added (e.g. mod_magazine)
    /// </summary>
    [JsonPropertyName("ammoTpl")]
    public string? AmmoTpl { get; set; }

    /// <summary>
    ///     Parent item the mod will go into
    /// </summary>
    [JsonPropertyName("parentTemplate")]
    public TemplateItem? ParentTemplate { get; set; }

    /// <summary>
    ///     Should mod be spawned/skipped/use default
    /// </summary>
    [JsonPropertyName("modSpawnResult")]
    public ModSpawn? ModSpawnResult { get; set; }

    /// <summary>
    ///     Weapon stats for weapon being generated
    /// </summary>
    [JsonPropertyName("weaponStats")]
    public WeaponStats? WeaponStats { get; set; }

    /// <summary>
    ///     List of item tpls the weapon does not support
    /// </summary>
    [JsonPropertyName("conflictingItemTpls")]
    public HashSet<string>? ConflictingItemTpls { get; set; }

    [JsonPropertyName("botData")]
    public BotData? BotData { get; set; }
}
