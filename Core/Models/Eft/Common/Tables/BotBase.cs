using System.Text.Json.Serialization;

namespace Core.Models.Eft.Common.Tables;

public class BotBase
{
    
}


public class Info
{
    public string EntryPoint { get; set; }
    public string Nickname { get; set; }
    public string? MainProfileNickname { get; set; }
    public string LowerNickname { get; set; }
    public string Side { get; set; }
    public bool SquadInviteRestriction { get; set; }
    public bool HasCoopExtension { get; set; }
    public bool HasPveGame { get; set; }
    public string Voice { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public long RegistrationDate { get; set; }
    public string GameVersion { get; set; }
    public int AccountType { get; set; }
    public MemberCategory MemberCategory { get; set; }
    public MemberCategory SelectedMemberCategory { get; set; }
    
    [JsonPropertyName("lockedMoveCommands")]
    public bool LockedMoveCommands { get; set; }
    public long SavageLockTime { get; set; }
    public long LastTimePlayedAsSavage { get; set; }
    public BotInfoSettings Settings { get; set; }
    public long NicknameChangeDate { get; set; }
    public List<object> NeedWipeOptions { get; set; }
    
    [JsonPropertyName("lastCompletedWipe")]
    public LastCompleted LastCompletedWipe { get; set; }
    public List<Ban> Bans { get; set; }
    public bool BannedState { get; set; }
    public long BannedUntil { get; set; }
    public bool IsStreamerModeAvailable { get; set; }
    
    [JsonPropertyName("lastCompletedEvent")]
    public LastCompleted? LastCompletedEvent { get; set; }
    
    [JsonPropertyName("isMigratedSkills")]
    public bool IsMigratedSkills { get; set; }
}

public class BotInfoSettings
{
    public string Role { get; set; }
    public string BotDifficulty { get; set; }
    public int Experience { get; set; }
    public int StandingForKill { get; set; }
    public int AggressorBonus { get; set; }
    public bool UseSimpleAnimator { get; set; }
}

public class Ban
{
    [JsonPropertyName("banType")]
    public BanType BanType { get; set; }
    [JsonPropertyName("dateTime")]
    public long DateTime { get; set; }
}

public enum BanType
{
    CHAT = 0,
    RAGFAIR = 1,
    VOIP = 2,
    TRADING = 3,
    ONLINE = 4,
    FRIENDS = 5,
    CHANGE_NICKNAME = 6,
}

public class Customization
{
    public string Head { get; set; }
    public string Body { get; set; }
    public string Feet { get; set; }
    public string Hands { get; set; }
}

public class Health
{
    public CurrentMax Hydration { get; set; }
    public CurrentMax Energy { get; set; }
    public CurrentMax Temperature { get; set; }
    public BodyPartsHealth BodyParts { get; set; }
    public int UpdateTime { get; set; }
    public bool? Immortal { get; set; }
}

public class BodyPartsHealth
{
    public BodyPartHealth Head { get; set; }
    public BodyPartHealth Chest { get; set; }
    public BodyPartHealth Stomach { get; set; }
    public BodyPartHealth LeftArm { get; set; }
    public BodyPartHealth RightArm { get; set; }
    public BodyPartHealth LeftLeg { get; set; }
    public BodyPartHealth RightLeg { get; set; }
}

public class BodyPartHealth
{
    public CurrentMax Health { get; set; }
    public Dictionary<string, BodyPartEffectProperties>? Effects { get; set; }
}

public class BodyPartEffectProperties
{
    // TODO: this was any, what actual type is it?
    public object? ExtraData { get; set; }
    public int Time { get; set; }
}

public class CurrentMax
{
    public int Current { get; set; }
    public int Maximum { get; set; }
}

public class Inventory {
    [JsonPropertyName("items")]
    public List<Item> Items { get; set; }
    [JsonPropertyName("equipment")]
    public string Equipment { get; set; }
    [JsonPropertyName("stash")]
    public string Stash { get; set; }
    [JsonPropertyName("sortingTable")]
    public string SortingTable { get; set; }
    [JsonPropertyName("questRaidItems")]
    public string QuestRaidItems { get; set; }
    [JsonPropertyName("questStashItems")]
    public string QuestStashItems { get; set; }
    /** Key is hideout area enum numeric as string e.g. "24", value is area _id  */
    [JsonPropertyName("hideoutAreaStashes")]
    public Dictionary<string, string> HideoutAreaStashes { get; set; }
    [JsonPropertyName("fastPanel")]
    public Dictionary<string, string> FastPanel { get; set; }
    [JsonPropertyName("favoriteItems")]
    public List<string> FavoriteItems { get; set; }
}

public class BaseJsonSkills {
    public Dictionary<string, Common> Common { get; set; }
    public Dictionary<string, Mastering> Mastering { get; set; }
    public int Points { get; set; }
}

public class Skills {
    public List<Common> Common { get; set; }
    public List<Mastering> Mastering { get; set; }
    public int Points { get; set; }
}

public class BaseSkill {
    public string Id { get; set; }
    public int Progress { get; set; }
    [JsonPropertyName("max")]
    public int? Max { get; set; }
    [JsonPropertyName("min")]
    public int? Min { get; set; }
}

public class Common : BaseSkill {
    public int? PointsEarnedDuringSession { get; set; }
    public int? LastAccess { get; set; }
}

public class Mastering : BaseSkill {}

public class Stats {
    public EftStats? Eft { get; set; }
}

public class EftStats
{
    public List<string> CarriedQuestItems { get; set; }
    public List<Victim> Victims { get; set; }
    public int TotalSessionExperience { get; set; }
    public long LastSessionDate { get; set; }
    public SessionCounters SessionCounters { get; set; }
    public OverallCounters OverallCounters { get; set; }
    public float? SessionExperienceMult { get; set; }
    public float? ExperienceBonusMult { get; set; }
    public Aggressor? Aggressor { get; set; }
    public List<DroppedItem>? DroppedItems { get; set; }
    public List<FoundInRaidItem>? FoundInRaidItems { get; set; }
    public DamageHistory? DamageHistory { get; set; }
    public DeathCause? DeathCause { get; set; }
    public LastPlayerState? LastPlayerState { get; set; }
    public int TotalInGameTime { get; set; }
    public string? SurvivorClass { get; set; }
    [JsonPropertyName("sptLastRaidFenceRepChange")]
    public float? SptLastRaidFenceRepChange { get; set; }
}

public class DroppedItem
{
    public string QuestId { get; set; }
    public string ItemId { get; set; }
    public string ZoneId { get; set; }
}

public class FoundInRaidItem
{
    public string QuestId { get; set; }
    public string ItemId { get; set; }
}

// TODO: Same as Aggressor?
public class Victim
{
    public string AccountId { get; set; }
    public string ProfileId { get; set; }
    public string Name { get; set; }
    public string Side { get; set; }
    public string BodyPart { get; set; }
    public string Time { get; set; }
    public float Distance { get; set; }
    public int Level { get; set; }
    public string Weapon { get; set; }
    public string Role { get; set; }
    public string Location { get; set; }
}

public class SessionCounters
{
    public List<CounterKeyValue> Items { get; set; }
}

public class OverallCounters
{
    public List<CounterKeyValue> Items { get; set; }
}

public class CounterKeyValue
{
    public List<string> Key { get; set; }
    public int Value { get; set; }
}

public class Aggressor 
{
    public string AccountId { get; set; }
    public string ProfileId { get; set; }
    public string MainProfileNickname { get; set; }
    public string Name { get; set; }
    public string Side { get; set; }
    public string BodyPart { get; set; }
    public string HeadSegment { get; set; }
    public string WeaponName { get; set; }
    public string Category { get; set; }
}

public class DamageHistory 
{
    public string LethalDamagePart { get; set; }
    public LethalDamage LethalDamage { get; set; }
    public BodyPartsDamageHistory BodyParts { get; set; }
}

// TODO: this class seems exactly the same as DamageStats, why have it?
public class LethalDamage 
{
    public int Amount { get; set; }
    public string Type { get; set; }
    public string SourceId { get; set; }
    public string OverDamageFrom { get; set; }
    public bool Blunt { get; set; }
    public int ImpactsCount { get; set; }
}

public class BodyPartsDamageHistory
{
    public List<DamageStats> Head { get; set; }
    public List<DamageStats> Chest { get; set; }
    public List<DamageStats> Stomach { get; set; }
    public List<DamageStats> LeftArm { get; set; }
    public List<DamageStats> RightArm { get; set; }
    public List<DamageStats> LeftLeg { get; set; }
    public List<DamageStats> RightLeg { get; set; }
    public List<DamageStats> Common { get; set; }
}

public class DamageStats
{
    public int Amount { get; set; }
    public string Type { get; set; }
    public string SourceId { get; set; }
    public string OverDamageFrom { get; set; }
    public bool Blunt { get; set; }
    public int ImpactsCount { get; set; }
}

public class DeathCause
{
    public string DamageType { get; set; }
    public string Side { get; set; }
    public string Role { get; set; }
    public string WeaponId { get; set; }
}

public class LastPlayerState
{
    public LastPlayerStateInfo Info { get; set; }
    public Dictionary<string, string> Customization { get; set; }

    // TODO: there is no definition on TS just any
    public object Equipment { get; set; }
}

public class LastPlayerStateInfo
{
    public string Nickname { get; set; }
    public string Side { get; set; }
    public int Level { get; set; }
    public MemberCategory MemberCategory { get; set; }
}

public class BackendCounter
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("qid")]
    public string? QId { get; set; }
    [JsonPropertyName("value")]
    public int Value { get; set; }
}

public class InsuredItem
{
    /** Trader Id item was insured by */
    [JsonPropertyName("tid")]
    public string TId { get; set; }
    [JsonPropertyName("itemId")]
    public string ItemId { get; set; }
}

public class Hideout
{
    public Dictionary<string, Production> Production { get; set; }
    public List<BotHideoutArea> Areas { get; set; }
    public Dictionary<string, HideoutImprovement> Improvements { get; set; }
    public HideoutCounters HideoutCounters { get; set; }
    public int Seed { get; set; }
    public List<string> MannequinPoses { get; set; }
    [JsonPropertyName("sptUpdateLastRunTimestamp")]
    public long SptUpdateLastRunTimestamp { get; set; }
}

public class HideoutCounters
{
    [JsonPropertyName("fuelCounter")]
    public int FuelCounter { get; set; }

    [JsonPropertyName("airFilterCounter")]
    public int AirFilterCounter { get; set; }

    [JsonPropertyName("waterFilterCounter")]
    public int WaterFilterCounter { get; set; }

    [JsonPropertyName("craftingTimeCounter")]
    public int CraftingTimeCounter { get; set; }
}

public class HideoutImprovement
{
    [JsonPropertyName("completed")]
    public bool Completed { get; set; }
    [JsonPropertyName("improveCompleteTimestamp")]
    public long ImproveCompleteTimestamp { get; set; }
}

public class Productive
{
    public List<Product> Products { get; set; }

    /** Seconds passed of production */
    public int? Progress { get; set; }

    /** Is craft in some state of being worked on by client (crafting/ready to pick up) */
    [JsonPropertyName("inProgress")]
    public bool? InProgress { get; set; }

    public string StartTimestamp { get; set; }
    public int? SkipTime { get; set; }

    /** Seconds needed to fully craft */
    public int? ProductionTime { get; set; }

    public List<Item> GivenItemsInStart { get; set; }
    public bool? Interrupted { get; set; }
    public string Code { get; set; }
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
    public List<Item> SptRequiredTools { get; set; }

    // Craft is cultist circle sacrifice
    [JsonPropertyName("sptIsCultistCircle")]
    public bool? SptIsCultistCircle { get; set; }
}

public class Production : Productive
{
    public string RecipeId { get; set; }
    public int? SkipTime { get; set; }
    public int? ProductionTime { get; set; }
}

public class ScavCase : Productive
{
    public string RecipeId { get; set; }
}

public class Product
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    [JsonPropertyName("_tpl")]
    public string Template { get; set; }
    [JsonPropertyName("upd")]
    public Upd? Upd { get; set; }
}

public class BotHideoutArea
{
    [JsonPropertyName("type")]
    public HideoutAreas Type { get; set; }
    [JsonPropertyName("level")]
    public int Level { get; set; }
    [JsonPropertyName("active")]
    public bool Active { get; set; }
    [JsonPropertyName("passiveBonusesEnabled")]
    public bool PassiveBonusesEnabled { get; set; }
    /** Must be integer */
    [JsonPropertyName("completeTime")]
    public int CompleteTime { get; set; }
    [JsonPropertyName("constructing")]
    public bool Constructing { get; set; }
    [JsonPropertyName("slots")]
    public List<HideoutSlot> Slots { get; set; }
    [JsonPropertyName("lastRecipe")]
    public string LastRecipe { get; set; }
}

public class HideoutSlot
{
    /// <summary>
    /// SPT specific value to keep track of what index this slot is (0,1,2,3 etc)
    /// </summary>
    [JsonPropertyName("locationIndex")]
    public int LocationIndex { get; set; }
    [JsonPropertyName("item")]
    public List<HideoutItem>? Items { get; set; }
}

public class HideoutItem
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    [JsonPropertyName("_tpl")]
    public string Template { get; set; }
    [JsonPropertyName("upd")]
    public Upd? Upd { get; set; }
}

public class LastCompleted
{
    [JsonPropertyName("$oid")]
    public string OId { get; set; }
}

public class Notes
{
    [JsonPropertyName("notes")]
    public List<Note> DataNotes { get; set; }
}

public enum SurvivorClass {
    UNKNOWN = 0,
    NEUTRALIZER = 1,
    MARAUDER = 2,
    PARAMEDIC = 3,
    SURVIVOR = 4,
}

public class QuestStatus
{
    [JsonPropertyName("qid")]
    public string QId { get; set; }
    [JsonPropertyName("startTime")]
    public long StartTime { get; set; }
    [JsonPropertyName("status")]
    public QuestStatus Status { get; set; }
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
    public int SalesSum { get; set; }
    [JsonPropertyName("standing")]
    public int Standing { get; set; }
    [JsonPropertyName("nextResupply")]
    public int NextResupply { get; set; }
    [JsonPropertyName("unlocked")]
    public bool Unlocked { get; set; }
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }
}

public class RagfairInfo 
{
    [JsonPropertyName("rating")]
    public double Rating { get; set; }
    [JsonPropertyName("isRatingGrowing")]
    public bool IsRatingGrowing { get; set; }
    [JsonPropertyName("offers")]
    public List<RagfairOffer> Offers { get; set; }
}


public class Bonus
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    [JsonPropertyName("type")]
    public BonusType Type { get; set; }
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

public class Note {
    public double Time { get; set; }
    public string Text { get; set; }
}