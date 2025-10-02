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
    public required HashSet<MongoId> UsecOnlyQuests { get; set; }

    /// <summary>
    ///     Collection of quests by id only available to bears
    /// </summary>
    [JsonPropertyName("bearOnlyQuests")]
    public required HashSet<MongoId> BearOnlyQuests { get; set; }

    /// <summary>
    ///     Quests that the keyed game version do not see/access
    /// </summary>
    [JsonPropertyName("profileBlacklist")]
    public required Dictionary<string, HashSet<MongoId>> ProfileBlacklist { get; set; }

    /// <summary>
    ///     key=questid, gameversions that can see/access quest
    /// </summary>
    [JsonPropertyName("profileWhitelist")]
    public required Dictionary<MongoId, HashSet<string>> ProfileWhitelist { get; set; }

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
    public required Dictionary<MongoId, EventQuestData> EventQuests { get; set; }

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
    /// <summary>
    ///     Pmc repeatable quest template ids keyed by type of quest
    /// Keys: elimination, completion, exploration
    /// </summary>
    [JsonPropertyName("pmc")]
    public required Dictionary<string, MongoId> Pmc { get; set; }

    /// <summary>
    ///     Scav repeatable quest template ids keyed by type of quest
    /// Keys: elimination, completion, exploration, pickup
    /// </summary>
    [JsonPropertyName("scav")]
    public required Dictionary<string, MongoId> Scav { get; set; }
}

public record EventQuestData
{
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
    /// <summary>
    ///     Id for type of repeatable quest
    /// </summary>
    [JsonPropertyName("id")]
    public required MongoId Id { get; set; }

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
    public required RepeatableQuestTypesConfig QuestConfig { get; set; }

    /// <summary>
    ///     Item base types to block when generating rewards
    /// </summary>
    [JsonPropertyName("rewardBaseTypeBlacklist")]
    public required HashSet<MongoId> RewardBaseTypeBlacklist { get; set; }

    /// <summary>
    ///     Item tplIds to ignore when generating rewards
    /// </summary>
    [JsonPropertyName("rewardBlacklist")]
    public required HashSet<MongoId> RewardBlacklist { get; set; }

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
    /// <summary>
    ///     Levels at which to increase to the next level of reward potential
    /// </summary>
    [JsonPropertyName("levels")]
    public required List<double> Levels { get; set; }

    /// <summary>
    ///     Experience reward tiers
    /// </summary>
    [JsonPropertyName("experience")]
    public required List<double> Experience { get; set; }

    /// <summary>
    ///     Rouble reward tiers
    /// </summary>
    [JsonPropertyName("roubles")]
    public required List<double> Roubles { get; set; }

    /// <summary>
    ///     Gp coin reward tiers
    /// </summary>
    [JsonPropertyName("gpCoins")]
    public required List<double> GpCoins { get; set; }

    /// <summary>
    ///     Item amount reward tiers
    /// </summary>
    [JsonPropertyName("items")]
    public required List<double> Items { get; set; }

    /// <summary>
    ///     reputation amount reward tiers
    /// </summary>
    [JsonPropertyName("reputation")]
    public required List<double> Reputation { get; set; }

    /// <summary>
    ///     Reward spread
    /// </summary>
    [JsonPropertyName("rewardSpread")]
    public required double RewardSpread { get; set; }

    /// <summary>
    ///     Skill reward chance tiers
    /// </summary>
    [JsonPropertyName("skillRewardChance")]
    public required List<double> SkillRewardChance { get; set; }

    /// <summary>
    ///     Skill reward amount tiers
    /// </summary>
    [JsonPropertyName("skillPointReward")]
    public required List<double> SkillPointReward { get; set; }
}

public record TraderWhitelist
{
    /// <summary>
    ///     Trader Id
    /// </summary>
    [JsonPropertyName("traderId")]
    public required MongoId TraderId { get; set; }

    /// <summary>
    ///     Human-readable name
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    ///     Quest types this trader can provide: Completion/Exploration/Elimination.
    /// </summary>
    [JsonPropertyName("questTypes")]
    public required HashSet<string> QuestTypes { get; set; }

    /// <summary>
    ///     Item categories that the reward can be
    /// </summary>
    [JsonPropertyName("rewardBaseWhitelist")]
    public required IEnumerable<MongoId> RewardBaseWhitelist { get; set; }

    /// <summary>
    ///     Can this reward be a weapon?
    /// </summary>
    [JsonPropertyName("rewardCanBeWeapon")]
    public required bool RewardCanBeWeapon { get; set; }

    /// <summary>
    ///     Chance that the reward is a weapon
    /// </summary>
    [JsonPropertyName("weaponRewardChancePercent")]
    public required double WeaponRewardChancePercent { get; set; }
}

public record RepeatableQuestTypesConfig
{
    /// <summary>
    ///     Defines exploration repeatable task generation parameters
    /// </summary>
    [JsonPropertyName("Exploration")]
    public required List<ExplorationConfig> ExplorationConfig { get; set; }

    /// <summary>
    ///     Defines completion repeatable task generation parameters
    /// </summary>
    [JsonPropertyName("Completion")]
    public required List<CompletionConfig> CompletionConfig { get; set; }

    /// <summary>
    ///     Defines pickup repeatable task generation parameters - TODO: Not implemented/No Data - NOTE: Does not work with dynamicLocale
    /// </summary>
    [JsonPropertyName("Pickup")]
    public Pickup? Pickup { get; set; }

    /// <summary>
    ///     Defines elimination repeatable task generation parameters
    /// </summary>
    [JsonPropertyName("Elimination")]
    public required List<EliminationConfig> Elimination { get; set; }
}

public record ExplorationConfig : BaseQuestConfig
{
    /// <summary>
    ///     Level range at which elimination tasks should be generated from this config
    /// </summary>
    [JsonPropertyName("levelRange")]
    public required MinMax<int> LevelRange { get; set; }

    /// <summary>
    ///     Minimum extract count that a per map extract requirement can be generated with
    /// </summary>
    [JsonPropertyName("minExtracts")]
    public required int MinimumExtracts { get; set; }

    /// <summary>
    ///     Maximum extract count that a per map extract requirement can be generated with
    /// </summary>
    [JsonPropertyName("maxExtracts")]
    public required int MaximumExtracts { get; set; }

    /// <summary>
    ///     Minimum extract count that a specific extract can be generated with
    /// </summary>
    [JsonPropertyName("minExtractsWithSpecificExit")]
    public required int MinimumExtractsWithSpecificExit { get; set; }

    /// <summary>
    ///     Maximum extract count that a specific extract can be generated with
    /// </summary>
    [JsonPropertyName("maxExtractsWithSpecificExit")]
    public required int MaximumExtractsWithSpecificExit { get; set; }

    /// <summary>
    ///     Specific extract generation data
    /// </summary>
    [JsonPropertyName("specificExits")]
    public required SpecificExits SpecificExits { get; set; }
}

public record SpecificExits
{
    /// <summary>
    ///     Chance that an operational task is generated with a specific extract
    /// </summary>
    [JsonPropertyName("chance")]
    public required double Chance { get; set; }

    /// <summary>
    ///     Whitelist of specific extract types
    /// </summary>
    [JsonPropertyName("passageRequirementWhitelist")]
    public required HashSet<string> PassageRequirementWhitelist { get; set; }
}

public record CompletionConfig : BaseQuestConfig
{
    /// <summary>
    ///     Level range at which completion tasks should be generated from this config
    /// </summary>
    [JsonPropertyName("levelRange")]
    public required MinMax<int> LevelRange { get; set; }

    /// <summary>
    ///     The minimum and maximum amounts that can be requested for an item
    /// </summary>
    [JsonPropertyName("requestedItemCount")]
    public required MinMax<int> RequestedItemCount { get; set; }

    /// <summary>
    ///     How many different unique items should be requested
    /// </summary>
    [JsonPropertyName("uniqueItemCount")]
    public required MinMax<int> UniqueItemCount { get; set; }

    /// <summary>
    ///     The minimum and maximum amounts that can be requested for bullets - TODO: Not implemented
    /// </summary>
    [JsonPropertyName("requestedBulletCount")]
    public required MinMax<int> RequestedBulletCount { get; set; }

    /// <summary>
    ///     Should the item whitelist be used
    /// </summary>
    [JsonPropertyName("useWhitelist")]
    public required bool UseWhitelist { get; set; }

    /// <summary>
    ///     Should the item blacklist be used
    /// </summary>
    [JsonPropertyName("useBlacklist")]
    public required bool UseBlacklist { get; set; }

    /// <summary>
    ///     Should the supplied items be required FiR
    /// </summary>
    [JsonPropertyName("requiredItemsAreFiR")]
    public required bool RequiredItemsAreFiR { get; set; }

    /// <summary>
    ///     Min/Max durability requirements for the item
    /// </summary>
    [JsonPropertyName("requiredItemMinDurabilityMinMax")]
    public required MinMax<int> RequiredItemMinDurabilityMinMax { get; set; }

    /// <summary>
    ///     Blacklisted item types to not collect
    /// </summary>
    [JsonPropertyName("requiredItemTypeBlacklist")]
    public required HashSet<MongoId> RequiredItemTypeBlacklist { get; set; }
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
    /// <summary>
    ///     Level range at which elimination tasks should be generated from this config
    /// </summary>
    [JsonPropertyName("levelRange")]
    public required MinMax<int> LevelRange { get; set; }

    /// <summary>
    ///     Target data probabilities
    /// </summary>
    [JsonPropertyName("targets")]
    public required List<ProbabilityObject<string, BossInfo>> Targets { get; set; }

    /// <summary>
    ///     Chance that a specific body part is needed as a requirement
    /// </summary>
    [JsonPropertyName("bodyPartChance")]
    public required int BodyPartChance { get; set; }

    /// <summary>
    ///     If the specific body part requirement is chosen, pick from these body parts
    /// </summary>
    [JsonPropertyName("bodyParts")]
    public required List<ProbabilityObject<string, List<string>>> BodyParts { get; set; }

    /// <summary>
    ///     Chance that a specific location modifier is selected
    /// </summary>
    [JsonPropertyName("specificLocationChance")]
    public required int SpecificLocationChance { get; set; }

    /// <summary>
    ///     Locations that should be blacklisted as a requirement
    /// </summary>
    [JsonPropertyName("distLocationBlacklist")]
    public required HashSet<string> DistLocationBlacklist { get; set; }

    /// <summary>
    ///     Probability that a distance requirement is chosen
    /// </summary>
    [JsonPropertyName("distProb")]
    public required double DistanceProbability { get; set; }

    /// <summary>
    ///     Maximum distance in meters that can be chosen
    /// </summary>
    [JsonPropertyName("maxDist")]
    public required double MaxDistance { get; set; }

    /// <summary>
    ///     Minimum distance in meters that can be chosen
    /// </summary>
    [JsonPropertyName("minDist")]
    public required double MinDistance { get; set; }

    /// <summary>
    ///     Maximum amount of kills that can be chosen
    /// </summary>
    [JsonPropertyName("maxKills")]
    public required int MaxKills { get; set; }

    /// <summary>
    ///     Minimum amount of kills that can be chosen
    /// </summary>
    [JsonPropertyName("minKills")]
    public required int MinKills { get; set; }

    /// <summary>
    ///     Maximum amount of boss kills that can be chosen
    /// </summary>
    [JsonPropertyName("maxBossKills")]
    public required int MaxBossKills { get; set; }

    /// <summary>
    ///     Minimum amount of boss kills that can be chosen
    /// </summary>
    [JsonPropertyName("minBossKills")]
    public required int MinBossKills { get; set; }

    /// <summary>
    ///     Maximum amount of PMC kills that can be chosen
    /// </summary>
    [JsonPropertyName("maxPmcKills")]
    public required int MaxPmcKills { get; set; }

    /// <summary>
    ///     Minimum amount of PMC kills that can be chosen
    /// </summary>
    [JsonPropertyName("minPmcKills")]
    public required int MinPmcKills { get; set; }

    /// <summary>
    ///     Chance that a specific weapon requirement is chosen
    /// </summary>
    [JsonPropertyName("weaponRequirementChance")]
    public required int WeaponRequirementChance { get; set; }

    /// <summary>
    ///     Chance that a weapon category requirement is chosen
    /// </summary>
    [JsonPropertyName("weaponCategoryRequirementChance")]
    public required int WeaponCategoryRequirementChance { get; set; }

    /// <summary>
    ///     If a weapon category requirement is chosen, pick from these categories
    /// </summary>
    [JsonPropertyName("weaponCategoryRequirements")]
    public required List<ProbabilityObject<string, List<MongoId>>> WeaponCategoryRequirements { get; set; }

    /// <summary>
    ///     If a weapon requirement is chosen, pick from these weapons
    /// </summary>
    [JsonPropertyName("weaponRequirements")]
    public required List<ProbabilityObject<string, List<MongoId>>> WeaponRequirements { get; set; }
}

public record BaseQuestConfig
{
    /// <summary>
    ///     Possible skills that can be rewarded expirence points
    /// </summary>
    [JsonPropertyName("possibleSkillRewards")]
    public List<string> PossibleSkillRewards { get; set; }
}

public record BossInfo
{
    /// <summary>
    ///     Is this target a boss
    /// </summary>
    [JsonPropertyName("isBoss")]
    public bool? IsBoss { get; set; }

    /// <summary>
    ///     Is ths target a PMC
    /// </summary>
    [JsonPropertyName("isPmc")]
    public bool? IsPmc { get; set; }
}
