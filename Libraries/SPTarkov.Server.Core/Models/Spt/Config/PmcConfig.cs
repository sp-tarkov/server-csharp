using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record PmcConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public override string Kind
    {
        get;
        set;
    } = "spt-pmc";

    /// <summary>
    ///     What game version should the PMC have
    /// </summary>
    [JsonPropertyName("gameVersionWeight")]
    public required Dictionary<string, double> GameVersionWeight
    {
        get;
        set;
    }

    /// <summary>
    ///     What account type should the PMC have
    /// </summary>
    [JsonPropertyName("accountTypeWeight")]
    public required Dictionary<MemberCategory, double> AccountTypeWeight
    {
        get;
        set;
    }

    /// <summary>
    ///     Global whitelist/blacklist of vest loot for PMCs
    /// </summary>
    [JsonPropertyName("vestLoot")]
    public required SlotLootSettings VestLoot
    {
        get;
        set;
    }

    /// <summary>
    ///     Global whitelist/blacklist of pocket loot for PMCs
    /// </summary>
    [JsonPropertyName("pocketLoot")]
    public required SlotLootSettings PocketLoot
    {
        get;
        set;
    }

    /// <summary>
    ///     Global whitelist/blacklist of backpack loot for PMCs
    /// </summary>
    [JsonPropertyName("backpackLoot")]
    public required SlotLootSettings BackpackLoot
    {
        get;
        set;
    }

    [JsonPropertyName("globalLootBlacklist")]
    public required List<string> GlobalLootBlacklist
    {
        get;
        set;
    }

    /// <summary>
    ///     Use difficulty defined in config/bot.json/difficulty instead of chosen difficulty dropdown value
    /// </summary>
    [JsonPropertyName("useDifficultyOverride")]
    public required bool UseDifficultyOverride
    {
        get;
        set;
    }

    /// <summary>
    ///     Difficulty override e.g. "AsOnline/Hard"
    /// </summary>
    [JsonPropertyName("difficulty")]
    public required string Difficulty
    {
        get;
        set;
    }

    /// <summary>
    ///     Chance out of 100 to have a complete gun in backpack
    /// </summary>
    [JsonPropertyName("looseWeaponInBackpackChancePercent")]
    public required double LooseWeaponInBackpackChancePercent
    {
        get;
        set;
    }

    /// <summary>
    ///     Chance out of 100 to have an enhancement applied to PMC weapon
    /// </summary>
    [JsonPropertyName("weaponHasEnhancementChancePercent")]
    public required double WeaponHasEnhancementChancePercent
    {
        get;
        set;
    }

    /// <summary>
    ///     MinMax count of weapons to have in backpack
    /// </summary>
    [JsonPropertyName("looseWeaponInBackpackLootMinMax")]
    public required MinMax<int> LooseWeaponInBackpackLootMinMax
    {
        get;
        set;
    }

    [JsonPropertyName("_isUsec")]
    public string? IsUsecDescription
    {
        get;
        set;
    }

    /// <summary>
    ///     Percentage chance PMC will be USEC
    /// </summary>
    [JsonPropertyName("isUsec")]
    public required double IsUsec
    {
        get;
        set;
    }

    /// <summary>
    ///     WildSpawnType enum value USEC PMCs use
    /// </summary>
    [JsonPropertyName("usecType")]
    public required string UsecType
    {
        get;
        set;
    }

    /// <summary>
    ///     WildSpawnType enum value BEAR PMCs use
    /// </summary>
    [JsonPropertyName("bearType")]
    public required string BearType
    {
        get;
        set;
    }

    [JsonPropertyName("_pmcType")]
    public string? PmcTypeDescription
    {
        get;
        set;
    }

    /// <summary>
    ///     What 'brain' does a PMC use, keyed by map and side (USEC/BEAR) key: map location, value: type for usec/bear
    /// </summary>
    [JsonPropertyName("pmcType")]
    public required Dictionary<string, Dictionary<string, Dictionary<string, double>>> PmcType
    {
        get;
        set;
    }

    [JsonPropertyName("maxBackpackLootTotalRub")]
    public required List<MinMaxLootValue> MaxBackpackLootTotalRub
    {
        get;
        set;
    }

    [JsonPropertyName("maxPocketLootTotalRub")]
    public required int MaxPocketLootTotalRub
    {
        get;
        set;
    }

    [JsonPropertyName("maxVestLootTotalRub")]
    public required int MaxVestLootTotalRub
    {
        get;
        set;
    }

    /// <summary>
    ///     How many levels above player level can a PMC be
    /// </summary>
    [JsonPropertyName("botRelativeLevelDeltaMax")]
    public required int BotRelativeLevelDeltaMax
    {
        get;
        set;
    }

    /// <summary>
    ///     How many levels below player level can a PMC be
    /// </summary>
    [JsonPropertyName("botRelativeLevelDeltaMin")]
    public required int BotRelativeLevelDeltaMin
    {
        get;
        set;
    }

    /// <summary>
    ///     Force a number of healing items into PMCs secure container to ensure they can heal
    /// </summary>
    [JsonPropertyName("forceHealingItemsIntoSecure")]
    public required bool ForceHealingItemsIntoSecure
    {
        get;
        set;
    }

    [JsonPropertyName("hostilitySettings")]
    public required Dictionary<string, HostilitySettings> HostilitySettings
    {
        get;
        set;
    }

    [JsonPropertyName("allPMCsHavePlayerNameWithRandomPrefixChance")]
    public required double AllPMCsHavePlayerNameWithRandomPrefixChance
    {
        get;
        set;
    }

    [JsonPropertyName("locationSpecificPmcLevelOverride")]
    public required Dictionary<string, MinMax<int>> LocationSpecificPmcLevelOverride
    {
        get;
        set;
    }

    /// <summary>
    ///     Should secure container loot from usec.json/bear.json be added to pmc bots secure
    /// </summary>
    [JsonPropertyName("addSecureContainerLootFromBotConfig")]
    public required bool AddSecureContainerLootFromBotConfig
    {
        get;
        set;
    }

    [JsonPropertyName("addPrefixToSameNamePMCAsPlayerChance")]
    public required int? AddPrefixToSameNamePMCAsPlayerChance
    {
        get;
        set;
    }

    [JsonPropertyName("lootItemLimitsRub")]
    public required List<MinMaxLootItemValue>? LootItemLimitsRub
    {
        get;
        set;
    }

    [JsonPropertyName("removeExistingPmcWaves")]
    public required bool? RemoveExistingPmcWaves
    {
        get;
        set;
    }

    [JsonPropertyName("customPmcWaves")]
    public required Dictionary<string, List<BossLocationSpawn>> CustomPmcWaves
    {
        get;
        set;
    }
}

public record HostilitySettings
{
    /// <summary>
    ///     Bot roles that are 100% an enemy
    /// </summary>
    [JsonPropertyName("additionalEnemyTypes")]
    public List<string>? AdditionalEnemyTypes
    {
        get;
        set;
    }

    /// <summary>
    ///     Objects that determine the % chance another bot type is an enemy
    /// </summary>
    [JsonPropertyName("chancedEnemies")]
    public List<ChancedEnemy>? ChancedEnemies
    {
        get;
        set;
    }

    [JsonPropertyName("bearEnemyChance")]
    public double? BearEnemyChance
    {
        get;
        set;
    }

    [JsonPropertyName("usecEnemyChance")]
    public double? UsecEnemyChance
    {
        get;
        set;
    }

    [JsonPropertyName("savageEnemyChance")]
    public double? SavageEnemyChance
    {
        get;
        set;
    }

    /// <summary>
    ///     Bot roles that are 100% a friendly
    /// </summary>
    [JsonPropertyName("additionalFriendlyTypes")]
    public List<string>? AdditionalFriendlyTypes
    {
        get;
        set;
    }

    [JsonPropertyName("savagePlayerBehaviour")]
    public string? SavagePlayerBehaviour
    {
        get;
        set;
    }
}

public record SlotLootSettings
{
    /// <summary>
    ///     Item Type whitelist
    /// </summary>
    [JsonPropertyName("whitelist")]
    public HashSet<string> Whitelist
    {
        get;
        set;
    } = [];

    /// <summary>
    ///     Item tpl blacklist
    /// </summary>
    [JsonPropertyName("blacklist")]
    public HashSet<string> Blacklist
    {
        get;
        set;
    } = [];
}

public record MinMaxLootValue : MinMax<int>
{
    [JsonPropertyName("value")]
    public double Value
    {
        get;
        set;
    }
}

public record MinMaxLootItemValue : MinMax<double>
{
    [JsonPropertyName("backpack")]
    public required MinMax<double> Backpack
    {
        get;
        set;
    }

    [JsonPropertyName("pocket")]
    public required MinMax<double> Pocket
    {
        get;
        set;
    }

    [JsonPropertyName("vest")]
    public required MinMax<double> Vest
    {
        get;
        set;
    }
}
