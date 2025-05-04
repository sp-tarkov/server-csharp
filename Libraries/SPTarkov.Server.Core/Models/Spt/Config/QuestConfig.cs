using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils.Collections;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record QuestConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-quest";

    /// <summary>
    ///     Hours to get/redeem items from quest mail keyed by profile type
    /// </summary>
    [JsonPropertyName("mailRedeemTimeHours")]
    public Dictionary<string, double?>? MailRedeemTimeHours { get; set; }

    [JsonPropertyName("questTemplateIds")]
    public PlayerTypeQuestIds? QuestTemplateIds { get; set; }

    /// <summary>
    ///     Show non-seasonal quests be shown to player
    /// </summary>
    [JsonPropertyName("showNonSeasonalEventQuests")]
    public bool? ShowNonSeasonalEventQuests { get; set; }

    [JsonPropertyName("eventQuests")]
    public Dictionary<string, EventQuestData>? EventQuests { get; set; }

    [JsonPropertyName("repeatableQuests")]
    public List<RepeatableQuestConfig>? RepeatableQuests { get; set; }

    [JsonPropertyName("locationIdMap")]
    public Dictionary<string, string>? LocationIdMap { get; set; }

    [JsonPropertyName("bearOnlyQuests")]
    public HashSet<string>? BearOnlyQuests { get; set; }

    [JsonPropertyName("usecOnlyQuests")]
    public HashSet<string>? UsecOnlyQuests { get; set; }

    /// <summary>
    ///     Quests that the keyed game version do not see/access
    /// </summary>
    [JsonPropertyName("profileBlacklist")]
    public Dictionary<string, HashSet<string>>? ProfileBlacklist { get; set; }

    /// <summary>
    ///     key=questid, gameversions that can see/access quest
    /// </summary>
    [JsonPropertyName("profileWhitelist")]
    public Dictionary<string, HashSet<string>>? ProfileWhitelist { get; set; }
}

public record PlayerTypeQuestIds
{
    [JsonPropertyName("pmc")]
    public QuestTypeIds? Pmc { get; set; }

    [JsonPropertyName("scav")]
    public QuestTypeIds? Scav { get; set; }
}

public record QuestTypeIds
{
    [JsonPropertyName("elimination")]
    public string? Elimination { get; set; }

    [JsonPropertyName("completion")]
    public string? Completion { get; set; }

    [JsonPropertyName("exploration")]
    public string? Exploration { get; set; }

    [JsonPropertyName("pickup")]
    public string? Pickup { get; set; }
}

public record EventQuestData
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("season")]
    public SeasonalEventType? Season { get; set; }

    [JsonPropertyName("startTimestamp")]
    public long? StartTimestamp { get; set; }

    [JsonPropertyName("endTimestamp")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public long? EndTimestamp { get; set; }

    [JsonPropertyName("yearly")]
    public bool? Yearly { get; set; }
}

public record RepeatableQuestConfig
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("side")]
    public string? Side { get; set; }

    [JsonPropertyName("types")]
    public List<string>? Types { get; set; }

    [JsonPropertyName("resetTime")]
    public long? ResetTime { get; set; }

    [JsonPropertyName("numQuests")]
    public int? NumQuests { get; set; }

    [JsonPropertyName("minPlayerLevel")]
    public int? MinPlayerLevel { get; set; }

    [JsonPropertyName("rewardScaling")]
    public RewardScaling? RewardScaling { get; set; }

    [JsonPropertyName("locations")]
    public Dictionary<ELocationName, List<string>>? Locations { get; set; }

    [JsonPropertyName("traderWhitelist")]
    public List<TraderWhitelist>? TraderWhitelist { get; set; }

    [JsonPropertyName("questConfig")]
    public RepeatableQuestTypesConfig? QuestConfig { get; set; }

    /// <summary>
    ///     Item base types to block when generating rewards
    /// </summary>
    [JsonPropertyName("rewardBaseTypeBlacklist")]
    public HashSet<string>? RewardBaseTypeBlacklist { get; set; }

    /// <summary>
    ///     Item tplIds to ignore when generating rewards
    /// </summary>
    [JsonPropertyName("rewardBlacklist")]
    public HashSet<string>? RewardBlacklist { get; set; }

    [JsonPropertyName("rewardAmmoStackMinSize")]
    public int? RewardAmmoStackMinSize { get; set; }

    [JsonPropertyName("freeChangesAvailable")]
    public int? FreeChangesAvailable { get; set; }

    [JsonPropertyName("freeChanges")]
    public int? FreeChanges { get; set; }

    [JsonPropertyName("keepDailyQuestTypeOnReplacement")]
    public bool? KeepDailyQuestTypeOnReplacement { get; set; }

    /// <summary>
    ///     Reputation standing price for replacing a repeatable
    /// </summary>
    [JsonPropertyName("standingChangeCost")]
    public IList<double>? StandingChangeCost { get; set; }
}

public record RewardScaling
{
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
    [JsonPropertyName("possibleSkillRewards")]
    public List<string>? PossibleSkillRewards { get; set; }
}

public record BossInfo
{
    [JsonPropertyName("isBoss")]
    public bool? IsBoss { get; set; }

    [JsonPropertyName("isPmc")]
    public bool? IsPmc { get; set; }
}
