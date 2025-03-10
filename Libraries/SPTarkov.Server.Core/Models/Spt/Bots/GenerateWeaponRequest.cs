using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Spt.Bots;

public record GenerateWeaponRequest
{
    /// <summary>
    /// Weapon to add mods to / result that is returned
    /// </summary>
    [JsonPropertyName("weapon")]
    public List<Item>? Weapon
    {
        get;
        set;
    }

    /// <summary>
    /// Pool of compatible mods to attach to weapon
    /// </summary>
    [JsonPropertyName("modPool")]
    public GlobalMods? ModPool
    {
        get;
        set;
    }

    /// <summary>
    /// ParentId of weapon
    /// </summary>
    [JsonPropertyName("weaponId")]
    public MongoId? WeaponId
    {
        get;
        set;
    }

    /// <summary>
    /// Weapon which mods will be generated on
    /// </summary>
    [JsonPropertyName("parentTemplate")]
    public TemplateItem? ParentTemplate
    {
        get;
        set;
    }

    /// <summary>
    /// Chance values mod will be added
    /// </summary>
    [JsonPropertyName("modSpawnChances")]
    public Dictionary<string, double>? ModSpawnChances
    {
        get;
        set;
    }

    /// <summary>
    /// Ammo tpl to use when generating magazines/cartridges
    /// </summary>
    [JsonPropertyName("ammoTpl")]
    public string? AmmoTpl
    {
        get;
        set;
    }

    /// <summary>
    /// Bot-specific properties
    /// </summary>
    [JsonPropertyName("botData")]
    public BotData? BotData
    {
        get;
        set;
    }

    /// <summary>
    /// limits placed on certain mod types per gun
    /// </summary>
    [JsonPropertyName("modLimits")]
    public BotModLimits? ModLimits
    {
        get;
        set;
    }

    /// <summary>
    /// Info related to the weapon being generated
    /// </summary>
    [JsonPropertyName("weaponStats")]
    public WeaponStats? WeaponStats
    {
        get;
        set;
    }

    /// <summary>
    /// Array of item tpls the weapon does not support
    /// </summary>
    [JsonPropertyName("conflictingItemTpls")]
    public HashSet<string>? ConflictingItemTpls
    {
        get;
        set;
    }
}

public record BotData
{
    /// <summary>
    /// Role of bot weapon is generated for
    /// </summary>
    [JsonPropertyName("role")]
    public string? Role
    {
        get;
        set;
    }

    /// <summary>
    /// Level of the bot weapon is being generated for
    /// </summary>
    [JsonPropertyName("level")]
    public int? Level
    {
        get;
        set;
    }

    /// <summary>
    /// role of bot when accessing bot.json equipment config settings
    /// </summary>
    [JsonPropertyName("equipmentRole")]
    public string? EquipmentRole
    {
        get;
        set;
    }
}

public record WeaponStats
{
    [JsonPropertyName("hasOptic")]
    public bool? HasOptic
    {
        get;
        set;
    }

    [JsonPropertyName("hasFrontIronSight")]
    public bool? HasFrontIronSight
    {
        get;
        set;
    }

    [JsonPropertyName("hasRearIronSight")]
    public bool? HasRearIronSight
    {
        get;
        set;
    }
}

public record BotModLimits
{
    [JsonPropertyName("scope")]
    public ItemCount? Scope
    {
        get;
        set;
    }

    [JsonPropertyName("scopeMax")]
    public int? ScopeMax
    {
        get;
        set;
    }

    [JsonPropertyName("scopeBaseTypes")]
    public List<string>? ScopeBaseTypes
    {
        get;
        set;
    }

    [JsonPropertyName("flashlightLaser")]
    public ItemCount? FlashlightLaser
    {
        get;
        set;
    }

    [JsonPropertyName("flashlightLaserMax")]
    public int? FlashlightLaserMax
    {
        get;
        set;
    }

    [JsonPropertyName("flashlightLaserBaseTypes")]
    public List<string>? FlashlightLaserBaseTypes
    {
        get;
        set;
    }
}

public record ItemCount
{
    [JsonPropertyName("count")]
    public int? Count
    {
        get;
        set;
    }
}
