using System.Text.Json.Serialization;

namespace Core.Models.Spt.Config;

public class QuestConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-quest";

    // Hours to get/redeem items from quest mail keyed by profile type
    [JsonPropertyName("mailRedeemTimeHours")]
    public Dictionary<string, double> MailRedeemTimeHours { get; set; }

    [JsonPropertyName("questTemplateIds")]
    public PlayerTypeQuestIds QuestTemplateIds { get; set; }

    /** Show non-seasonal quests be shown to player */
    [JsonPropertyName("showNonSeasonalEventQuests")]
    public bool ShowNonSeasonalEventQuests { get; set; }

    [JsonPropertyName("eventQuests")]
    public Dictionary<string, EventQuestData> EventQuests { get; set; }

    [JsonPropertyName("repeatableQuests")]
    public List<RepeatableQuestConfig> RepeatableQuests { get; set; }

    [JsonPropertyName("locationIdMap")]
    public Dictionary<string, string> LocationIdMap { get; set; }

    [JsonPropertyName("bearOnlyQuests")]
    public List<string> BearOnlyQuests { get; set; }

    [JsonPropertyName("usecOnlyQuests")]
    public List<string> UsecOnlyQuests { get; set; }

    /** Quests that the keyed game version do not see/access */
    [JsonPropertyName("profileBlacklist")]
    public Dictionary<string, List<string>> ProfileBlacklist { get; set; }

    /** key=questid, gameversions that can see/access quest */
    [JsonPropertyName("profileWhitelist")]
    public Dictionary<string, List<string>> ProfileWhitelist { get; set; }
}

public class PlayerTypeQuestIds
{
    [JsonPropertyName("pmc")]
    public QuestTypeIds Pmc { get; set; }

    [JsonPropertyName("scav")]
    public QuestTypeIds Scav { get; set; }
}

public class QuestTypeIds
{
    [JsonPropertyName("Elimination")]
    public string Elimination { get; set; }

    [JsonPropertyName("Completion")]
    public string Completion { get; set; }

    [JsonPropertyName("Exploration")]
    public string Exploration { get; set; }
}

public class EventQuestData
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("season")]
    public SeasonalEventType Season { get; set; }

    [JsonPropertyName("startTimestamp")]
    public long StartTimestamp { get; set; }

    [JsonPropertyName("endTimestamp")]
    public long EndTimestamp { get; set; }

    [JsonPropertyName("yearly")]
    public bool Yearly { get; set; }
}

public class RepeatableQuestConfig
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("side")]
    public string Side { get; set; }

    [JsonPropertyName("types")]
    public List<string> Types { get; set; }

    [JsonPropertyName("resetTime")]
    public long ResetTime { get; set; }

    [JsonPropertyName("numQuests")]
    public int NumQuests { get; set; }

    [JsonPropertyName("minPlayerLevel")]
    public int MinPlayerLevel { get; set; }

    [JsonPropertyName("rewardScaling")]
    public RewardScaling RewardScaling { get; set; }

    [JsonPropertyName("locations")]
    public Dictionary<ELocationName, List<string>> Locations { get; set; }

    [JsonPropertyName("traderWhitelist")]
    public List<TraderWhitelist> TraderWhitelist { get; set; }

    [JsonPropertyName("questConfig")]
    public RepeatableQuestTypesConfig QuestConfig { get; set; }

    /** Item base types to block when generating rewards */
    [JsonPropertyName("rewardBaseTypeBlacklist")]
    public List<string> RewardBaseTypeBlacklist { get; set; }

    /** Item tplIds to ignore when generating rewards */
    [JsonPropertyName("rewardBlacklist")]
    public List<string> RewardBlacklist { get; set; }

    [JsonPropertyName("rewardAmmoStackMinSize")]
    public int RewardAmmoStackMinSize { get; set; }

    [JsonPropertyName("freeChangesAvailable")]
    public int FreeChangesAvailable { get; set; }

    [JsonPropertyName("freeChanges")]
    public int FreeChanges { get; set; }

    [JsonPropertyName("keepDailyQuestTypeOnReplacement")]
    public bool KeepDailyQuestTypeOnReplacement { get; set; }
}

public class RewardScaling
{
    [JsonPropertyName("levels")]
    public List<int> Levels { get; set; }
    
    [JsonPropertyName("experience")]
    public List<int> Experience { get; set; }
    
    [JsonPropertyName("roubles")]
    public List<int> Roubles { get; set; }
    
    [JsonPropertyName("gpCoins")]
    public List<int> GpCoins { get; set; }
    
    [JsonPropertyName("items")]
    public List<int> Items { get; set; }
    
    [JsonPropertyName("reputation")]
    public List<int> Reputation { get; set; }
    
    [JsonPropertyName("rewardSpread")]
    public int RewardSpread { get; set; }
    
    [JsonPropertyName("skillRewardChance")]
    public List<int> SkillRewardChance { get; set; }
    
    [JsonPropertyName("skillPointReward")]
    public List<int> SkillPointReward { get; set; }
}

public class TraderWhitelist
{
    [JsonPropertyName("traderId")]
    public string TraderId { get; set; }
    
    [JsonPropertyName("questTypes")]
    public List<string> QuestTypes { get; set; }
    
    [JsonPropertyName("rewardBaseWhitelist")]
    public List<string> RewardBaseWhitelist { get; set; }
    
    [JsonPropertyName("rewardCanBeWeapon")]
    public bool RewardCanBeWeapon { get; set; }
    
    [JsonPropertyName("weaponRewardChancePercent")]
    public double WeaponRewardChancePercent { get; set; }
}

public class RepeatableQuestTypesConfig
{
    [JsonPropertyName("Exploration")]
    public Exploration Exploration { get; set; }
    
    [JsonPropertyName("Completion")]
    public Completion Completion { get; set; }
    
    [JsonPropertyName("Pickup")]
    public Pickup Pickup { get; set; }
    
    [JsonPropertyName("Elimination")]
    public List<EliminationConfig> Elimination { get; set; }
}

public class Exploration : BaseQuestConfig
{
    [JsonPropertyName("maxExtracts")]
    public int MaximumExtracts { get; set; }

    [JsonPropertyName("maxExtractsWithSpecificExit")]
    public int MaximumExtractsWithSpecificExit { get; set; }

    [JsonPropertyName("specificExits")]
    public SpecificExits SpecificExits { get; set; }
}

public class SpecificExits
{
    [JsonPropertyName("probability")]
    public int Probability { get; set; }

    [JsonPropertyName("passageRequirementWhitelist")]
    public List<string> PassageRequirementWhitelist { get; set; }
}

public class Completion : BaseQuestConfig
{
    [JsonPropertyName("minRequestedAmount")]
    public int MinimumRequestedAmount { get; set; }

    [JsonPropertyName("maxRequestedAmount")]
    public int MaximumRequestedAmount { get; set; }

    [JsonPropertyName("uniqueItemCount")]
    public int UniqueItemCount { get; set; }

    [JsonPropertyName("minRequestedBulletAmount")]
    public int MinimumRequestedBulletAmount { get; set; }

    [JsonPropertyName("maxRequestedBulletAmount")]
    public int MaximumRequestedBulletAmount { get; set; }

    [JsonPropertyName("useWhitelist")]
    public bool UseWhitelist { get; set; }

    [JsonPropertyName("useBlacklist")]
    public bool UseBlacklist { get; set; }
}

public class Pickup : BaseQuestConfig
{
    [JsonPropertyName("ItemTypeToFetchWithMaxCount")]
    public List<PickupTypeWithMaxCount> ItemTypeToFetchWithMaxCount { get; set; }
}

public class PickupTypeWithMaxCount
{
    [JsonPropertyName("itemType")]
    public string ItemType { get; set; }

    [JsonPropertyName("maxPickupCount")]
    public int MaximumPickupCount { get; set; }

    [JsonPropertyName("minPickupCount")]
    public int MinimumPickupCount { get; set; }
}

public class EliminationConfig : BaseQuestConfig
{
    [JsonPropertyName("levelRange")]
    public MinMax LevelRange { get; set; }

    [JsonPropertyName("targets")]
    public List<Target> Targets { get; set; }

    [JsonPropertyName("bodyPartProb")]
    public double BodyPartProbability { get; set; }

    [JsonPropertyName("bodyParts")]
    public List<BodyPart> BodyParts { get; set; }

    [JsonPropertyName("specificLocationProb")]
    public double SpecificLocationProbability { get; set; }

    [JsonPropertyName("distLocationBlacklist")]
    public List<string> DistLocationBlacklist { get; set; }

    [JsonPropertyName("distProb")]
    public double DistanceProbability { get; set; }

    [JsonPropertyName("maxDist")]
    public double MaxDistance { get; set; }

    [JsonPropertyName("minDist")]
    public double MinDistance { get; set; }

    [JsonPropertyName("maxKills")]
    public int MaxKills { get; set; }

    [JsonPropertyName("minKills")]
    public int MinKills { get; set; }

    [JsonPropertyName("minBossKills")]
    public int MinBossKills { get; set; }

    [JsonPropertyName("maxBossKills")]
    public int MaxBossKills { get; set; }

    [JsonPropertyName("minPmcKills")]
    public int MinPmcKills { get; set; }

    [JsonPropertyName("maxPmcKills")]
    public int MaxPmcKills { get; set; }

    [JsonPropertyName("weaponCategoryRequirementProb")]
    public double WeaponCategoryRequirementProbability { get; set; }

    [JsonPropertyName("weaponCategoryRequirements")]
    public List<WeaponRequirement> WeaponCategoryRequirements { get; set; }

    [JsonPropertyName("weaponRequirementProb")]
    public double WeaponRequirementProbability { get; set; }

    [JsonPropertyName("weaponRequirements")]
    public List<WeaponRequirement> WeaponRequirements { get; set; }
}

public class BaseQuestConfig
{
    [JsonPropertyName("possibleSkillRewards")]
    public List<string> PossibleSkillRewards { get; set; }
}

public class Target : ProbabilityObject
{
    [JsonPropertyName("data")]
    public BossInfo Data { get; set; }
}

public class BossInfo
{
    [JsonPropertyName("isBoss")]
    public bool IsBoss { get; set; }

    [JsonPropertyName("isPmc")]
    public bool IsPmc { get; set; }
}

public class BodyPart : ProbabilityObject {
    [JsonPropertyName("data")]
    public string[] Data { get; set; }
}

public class WeaponRequirement : ProbabilityObject {
    [JsonPropertyName("data")]
    public string[] Data { get; set; }
}

public class ProbabilityObject {
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("relativeProbability")]
    public double RelativeProbability { get; set; }

    [JsonPropertyName("data")]
    public object Data { get; set; }
}