using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Notes;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record BotBase
{
    [JsonPropertyName("_id")]
    public MongoId? Id { get; set; }

    [JsonPropertyName("aid")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public int? Aid { get; set; }

    /// <summary>
    ///     SPT property - use to store player id - TODO - move to AID ( account id as guid of choice)
    /// </summary>
    [JsonPropertyName("sessionId")]
    public MongoId? SessionId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("savage")]
    public MongoId? Savage { get; set; }

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

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("Encyclopedia")]
    public Dictionary<MongoId, bool>? Encyclopedia { get; set; }

    [JsonPropertyName("TaskConditionCounters")]
    public Dictionary<MongoId, TaskConditionCounter>? TaskConditionCounters { get; set; }

    [JsonPropertyName("InsuredItems")]
    public List<InsuredItem>? InsuredItems { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("Hideout")]
    public Hideout? Hideout { get; set; }

    [JsonPropertyName("Quests")]
    public List<QuestStatus>? Quests { get; set; }

    [JsonPropertyName("TradersInfo")]
    public Dictionary<MongoId, TraderInfo> TradersInfo { get; set; }

    [JsonPropertyName("UnlockedInfo")]
    public UnlockedInfo? UnlockedInfo { get; set; }

    [JsonPropertyName("RagfairInfo")]
    public RagfairInfo? RagfairInfo { get; set; }

    /// <summary>
    ///     Achievement id and timestamp
    /// </summary>
    [JsonPropertyName("Achievements")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    public Dictionary<MongoId, long>? Achievements { get; set; }

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

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("WishList")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    public Dictionary<MongoId, int>? WishList { get; set; }

    public Dictionary<MongoId, int>? Variables { get; set; }

    [JsonPropertyName("moneyTransferLimitData")]
    public MoneyTransferLimits MoneyTransferLimitData { get; set; }

    /// <summary>
    ///     SPT specific property used during bot generation in raid
    /// </summary>
    [JsonPropertyName("sptIsPmc")]
    public bool? IsPmc { get; set; }
}

public record MoneyTransferLimits
{
    // Resets every 24 hours in live
    /// <summary>
    ///     TODO: Implement
    /// </summary>
    [JsonPropertyName("nextResetTime")]
    public double? NextResetTime { get; set; }

    [JsonPropertyName("remainingLimit")]
    public double? RemainingLimit { get; set; }

    [JsonPropertyName("totalLimit")]
    public double? TotalLimit { get; set; }

    [JsonPropertyName("resetInterval")]
    public double? ResetInterval { get; set; }
}

public record TaskConditionCounter
{
    private string? _type;

    [JsonPropertyName("id")]
    public MongoId? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type
    {
        get { return _type; }
        set { _type = value == null ? null : string.Intern(value); }
    }

    [JsonPropertyName("value")]
    public double? Value { get; set; }

    /// <summary>
    ///     Quest id
    /// </summary>
    [JsonPropertyName("sourceId")]
    public MongoId? SourceId { get; set; }
}

public record UnlockedInfo
{
    [JsonPropertyName("unlockedProductionRecipe")]
    public HashSet<MongoId>? UnlockedProductionRecipe { get; set; }
}

public record Info
{
    private string? _side;

    public string? EntryPoint { get; set; }

    public string? Nickname { get; set; }

    public string? MainProfileNickname { get; set; }

    public string? LowerNickname { get; set; }

    public string? Side
    {
        get { return _side; }
        set { _side = string.Intern(value); }
    }

    public int? Level { get; set; }

    //Experience the bot has gained
    // Confirmed in client
    public int? Experience { get; set; }

    // Confirmed in client
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public int? RegistrationDate { get; set; }

    public string? GameVersion { get; set; }

    public double? AccountType { get; set; }

    public MemberCategory? MemberCategory { get; set; }

    public MemberCategory? SelectedMemberCategory { get; set; }

    public IEnumerable<Ban>? Bans { get; set; }

    [JsonPropertyName("lockedMoveCommands")]
    public bool? LockedMoveCommands { get; set; }

    public double? SavageLockTime { get; set; }

    public long? LastTimePlayedAsSavage { get; set; }

    public BotInfoSettings? Settings { get; set; }

    public long? NicknameChangeDate { get; set; }

    public List<object>? NeedWipeOptions { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("lastCompletedWipe")]
    public LastCompleted? LastCompletedWipe { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("lastWipeTimestamp")]
    public LastCompleted? LastWipeTimestamp { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("lastCompletedEvent")]
    public LastCompleted? LastCompletedEvent { get; set; }

    public string? GroupId { get; set; }

    public string? TeamId { get; set; }

    public bool? BannedState { get; set; }

    public long? BannedUntil { get; set; }

    public bool? IsStreamerModeAvailable { get; set; }

    public bool? SquadInviteRestriction { get; set; }

    public bool? HasCoopExtension { get; set; }

    public bool? HasPveGame { get; set; }

    [JsonPropertyName("isMigratedSkills")]
    public bool? IsMigratedSkills { get; set; }

    public string? Type { get; set; }

    // Confirmed in client
    public int? PrestigeLevel { get; set; }
}

public record BotInfoSettings
{
    private string? _botDifficulty;
    private string? _role;

    public string? Role
    {
        get { return _role; }
        set { _role = value == null ? null : string.Intern(value); }
    }

    public string? BotDifficulty
    {
        get { return _botDifficulty; }
        set { _botDifficulty = value == null ? null : string.Intern(value); }
    }

    // Experience given for being killed
    public int? Experience { get; set; }

    public double? StandingForKill { get; set; }

    public double? AggressorBonus { get; set; }

    public bool? UseSimpleAnimator { get; set; }
}

public record Ban
{
    [JsonPropertyName("banType")]
    public BanType? BanType { get; set; }

    [JsonPropertyName("dateTime")]
    public long? DateTime { get; set; }
}

public enum BanType
{
    Chat,
    RagFair,
    Voip,
    Trading,
    Online,
    Friends,
    ChangeNickname,
}

public record Customization
{
    public MongoId? Head { get; set; }

    public MongoId? Body { get; set; }

    public MongoId? Feet { get; set; }

    public MongoId? Hands { get; set; }

    public MongoId? DogTag { get; set; }

    public MongoId? Voice { get; set; }
}

public record BotBaseHealth
{
    public CurrentMinMax? Hydration { get; set; }

    public CurrentMinMax? Energy { get; set; }

    public CurrentMinMax? Temperature { get; set; }

    public CurrentMinMax? Poison { get; set; }

    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    [JsonPropertyName("BodyParts")]
    public Dictionary<string, BodyPartHealth>? BodyParts { get; set; }

    public double? UpdateTime { get; set; }

    public bool? Immortal { get; set; }
}

public record BodyPartHealth
{
    public CurrentMinMax? Health { get; set; }

    public Dictionary<string, BodyPartEffectProperties?>? Effects { get; set; } // TODO: change key to DamageEffectType enum
}

public record BodyPartEffectProperties
{
    // TODO: this was any, what actual type is it?
    public object? ExtraData { get; set; }

    public double? Time { get; set; }
}

public record CurrentMinMax
{
    public double? Current { get; set; }

    public double? Minimum { get; set; }

    public double? Maximum { get; set; }

    public double? OverDamageReceivedMultiplier { get; set; }

    public double? EnvironmentDamageMultiplier { get; set; }
}

public record BotBaseInventory
{
    [JsonPropertyName("items")]
    public List<Item>? Items { get; set; }

    [JsonPropertyName("equipment")]
    public MongoId? Equipment { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("stash")]
    public MongoId? Stash { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("sortingTable")]
    public MongoId? SortingTable { get; set; }

    [JsonPropertyName("questRaidItems")]
    public MongoId? QuestRaidItems { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("questStashItems")]
    public MongoId? QuestStashItems { get; set; }

    /// <summary>
    ///     Key is hideout area enum numeric as string e.g. "24", value is area _id
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("hideoutAreaStashes")]
    // TODO: key should be EAreaType enum
    public Dictionary<string, MongoId>? HideoutAreaStashes { get; set; } // Key = hideout area key as string

    /// <summary>
    /// key = "Item4", "Item10"
    /// </summary>
    [JsonPropertyName("fastPanel")]
    public Dictionary<string, MongoId>? FastPanel { get; set; }

    [JsonPropertyName("favoriteItems")]
    public IEnumerable<MongoId>? FavoriteItems { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("hideoutCustomizationStashId")]
    public MongoId? HideoutCustomizationStashId { get; set; }
}

public record Skills
{
    public IEnumerable<CommonSkill> Common { get; set; }

    public IEnumerable<MasterySkill>? Mastering { get; set; }

    public double Points { get; set; }
}

public record MasterySkill
{
    public string Id { get; set; }

    public double Progress { get; set; }
}

public record CommonSkill
{
    public double PointsEarnedDuringSession { get; set; }

    public long LastAccess { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SkillTypes Id { get; set; }

    public double Progress { get; set; }

    [JsonPropertyName("max")]
    public int? Max { get; set; }

    [JsonPropertyName("min")]
    public int? Min { get; set; }
}

public record Stats
{
    public EftStats? Eft { get; set; }
}

public record EftStats
{
    public IEnumerable<string>? CarriedQuestItems { get; set; }

    public IEnumerable<Victim>? Victims { get; set; }

    public double? TotalSessionExperience { get; set; }

    public long? LastSessionDate { get; set; }

    public SessionCounters? SessionCounters { get; set; }

    public OverallCounters? OverallCounters { get; set; }

    public float? SessionExperienceMult { get; set; }

    public float? ExperienceBonusMult { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public Aggressor? Aggressor { get; set; }

    public IEnumerable<DroppedItem>? DroppedItems { get; set; }

    public IEnumerable<FoundInRaidItem>? FoundInRaidItems { get; set; }

    public DamageHistory? DamageHistory { get; set; }

    public DeathCause? DeathCause { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public LastPlayerState? LastPlayerState { get; set; }

    public long? TotalInGameTime { get; set; }

    public string? SurvivorClass { get; set; }

    [JsonPropertyName("sptLastRaidFenceRepChange")]
    public float? SptLastRaidFenceRepChange { get; set; }
}

public record DroppedItem
{
    public MongoId QuestId { get; set; }

    public MongoId? ItemId { get; set; }

    public string? ZoneId { get; set; }
}

public record FoundInRaidItem
{
    public MongoId QuestId { get; set; }

    public MongoId? ItemId { get; set; }
}

public record Victim
{
    public string? AccountId { get; set; }

    public MongoId? ProfileId { get; set; }

    public string? Name { get; set; }

    public string? Side { get; set; }

    public string? BodyPart { get; set; }

    public string? Time { get; set; }

    public double? Distance { get; set; }

    public double? Level { get; set; }

    public string? Weapon { get; set; }

    public double? PrestigeLevel { get; set; }

    public string? ColliderType { get; set; }

    public string? Role { get; set; }

    public string? Location { get; set; }

    [JsonPropertyName("GInterface187.ProfileId")]
    public object profileIdTest { get; set; }

    [JsonPropertyName("GInterface187.Nickname")]
    public object NicknameTest { get; set; }

    [JsonPropertyName("GInterface187.Side")]
    public object SideTest { get; set; }

    [JsonPropertyName("GInterface187.PrestigeLevel")]
    public object PrestigeLevelTest { get; set; }
}

public record SessionCounters
{
    public IEnumerable<CounterKeyValue>? Items { get; set; }
}

public record OverallCounters
{
    public List<CounterKeyValue>? Items { get; set; }
}

public record CounterKeyValue
{
    public HashSet<string>? Key { get; set; }

    public double? Value { get; set; }
}

public record Aggressor
{
    public double? PrestigeLevel { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? AccountId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public MongoId? ProfileId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? MainProfileNickname { get; set; }

    public string? Name { get; set; }

    public string? Side { get; set; }

    public string? BodyPart { get; set; }

    public string? HeadSegment { get; set; }

    public string? WeaponName { get; set; }

    public string? Category { get; set; }

    public string? ColliderType { get; set; }

    public string? Role { get; set; }
}

public record DamageHistory
{
    public string? LethalDamagePart { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public DamageStats? LethalDamage { get; set; }

    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public BodyPartsDamageHistory? BodyParts { get; set; }
}

public record BodyPartsDamageHistory
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

public record DamageStats
{
    private string? _type;

    public double? Amount { get; set; }

    public string? Type
    {
        get { return _type; }
        set { _type = value == null ? null : string.Intern(value); }
    }

    public string? SourceId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? OverDamageFrom { get; set; }

    public bool? Blunt { get; set; }

    public double? ImpactsCount { get; set; }
}

public record DeathCause
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DamageType? DamageType { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PlayerSide? Side { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WildSpawnType? Role { get; set; }

    public string? WeaponId { get; set; }
}

public record LastPlayerState
{
    public LastPlayerStateInfo? Info { get; set; }

    public Dictionary<string, string>? Customization { get; set; }

    // TODO: there is no definition on TS just any
    public object? Equipment { get; set; }
}

public record LastPlayerStateInfo
{
    public string? Nickname { get; set; }

    public string? Side { get; set; }

    public double? Level { get; set; }

    public MemberCategory? MemberCategory { get; set; }
}

public record BackendCounter
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("qid")]
    public MongoId? QId { get; set; }

    [JsonPropertyName("value")]
    public double? Value { get; set; }
}

public record InsuredItem
{
    /// <summary>
    ///     Trader ID item was insured by
    /// </summary>
    [JsonPropertyName("tid")]
    public MongoId TId { get; set; }

    [JsonPropertyName("itemId")]
    public MongoId? ItemId { get; set; }
}

public record Hideout
{
    public Dictionary<MongoId, Production?>? Production { get; set; }

    public List<BotHideoutArea>? Areas { get; set; }

    public Dictionary<MongoId, HideoutImprovement>? Improvements { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public HideoutCounters? HideoutCounters { get; set; }

    /// <summary>
    ///     32 char hex value
    /// </summary>
    public string? Seed { get; set; }

    public Dictionary<MongoId, MongoId>? MannequinPoses { get; set; }

    [JsonPropertyName("sptUpdateLastRunTimestamp")]
    public long? SptUpdateLastRunTimestamp { get; set; }

    public Dictionary<string, MongoId>? Customization { get; set; } // Key = Area customisation type as string, e.g. "Wall", "Light", "ShootingRangeMark"
}

public record HideoutCounters
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

public record HideoutImprovement
{
    [JsonPropertyName("completed")]
    public bool? Completed { get; set; }

    [JsonPropertyName("improveCompleteTimestamp")]
    public long? ImproveCompleteTimestamp { get; set; }
}

public record Production // use this instead of productive and scavcase
{
    public List<Item>? Products { get; set; }

    /// <summary>
    ///     Seconds passed of production
    /// </summary>
    public double? Progress { get; set; }

    /// <summary>
    ///     Is craft in some state of being worked on by client (crafting/ready to pick up)
    /// </summary>
    [JsonPropertyName("inProgress")]
    public bool? InProgress { get; set; }

    public long? StartTimestamp { get; set; }

    public double? SkipTime { get; set; }

    /// <summary>
    ///     Seconds needed to fully craft
    /// </summary>
    public double? ProductionTime { get; set; }

    public List<Item>? GivenItemsInStart { get; set; }

    public bool? Interrupted { get; set; }

    public string? Code { get; set; }

    public bool? Decoded { get; set; }

    public bool? AvailableForFinish { get; set; }

    /// <summary>
    ///     Used in hideout production.json
    /// </summary>
    public bool? needFuelForAllProductionTime { get; set; }

    /// <summary>
    ///     Used when sending data to client
    /// </summary>
    public bool? NeedFuelForAllProductionTime { get; set; }

    [JsonPropertyName("sptIsScavCase")]
    public bool? SptIsScavCase { get; set; }

    /// <summary>
    ///     Some crafts are always inProgress, but need to be reset, e.g. water collector
    /// </summary>
    [JsonPropertyName("sptIsComplete")]
    public bool? SptIsComplete { get; set; }

    /// <summary>
    ///     Is the craft a Continuous, e.g. bitcoins/water collector
    /// </summary>
    [JsonPropertyName("sptIsContinuous")]
    public bool? SptIsContinuous { get; set; }

    /// <summary>
    ///     Stores a list of tools used in this craft and whether they're FiR, to give back once the craft is done
    /// </summary>
    [JsonPropertyName("sptRequiredTools")]
    public List<Item>? SptRequiredTools { get; set; }

    /// <summary>
    ///     Craft is cultist circle sacrifice
    /// </summary>
    [JsonPropertyName("sptIsCultistCircle")]
    public bool? SptIsCultistCircle { get; set; }

    public MongoId RecipeId { get; set; }
}

public record BotHideoutArea
{
    [JsonPropertyName("type")]
    public HideoutAreas Type { get; set; }

    [JsonPropertyName("level")]
    public int? Level { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("passiveBonusesEnabled")]
    public bool? PassiveBonusesEnabled { get; set; }

    /// <summary>
    ///     Must be integer
    /// </summary>
    [JsonPropertyName("completeTime")]
    public int? CompleteTime { get; set; }

    [JsonPropertyName("constructing")]
    public bool? Constructing { get; set; }

    [JsonPropertyName("slots")]
    public List<HideoutSlot>? Slots { get; set; }

    [JsonPropertyName("lastRecipe")]
    public MongoId? LastRecipe { get; set; }
}

public record HideoutSlot
{
    /// <summary>
    ///     SPT specific value to keep track of what index this slot is (0,1,2,3 etc.)
    /// </summary>
    [JsonPropertyName("locationIndex")]
    public int? LocationIndex { get; set; }

    [JsonPropertyName("item")]
    public List<HideoutItem>? Items { get; set; }
}

public record LastCompleted
{
    [JsonPropertyName("$oid")]
    public string? OId { get; set; }
}

public record Notes
{
    [JsonPropertyName("Notes")]
    public List<Note>? DataNotes { get; set; }
}

public enum SurvivorClass
{
    Unknown,
    Neutralizer,
    Marauder,
    Paramedic,
    Survivor,
}

public record TraderInfo
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

public record RagfairInfo
{
    [JsonPropertyName("rating")]
    public double? Rating { get; set; }

    [JsonPropertyName("isRatingGrowing")]
    public bool? IsRatingGrowing { get; set; }

    [JsonPropertyName("offers")]
    public List<RagfairOffer>? Offers { get; set; }

    [JsonPropertyName("sellSum")]
    public double? SellSum { get; set; }

    [JsonPropertyName("notSellSum")]
    public double? NotSellSum { get; set; }
}

public record Bonus
{
    [JsonPropertyName("id")]
    public MongoId Id { get; set; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BonusType? Type { get; set; }

    [JsonPropertyName("templateId")]
    public MongoId? TemplateId { get; set; }

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
