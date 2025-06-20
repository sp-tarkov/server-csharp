using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils.Collections;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record QuestConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public override string Kind { get; set; } = "spt-quest";

    /// <summary>
    ///     Hours to get/redeem items from quest mail keyed by profile type
    /// </summary>
    [JsonPropertyName("mailRedeemTimeHours")]
    public required Dictionary<string, double> MailRedeemTimeHours { get; set; }

    /// <summary>
    ///     Collection of quests by id only available to usec
    /// </summary>
    [JsonPropertyName("usecOnlyQuests")]
    public required HashSet<string> UsecOnlyQuests { get; set; }

    /// <summary>
    ///     Collection of quests by id only available to bears
    /// </summary>
    [JsonPropertyName("bearOnlyQuests")]
    public required HashSet<string> BearOnlyQuests { get; set; }

    /// <summary>
    ///     Quests that the keyed game version do not see/access
    /// </summary>
    [JsonPropertyName("profileBlacklist")]
    public required Dictionary<string, HashSet<string>> ProfileBlacklist { get; set; }

    /// <summary>
    ///     key=questid, gameversions that can see/access quest
    /// </summary>
    [JsonPropertyName("profileWhitelist")]
    public required Dictionary<string, HashSet<string>> ProfileWhitelist { get; set; }

    /// <summary>
    ///     Holds repeatable quest template ids for pmc's and scav's
    /// </summary>
    [JsonPropertyName("repeatableQuestTemplateIds")]
    public required RepeatableQuestTemplates RepeatableQuestTemplates { get; set; }

    /// <summary>
    ///     Show non-seasonal quests be shown to players
    /// </summary>
    [JsonPropertyName("showNonSeasonalEventQuests")]
    public required bool ShowNonSeasonalEventQuests { get; set; }

    /// <summary>
    ///     Collection of event quest data keyed by quest id.
    /// </summary>
    [JsonPropertyName("eventQuests")]
    public required Dictionary<string, EventQuestData> EventQuests { get; set; }

    /// <summary>
    ///     List of repeatable quest configs for; daily, weekly, and daily scav.
    /// </summary>
    [JsonPropertyName("repeatableQuests")]
    public required List<RepeatableQuestConfig> RepeatableQuests { get; set; }

    /// <summary>
    ///     Maps internal map names to their mongoId: Key - internal :: val - Mongoid
    /// </summary>
    [JsonPropertyName("locationIdMap")]
    public required Dictionary<string, string> LocationIdMap { get; set; }
}

public record RepeatableQuestTemplates
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    /// <summary>
    ///     Pmc repeatable quest template ids keyed by type of quest
    /// Keys: elimination, completion, exploration
    /// </summary>
    [JsonPropertyName("pmc")]
    public required Dictionary<string, string> Pmc { get; set; }

    /// <summary>
    ///     Scav repeatable quest template ids keyed by type of quest
    /// Keys: elimination, completion, exploration, pickup
    /// </summary>
    [JsonPropertyName("scav")]
    public required Dictionary<string, string> Scav { get; set; }
}

public record EventQuestData
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    /// <summary>
    ///     Name of the event quest
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    ///     Season to which this quest belongs
    /// </summary>
    [JsonPropertyName("season")]
    public required SeasonalEventType Season { get; set; }

    /// <summary>
    ///     Start timestamp
    /// </summary>
    [JsonPropertyName("startTimestamp")]
    public required long StartTimestamp { get; set; }

    /// <summary>
    ///     End timestamp
    /// </summary>
    [JsonPropertyName("endTimestamp")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public required long EndTimestamp { get; set; }

    /// <summary>
    ///     Is this quest part of a yearly event, ex: Christmas
    /// </summary>
    [JsonPropertyName("yearly")]
    public required bool Yearly { get; set; }
}

public record RepeatableQuestConfig
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    /// <summary>
    ///     Id for type of repeatable quest
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    ///     Human-readable name for repeatable quest type
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    ///     Side this config belongs to. Note: Random not implemented, do not use!
    /// </summary>
    [JsonPropertyName("side")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required PlayerGroup Side { get; set; }

    /// <summary>
    ///     Types of tasks this config can generate; ex: Elimination
    /// </summary>
    [JsonPropertyName("types")]
    public required List<string> Types { get; set; }

    /// <summary>
    ///     How long does the task stay active for after accepting it
    /// </summary>
    [JsonPropertyName("resetTime")]
    public required long ResetTime { get; set; }

    /// <summary>
    ///     How many quests should we provide per ResetTime
    /// </summary>
    [JsonPropertyName("numQuests")]
    public required int NumQuests { get; set; }

    /// <summary>
    ///     Min player level required to receive a quest from this config
    /// </summary>
    [JsonPropertyName("minPlayerLevel")]
    public required int MinPlayerLevel { get; set; }

    /// <summary>
    ///     Reward scaling config
    /// </summary>
    [JsonPropertyName("rewardScaling")]
    public required RewardScaling RewardScaling { get; set; }

    /// <summary>
    ///     Location map
    /// </summary>
    [JsonPropertyName("locations")]
    public required Dictionary<ELocationName, List<string>> Locations { get; set; }

    /// <summary>
    ///     Traders that are allowed to generate tasks from this config.
    /// Includes quest types, reward whitelist, and whether rewards can be weapons.
    /// </summary>
    [JsonPropertyName("traderWhitelist")]
    public required List<TraderWhitelist> TraderWhitelist { get; set; }

    /// <summary>
    ///     Quest config, holds information on how a task should be generated
    /// </summary>
    [JsonPropertyName("questConfig")]
    public RepeatableQuestTypesConfig QuestConfig { get; set; }

    /// <summary>
    ///     Item base types to block when generating rewards
    /// </summary>
    [JsonPropertyName("rewardBaseTypeBlacklist")]
    public required HashSet<string> RewardBaseTypeBlacklist { get; set; }

    /// <summary>
    ///     Item tplIds to ignore when generating rewards
    /// </summary>
    [JsonPropertyName("rewardBlacklist")]
    public required HashSet<string> RewardBlacklist { get; set; }

    /// <summary>
    ///     Minimum stack size that an ammo reward should be generated with
    /// </summary>
    [JsonPropertyName("rewardAmmoStackMinSize")]
    public required int RewardAmmoStackMinSize { get; set; }

    /// <summary>
    ///     How many free task changes are available from this config
    /// </summary>
    [JsonPropertyName("freeChangesAvailable")]
    public required int FreeChangesAvailable { get; set; }

    /// <summary>
    ///     How many free task changes remain from this config
    /// </summary>
    [JsonPropertyName("freeChanges")]
    public required int FreeChanges { get; set; }

    /// <summary>
    ///     Should the task replacement category be the same as the one its replacing
    /// </summary>
    [JsonPropertyName("keepDailyQuestTypeOnReplacement")]
    public required bool KeepDailyQuestTypeOnReplacement { get; set; }

    /// <summary>
    ///     Reputation standing price for replacing a repeatable
    /// </summary>
    [JsonPropertyName("standingChangeCost")]
    public required IList<double> StandingChangeCost { get; set; }
}

public record RewardScaling
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("levels")]
    public List<double>? Levels { get; set; }

    [JsonPropertyName("experience")]
    public List<double>? Experience { get; set; }

    [JsonPropertyName("roubles")]
    public List<double>? Roubles { get; set; }

    [JsonPropertyName("gpCoins")]
    public List<double>? GpCoins { get; set; }

    [JsonPropertyName("items")]
    public List<double>? Items { get; set; }

    [JsonPropertyName("reputation")]
    public List<double>? Reputation { get; set; }

    [JsonPropertyName("rewardSpread")]
    public double? RewardSpread { get; set; }

    [JsonPropertyName("skillRewardChance")]
    public List<double>? SkillRewardChance { get; set; }

    [JsonPropertyName("skillPointReward")]
    public List<double>? SkillPointReward { get; set; }
}

public record TraderWhitelist
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("traderId")]
    public string? TraderId { get; set; }

    [JsonPropertyName("questTypes")]
    public List<string>? QuestTypes { get; set; }

    [JsonPropertyName("rewardBaseWhitelist")]
    public List<string>? RewardBaseWhitelist { get; set; }

    [JsonPropertyName("rewardCanBeWeapon")]
    public bool? RewardCanBeWeapon { get; set; }

    [JsonPropertyName("weaponRewardChancePercent")]
    public double? WeaponRewardChancePercent { get; set; }
}

public record RepeatableQuestTypesConfig
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("Exploration")]
    public Exploration? Exploration { get; set; }

    [JsonPropertyName("Completion")]
    public Completion? Completion { get; set; }

    [JsonPropertyName("Pickup")]
    public Pickup? Pickup { get; set; }

    [JsonPropertyName("Elimination")]
    public List<EliminationConfig>? Elimination { get; set; }
}

public record Exploration : BaseQuestConfig
{
    [JsonPropertyName("maxExtracts")]
    public int? MaximumExtracts { get; set; }

    [JsonPropertyName("maxExtractsWithSpecificExit")]
    public int? MaximumExtractsWithSpecificExit { get; set; }

    [JsonPropertyName("specificExits")]
    public SpecificExits? SpecificExits { get; set; }
}

public record SpecificExits
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("probability")]
    public double? Probability { get; set; }

    [JsonPropertyName("passageRequirementWhitelist")]
    public List<string>? PassageRequirementWhitelist { get; set; }
}

public record Completion : BaseQuestConfig
{
    [JsonPropertyName("minRequestedAmount")]
    public int? MinimumRequestedAmount { get; set; }

    [JsonPropertyName("maxRequestedAmount")]
    public int? MaximumRequestedAmount { get; set; }

    [JsonPropertyName("uniqueItemCount")]
    public int? UniqueItemCount { get; set; }

    [JsonPropertyName("minRequestedBulletAmount")]
    public int? MinimumRequestedBulletAmount { get; set; }

    [JsonPropertyName("maxRequestedBulletAmount")]
    public int? MaximumRequestedBulletAmount { get; set; }

    [JsonPropertyName("useWhitelist")]
    public bool? UseWhitelist { get; set; }

    [JsonPropertyName("useBlacklist")]
    public bool? UseBlacklist { get; set; }

    /// <summary>
    ///     Should supplied items be required FiR
    /// </summary>
    [JsonPropertyName("requiredItemsAreFiR")]
    public bool? RequiredItemsAreFiR { get; set; }

    /// <summary>
    ///     Should supplied items be required FiR
    /// </summary>
    [JsonPropertyName("requiredItemMinDurabilityMinMax")]
    public MinMax<double>? RequiredItemMinDurabilityMinMax { get; set; }

    /// <summary>
    ///     Blacklisted item types to not collect
    /// </summary>
    [JsonPropertyName("requiredItemTypeBlacklist")]
    public HashSet<string>? RequiredItemTypeBlacklist { get; set; }
}

public record Pickup : BaseQuestConfig
{
    [JsonPropertyName("ItemTypeToFetchWithMaxCount")]
    public List<PickupTypeWithMaxCount>? ItemTypeToFetchWithMaxCount { get; set; }

    public List<string>? ItemTypesToFetch { get; set; }

    [JsonPropertyName("maxItemFetchCount")]
    public int? MaxItemFetchCount { get; set; }
}

public record PickupTypeWithMaxCount
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("itemType")]
    public string? ItemType { get; set; }

    [JsonPropertyName("maxPickupCount")]
    public int? MaximumPickupCount { get; set; }

    [JsonPropertyName("minPickupCount")]
    public int? MinimumPickupCount { get; set; }
}

public record EliminationConfig : BaseQuestConfig
{
    [JsonPropertyName("levelRange")]
    public MinMax<int>? LevelRange { get; set; }

    [JsonPropertyName("targets")]
    public List<ProbabilityObject<string, BossInfo>>? Targets { get; set; }

    [JsonPropertyName("bodyPartProb")]
    public double? BodyPartProbability { get; set; }

    [JsonPropertyName("bodyParts")]
    public List<ProbabilityObject<string, List<string>>>? BodyParts { get; set; }

    [JsonPropertyName("specificLocationProb")]
    public double? SpecificLocationProbability { get; set; }

    [JsonPropertyName("distLocationBlacklist")]
    public List<string>? DistLocationBlacklist { get; set; }

    [JsonPropertyName("distProb")]
    public double? DistanceProbability { get; set; }

    [JsonPropertyName("maxDist")]
    public double? MaxDistance { get; set; }

    [JsonPropertyName("minDist")]
    public double? MinDistance { get; set; }

    [JsonPropertyName("maxKills")]
    public int? MaxKills { get; set; }

    [JsonPropertyName("minKills")]
    public int? MinKills { get; set; }

    [JsonPropertyName("minBossKills")]
    public int? MinBossKills { get; set; }

    [JsonPropertyName("maxBossKills")]
    public int? MaxBossKills { get; set; }

    [JsonPropertyName("minPmcKills")]
    public int? MinPmcKills { get; set; }

    [JsonPropertyName("maxPmcKills")]
    public int? MaxPmcKills { get; set; }

    [JsonPropertyName("weaponCategoryRequirementProb")]
    public double? WeaponCategoryRequirementProbability { get; set; }

    [JsonPropertyName("weaponCategoryRequirements")]
    public List<ProbabilityObject<string, List<string>>>? WeaponCategoryRequirements { get; set; }

    [JsonPropertyName("weaponRequirementProb")]
    public double? WeaponRequirementProbability { get; set; }

    [JsonPropertyName("weaponRequirements")]
    public List<ProbabilityObject<string, List<string>>>? WeaponRequirements { get; set; }
}

public record BaseQuestConfig
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("possibleSkillRewards")]
    public List<string>? PossibleSkillRewards { get; set; }
}

public record BossInfo
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("isBoss")]
    public bool? IsBoss { get; set; }

    [JsonPropertyName("isPmc")]
    public bool? IsPmc { get; set; }
}
