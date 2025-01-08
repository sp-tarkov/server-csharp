using System.Reflection;
using System.Text.Json.Serialization;
using Core.Models.Eft.Ragfair;
using Core.Models.Enums;
using Core.Utils.Json.Converters;

namespace Core.Models.Eft.Common.Tables;

public class BotBase
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("aid")]
    public double? Aid { get; set; }

    /** SPT property - use to store player id - TODO - move to AID ( account id as guid of choice) */
    [JsonPropertyName("sessionId")]
    public string? SessionId { get; set; }

    [JsonPropertyName("savage")]
    public string? Savage { get; set; }

    [JsonPropertyName("karmaValue")]
    public double? KarmaValue { get; set; }

    [JsonPropertyName("Info")]
    public Info? Info { get; set; }

    [JsonPropertyName("Customization")]
    public Customization? Customization { get; set; }

    [JsonPropertyName("Health")]
    public BotBaseHealth? Health { get; set; }

    [JsonPropertyName("Inventory")]
    public BotBaseInventory? Inventory { get; set; }

    [JsonPropertyName("Skills")]
    public Skills? Skills { get; set; }

    [JsonPropertyName("Stats")]
    public Stats? Stats { get; set; }

    [JsonPropertyName("Encyclopedia")]
    public Dictionary<string, bool>? Encyclopedia { get; set; }

    [JsonPropertyName("TaskConditionCounters")]
    public Dictionary<string, TaskConditionCounter>? TaskConditionCounters { get; set; }

    [JsonPropertyName("InsuredItems")]
    public List<InsuredItem>? InsuredItems { get; set; }

    [JsonPropertyName("Hideout")]
    public Hideout? Hideout { get; set; }

    [JsonPropertyName("Quests")]
    public List<Quests>? Quests { get; set; }

    [JsonPropertyName("TradersInfo")]
    public Dictionary<string, TraderInfo>? TradersInfo { get; set; }

    [JsonPropertyName("UnlockedInfo")]
    public UnlockedInfo? UnlockedInfo { get; set; }

    [JsonPropertyName("RagfairInfo")]
    public RagfairInfo? RagfairInfo { get; set; }

    /** Achievement id and timestamp */
    [JsonPropertyName("Achievements")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    public Dictionary<string, int>? Achievements { get; set; }

    [JsonPropertyName("RepeatableQuests")]
    public List<PmcDataRepeatableQuest>? RepeatableQuests { get; set; }

    [JsonPropertyName("Bonuses")]
    public List<Bonus>? Bonuses { get; set; }

    [JsonPropertyName("Notes")]
    public Notes? Notes { get; set; }

    [JsonPropertyName("CarExtractCounts")]
    public Dictionary<string, int>? CarExtractCounts { get; set; }

    [JsonPropertyName("CoopExtractCounts")]
    public Dictionary<string, int>? CoopExtractCounts { get; set; }

    [JsonPropertyName("SurvivorClass")]
    public SurvivorClass? SurvivorClass { get; set; }

    [JsonPropertyName("WishList")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    public Dictionary<string, int>? WishList { get; set; }

    [JsonPropertyName("moneyTransferLimitData")]
    public MoneyTransferLimits? MoneyTransferLimitData { get; set; }

    /** SPT specific property used during bot generation in raid */
    [JsonPropertyName("sptIsPmc")]
    public bool? IsPmc { get; set; }
}

public class MoneyTransferLimits
{
    // Resets every 24 hours in live
    /** TODO: Implement */
    [JsonPropertyName("nextResetTime")]
    public double? NextResetTime { get; set; }

    [JsonPropertyName("remainingLimit")]
    public double? RemainingLimit { get; set; }

    [JsonPropertyName("totalLimit")]
    public double? TotalLimit { get; set; }

    [JsonPropertyName("resetInterval")]
    public double? ResetInterval { get; set; }
}

public class TaskConditionCounter
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("value")]
    public double? Value { get; set; }

    /** Quest id */
    [JsonPropertyName("sourceId")]
    public string? SourceId { get; set; }
}

public class UnlockedInfo
{
    [JsonPropertyName("unlockedProductionRecipe")]
    public List<string>? UnlockedProductionRecipe { get; set; }
}

public class Info
{
    public string? EntryPoint { get; set; }
    public string? Nickname { get; set; }
    public string? MainProfileNickname { get; set; }
    public string? LowerNickname { get; set; }
    public string? Side { get; set; }
    public bool? SquadInviteRestriction { get; set; }
    public bool? HasCoopExtension { get; set; }
    public bool? HasPveGame { get; set; }
    public string? Voice { get; set; }
    public double? Level { get; set; }
    public double? Experience { get; set; }
    public long? RegistrationDate { get; set; }
    public string? GameVersion { get; set; }
    public double? AccountType { get; set; }
    public MemberCategory? MemberCategory { get; set; }
    public MemberCategory? SelectedMemberCategory { get; set; }

    [JsonPropertyName("lockedMoveCommands")]
    public bool? LockedMoveCommands { get; set; }

    public long? SavageLockTime { get; set; }
    public long? LastTimePlayedAsSavage { get; set; }
    public BotInfoSettings? Settings { get; set; }
    public long? NicknameChangeDate { get; set; }
    public List<object>? NeedWipeOptions { get; set; }

    [JsonPropertyName("lastCompletedWipe")]
    public LastCompleted? LastCompletedWipe { get; set; }

    public List<Ban>? Bans { get; set; }
    public bool? BannedState { get; set; }
    public long? BannedUntil { get; set; }
    public bool? IsStreamerModeAvailable { get; set; }

    [JsonPropertyName("lastCompletedEvent")]
    public LastCompleted? LastCompletedEvent { get; set; }

    [JsonPropertyName("isMigratedSkills")]
    public bool? IsMigratedSkills { get; set; }
}

public class BotInfoSettings
{
    public string? Role { get; set; }
    public string? BotDifficulty { get; set; }
    public double? Experience { get; set; }
    public double? StandingForKill { get; set; }
    public double? AggressorBonus { get; set; }
    public bool? UseSimpleAnimator { get; set; }
}

public class Ban
{
    [JsonPropertyName("banType")]
    public BanType? BanType { get; set; }

    [JsonPropertyName("dateTime")]
    public long? DateTime { get; set; }
}

public enum BanType
{
    CHAT = 0,
    RAGFAIR = 1,
    VOIP = 2,
    TRADING = 3,
    ONLINE = 4,
    FRIENDS = 5,
    CHANGE_NICKNAME = 6
}

public class Customization
{
    public string? Head { get; set; }
    public string? Body { get; set; }
    public string? Feet { get; set; }
    public string? Hands { get; set; }
}

public class BotBaseHealth
{
    public CurrentMax? Hydration { get; set; }
    public CurrentMax? Energy { get; set; }
    public CurrentMax? Temperature { get; set; }
    public BodyPartsHealth? BodyParts { get; set; }
    public double? UpdateTime { get; set; }
    public bool? Immortal { get; set; }
}

public class BodyPartsHealth
{
    public BodyPartHealth? Head { get; set; }
    public BodyPartHealth? Chest { get; set; }
    public BodyPartHealth? Stomach { get; set; }
    public BodyPartHealth? LeftArm { get; set; }
    public BodyPartHealth? RightArm { get; set; }
    public BodyPartHealth? LeftLeg { get; set; }
    public BodyPartHealth? RightLeg { get; set; }
}

public class BodyPartHealth
{
    public CurrentMax? Health { get; set; }
    public Dictionary<string, BodyPartEffectProperties>? Effects { get; set; }
}

public class BodyPartEffectProperties
{
    // TODO: this was any, what actual type is it?
    public object? ExtraData { get; set; }
    public double? Time { get; set; }
}

public class CurrentMax
{
    public double? Current { get; set; }
    public double? Maximum { get; set; }
}

public class BotBaseInventory
{
    [JsonPropertyName("items")]
    public List<Item>? Items { get; set; }

    [JsonPropertyName("equipment")]
    public string? Equipment { get; set; }

    [JsonPropertyName("stash")]
    public string? Stash { get; set; }

    [JsonPropertyName("sortingTable")]
    public string? SortingTable { get; set; }

    [JsonPropertyName("questRaidItems")]
    public string? QuestRaidItems { get; set; }

    [JsonPropertyName("questStashItems")]
    public string? QuestStashItems { get; set; }

    /** Key is hideout area enum numeric as string e.g. "24", value is area _id  */
    [JsonPropertyName("hideoutAreaStashes")]
    public Dictionary<string, string>? HideoutAreaStashes { get; set; }

    [JsonPropertyName("fastPanel")]
    public Dictionary<string, string>? FastPanel { get; set; }

    [JsonPropertyName("favoriteItems")]
    public List<string>? FavoriteItems { get; set; }
}

public class BaseJsonSkills
{
    public Dictionary<string, Common>? Common { get; set; }
    public Dictionary<string, Mastering>? Mastering { get; set; }
    public double? Points { get; set; }
}

public class Skills
{
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    public Dictionary<SkillTypes, Common>? Common { get; set; }

    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    public Dictionary<string, Mastering>? Mastering { get; set; }

    public double? Points { get; set; }
}

public class BaseSkill
{
    public string? Id { get; set; }
    public double? Progress { get; set; }

    [JsonPropertyName("max")]
    public int? Max { get; set; }

    [JsonPropertyName("min")]
    public int? Min { get; set; }
}

public class Common : BaseSkill
{
    public int? PointsEarnedDuringSession { get; set; }
    public int? LastAccess { get; set; }
}

public class Mastering : BaseSkill
{
}

public class Stats
{
    public EftStats? Eft { get; set; }
}

public class EftStats
{
    public List<string>? CarriedQuestItems { get; set; }
    public List<Victim>? Victims { get; set; }
    public double? TotalSessionExperience { get; set; }
    public long? LastSessionDate { get; set; }
    public SessionCounters? SessionCounters { get; set; }
    public OverallCounters? OverallCounters { get; set; }
    public float? SessionExperienceMult { get; set; }
    public float? ExperienceBonusMult { get; set; }
    public Aggressor? Aggressor { get; set; }
    public List<DroppedItem>? DroppedItems { get; set; }
    public List<FoundInRaidItem>? FoundInRaidItems { get; set; }
    public DamageHistory? DamageHistory { get; set; }
    public DeathCause? DeathCause { get; set; }
    public LastPlayerState? LastPlayerState { get; set; }
    public double? TotalInGameTime { get; set; }
    public string? SurvivorClass { get; set; }

    [JsonPropertyName("sptLastRaidFenceRepChange")]
    public float? SptLastRaidFenceRepChange { get; set; }
}

public class DroppedItem
{
    public string? QuestId { get; set; }
    public string? ItemId { get; set; }
    public string? ZoneId { get; set; }
}

public class FoundInRaidItem
{
    public string? QuestId { get; set; }
    public string? ItemId { get; set; }
}

// TODO: Same as Aggressor?
public class Victim
{
    public string? AccountId { get; set; }
    public string? ProfileId { get; set; }
    public string? Name { get; set; }
    public string? Side { get; set; }
    public string? BodyPart { get; set; }
    public string? Time { get; set; }
    public float? Distance { get; set; }
    public double? Level { get; set; }
    public string? Weapon { get; set; }
    public string? Role { get; set; }
    public string? Location { get; set; }
}

public class SessionCounters
{
    public List<CounterKeyValue>? Items { get; set; }
}

public class OverallCounters
{
    public List<CounterKeyValue>? Items { get; set; }
}

public class CounterKeyValue
{
    public List<string>? Key { get; set; }
    public double? Value { get; set; }
}

public class Aggressor
{
    public string? AccountId { get; set; }
    public string? ProfileId { get; set; }
    public string? MainProfileNickname { get; set; }
    public string? Name { get; set; }
    public string? Side { get; set; }
    public string? BodyPart { get; set; }
    public string? HeadSegment { get; set; }
    public string? WeaponName { get; set; }
    public string? Category { get; set; }
}

public class DamageHistory
{
    public string? LethalDamagePart { get; set; }
    public LethalDamage? LethalDamage { get; set; }

    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    public BodyPartsDamageHistory? BodyParts { get; set; }
}

// TODO: this class seems exactly the same as DamageStats, why have it?
public class LethalDamage
{
    public double? Amount { get; set; }
    public string? Type { get; set; }
    public string? SourceId { get; set; }
    public string? OverDamageFrom { get; set; }
    public bool? Blunt { get; set; }
    public double? ImpactsCount { get; set; }
}

public class BodyPartsDamageHistory
{
    public List<DamageStats>? Head { get; set; }
    public List<DamageStats>? Chest { get; set; }
    public List<DamageStats>? Stomach { get; set; }
    public List<DamageStats>? LeftArm { get; set; }
    public List<DamageStats>? RightArm { get; set; }
    public List<DamageStats>? LeftLeg { get; set; }
    public List<DamageStats>? RightLeg { get; set; }
    public List<DamageStats>? Common { get; set; }
}

public class DamageStats
{
    public double? Amount { get; set; }
    public string? Type { get; set; }
    public string? SourceId { get; set; }
    public string? OverDamageFrom { get; set; }
    public bool? Blunt { get; set; }
    public double? ImpactsCount { get; set; }
}

public class DeathCause
{
    public string? DamageType { get; set; }
    public string? Side { get; set; }
    public string? Role { get; set; }
    public string? WeaponId { get; set; }
}

public class LastPlayerState
{
    public LastPlayerStateInfo? Info { get; set; }
    public Dictionary<string, string>? Customization { get; set; }

    // TODO: there is no definition on TS just any
    public object? Equipment { get; set; }
}

public class LastPlayerStateInfo
{
    public string? Nickname { get; set; }
    public string? Side { get; set; }
    public double? Level { get; set; }
    public MemberCategory? MemberCategory { get; set; }
}

public class BackendCounter
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("qid")]
    public string? QId { get; set; }

    [JsonPropertyName("value")]
    public double? Value { get; set; }
}

public class InsuredItem
{
    /** Trader Id item was insured by */
    [JsonPropertyName("tid")]
    public string? TId { get; set; }

    [JsonPropertyName("itemId")]
    public string? ItemId { get; set; }
}

public class Hideout
{
    public Dictionary<string, Production>? Production { get; set; }
    public List<BotHideoutArea>? Areas { get; set; }
    public Dictionary<string, HideoutImprovement>? Improvements { get; set; }
    public HideoutCounters? HideoutCounters { get; set; }
    public double? Seed { get; set; }
    public List<string>? MannequinPoses { get; set; }

    [JsonPropertyName("sptUpdateLastRunTimestamp")]
    public long? SptUpdateLastRunTimestamp { get; set; }
}

public class HideoutCounters
{
    [JsonPropertyName("fuelCounter")]
    public double? FuelCounter { get; set; }

    [JsonPropertyName("airFilterCounter")]
    public double? AirFilterCounter { get; set; }

    [JsonPropertyName("waterFilterCounter")]
    public double? WaterFilterCounter { get; set; }

    [JsonPropertyName("craftingTimeCounter")]
    public double? CraftingTimeCounter { get; set; }
}

public class HideoutImprovement
{
    [JsonPropertyName("completed")]
    public bool? Completed { get; set; }

    [JsonPropertyName("improveCompleteTimestamp")]
    public long? ImproveCompleteTimestamp { get; set; }
}

public class Productive
{
    public List<Product>? Products { get; set; }

    /** Seconds passed of production */
    public int? Progress { get; set; }

    /** Is craft in some state of being worked on by client (crafting/ready to pick up) */
    [JsonPropertyName("inProgress")]
    public bool? InProgress { get; set; }

    public string? StartTimestamp { get; set; }
    public int? SkipTime { get; set; }

    /** Seconds needed to fully craft */
    public int? ProductionTime { get; set; }

    public List<Item>? GivenItemsInStart { get; set; }
    public bool? Interrupted { get; set; }
    public string? Code { get; set; }
    public bool? Decoded { get; set; }
    public bool? AvailableForFinish { get; set; }

    /** Used in hideout production.json */
    public bool? needFuelForAllProductionTime { get; set; }

    /** Used when sending data to client */
    public bool? NeedFuelForAllProductionTime { get; set; }

    [JsonPropertyName("sptIsScavCase")]
    public bool? SptIsScavCase { get; set; }

    /** Some crafts are always inProgress, but need to be reset, e.g. water collector */
    [JsonPropertyName("sptIsComplete")]
    public bool? SptIsComplete { get; set; }

    /** Is the craft a Continuous, e.g bitcoins/water collector */
    [JsonPropertyName("sptIsContinuous")]
    public bool? SptIsContinuous { get; set; }

    /** Stores a list of tools used in this craft and whether they're FiR, to give back once the craft is done */
    [JsonPropertyName("sptRequiredTools")]
    public List<Item>? SptRequiredTools { get; set; }

    // Craft is cultist circle sacrifice
    [JsonPropertyName("sptIsCultistCircle")]
    public bool? SptIsCultistCircle { get; set; }
}

public class Production : Productive
{
    public string? RecipeId { get; set; }
    public int? SkipTime { get; set; }
    public int? ProductionTime { get; set; }
}

public class ScavCase : Productive
{
    public string? RecipeId { get; set; }
}

public class Product
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("_tpl")]
    public string? Template { get; set; }

    [JsonPropertyName("upd")]
    public Upd? Upd { get; set; }
}

public class BotHideoutArea
{
    [JsonPropertyName("type")]
    public HideoutAreas? Type { get; set; }

    [JsonPropertyName("level")]
    public double? Level { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("passiveBonusesEnabled")]
    public bool? PassiveBonusesEnabled { get; set; }

    /** Must be integer */
    [JsonPropertyName("completeTime")]
    public double? CompleteTime { get; set; }

    [JsonPropertyName("constructing")]
    public bool? Constructing { get; set; }

    [JsonPropertyName("slots")]
    public List<HideoutSlot>? Slots { get; set; }

    [JsonPropertyName("lastRecipe")]
    public string? LastRecipe { get; set; }
}

public class HideoutSlot
{
    /// <summary>
    /// SPT specific value to keep track of what index this slot is (0,1,2,3 etc)
    /// </summary>
    [JsonPropertyName("locationIndex")]
    public double? LocationIndex { get; set; }

    [JsonPropertyName("item")]
    public List<HideoutItem>? Items { get; set; }
}

public class HideoutItem
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("_tpl")]
    public string? Template { get; set; }

    [JsonPropertyName("upd")]
    public Upd? Upd { get; set; }
}

public class LastCompleted
{
    [JsonPropertyName("$oid")]
    public string? OId { get; set; }
}

public class Notes
{
    [JsonPropertyName("notes")]
    public List<Note>? DataNotes { get; set; }
}

public enum SurvivorClass
{
    UNKNOWN = 0,
    NEUTRALIZER = 1,
    MARAUDER = 2,
    PARAMEDIC = 3,
    SURVIVOR = 4
}

public class Quests
{
    [JsonPropertyName("qid")]
    public string? QId { get; set; }

    [JsonPropertyName("startTime")]
    public long? StartTime { get; set; }

    [JsonPropertyName("status")]
    public QuestStatus? Status { get; set; }

    [JsonPropertyName("statusTimers")]
    public Dictionary<string, long>? StatusTimers { get; set; }

    /** Property does not exist in live profile data, but is used by ProfileChanges.questsStatus when sent to client */
    [JsonPropertyName("completedConditions")]
    public List<string>? CompletedConditions { get; set; }

    [JsonPropertyName("availableAfter")]
    public long? AvailableAfter { get; set; }
}

public class TraderInfo
{
    [JsonPropertyName("loyaltyLevel")]
    public int? LoyaltyLevel { get; set; }

    [JsonPropertyName("salesSum")]
    public double? SalesSum { get; set; }

    [JsonPropertyName("standing")]
    public double? Standing { get; set; }

    [JsonPropertyName("nextResupply")]
    public double? NextResupply { get; set; }

    [JsonPropertyName("unlocked")]
    public bool? Unlocked { get; set; }

    [JsonPropertyName("disabled")]
    public bool? Disabled { get; set; }
}

public class RagfairInfo
{
    [JsonPropertyName("rating")]
    public double? Rating { get; set; }

    [JsonPropertyName("isRatingGrowing")]
    public bool? IsRatingGrowing { get; set; }

    [JsonPropertyName("offers")]
    public List<RagfairOffer>? Offers { get; set; }
}

public class Bonus
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public BonusType? Type { get; set; }

    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("passive")]
    public bool? IsPassive { get; set; }

    [JsonPropertyName("production")]
    public bool? IsProduction { get; set; }

    [JsonPropertyName("visible")]
    public bool? IsVisible { get; set; }

    [JsonPropertyName("value")]
    public double? Value { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("filter")]
    public List<string>? Filter { get; set; }

    [JsonPropertyName("skillType")]
    public BonusSkillType? SkillType { get; set; }
}

public class Note
{
    public double? Time { get; set; }
    public string? Text { get; set; }
}
