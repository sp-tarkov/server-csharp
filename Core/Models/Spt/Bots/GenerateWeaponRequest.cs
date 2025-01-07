using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Spt.Bots;

public class GenerateWeaponRequest
{
    /** Weapon to add mods to / result that is returned */
    [JsonPropertyName("weapon")]
    public List<Item> Weapon { get; set; }

    /** Pool of compatible mods to attach to weapon */
    [JsonPropertyName("modPool")]
    public GlobalMods ModPool { get; set; }

    /** ParentId of weapon */
    [JsonPropertyName("weaponId")]
    public string WeaponId { get; set; }

    /** Weapon which mods will be generated on */
    [JsonPropertyName("parentTemplate")]
    public TemplateItem ParentTemplate { get; set; }

    /** Chance values mod will be added */
    [JsonPropertyName("modSpawnChances")]
    public ModsChances ModSpawnChances { get; set; }

    /** Ammo tpl to use when generating magazines/cartridges */
    [JsonPropertyName("ammoTpl")]
    public string AmmoTpl { get; set; }

    /** Bot-specific properties */
    [JsonPropertyName("botData")]
    public BotData BotData { get; set; }

    /** limits placed on certain mod types per gun */
    [JsonPropertyName("modLimits")]
    public BotModLimits ModLimits { get; set; }

    /** Info related to the weapon being generated */
    [JsonPropertyName("weaponStats")]
    public WeaponStats WeaponStats { get; set; }

    /** Array of item tpls the weapon does not support */
    [JsonPropertyName("conflictingItemTpls")]
    public HashSet<string> ConflictingItemTpls { get; set; }
}

public class BotData
{
    /** Role of bot weapon is generated for */
    [JsonPropertyName("role")]
    public string Role { get; set; }

    /** Level of the bot weapon is being generated for */
    [JsonPropertyName("level")]
    public int Level { get; set; }

    /** role of bot when accessing bot.json equipment config settings */
    [JsonPropertyName("equipmentRole")]
    public string EquipmentRole { get; set; }
}

public class WeaponStats
{
    [JsonPropertyName("hasOptic")]
    public bool? HasOptic { get; set; }

    [JsonPropertyName("hasFrontIronSight")]
    public bool? HasFrontIronSight { get; set; }

    [JsonPropertyName("hasRearIronSight")]
    public bool? HasRearIronSight { get; set; }
}

public class BotModLimits
{
    [JsonPropertyName("scope")]
    public ItemCount Scope { get; set; }
    
    [JsonPropertyName("scopeMax")]
    public int ScopeMax { get; set; }
    
    [JsonPropertyName("scopeBaseTypes")]
    public string[] ScopeBaseTypes { get; set; }
    
    [JsonPropertyName("flashlightLaser")]
    public ItemCount FlashlightLaser { get; set; }
    
    [JsonPropertyName("flashlightLaserMax")]
    public int FlashlightLaserMax { get; set; }
    
    [JsonPropertyName("flashlightLaserBaseTypes")]
    public string[] FlashlightLaserBaseTypes { get; set; }
}

public class ItemCount
{
    [JsonPropertyName("count")]
    public int Count { get; set; }
}