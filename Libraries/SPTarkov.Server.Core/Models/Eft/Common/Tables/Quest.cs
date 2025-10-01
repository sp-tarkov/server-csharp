using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Utils.Json;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Quest
{
    /// <summary>
    ///     SPT addition - human readable quest name
    /// </summary>
    [JsonPropertyName("QuestName")]
    public string? QuestName { get; set; }

    /// <summary>
    ///     _id
    /// </summary>
    [JsonPropertyName("_id")]
    public required MongoId Id { get; set; }

    [JsonPropertyName("canShowNotificationsInGame")]
    public required bool CanShowNotificationsInGame { get; set; }

    [JsonPropertyName("conditions")]
    public required QuestConditionTypes Conditions { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("failMessageText")]
    public string? FailMessageText { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("traderId")]
    public required MongoId TraderId { get; set; }

    [JsonPropertyName("location")]
    public required string Location { get; set; }

    [JsonPropertyName("image")]
    public required string Image { get; set; }

    [JsonPropertyName("type")] // can be string or QuestTypeEnum
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required QuestTypeEnum Type { get; set; }

    [JsonPropertyName("isKey")]
    public bool? IsKey { get; set; }

    [JsonPropertyName("restartable")]
    public required bool Restartable { get; set; }

    [JsonPropertyName("instantComplete")]
    public bool? InstantComplete { get; set; }

    [JsonPropertyName("secretQuest")]
    public bool? SecretQuest { get; set; }

    [JsonPropertyName("startedMessageText")]
    public string? StartedMessageText { get; set; }

    [JsonPropertyName("successMessageText")]
    public string? SuccessMessageText { get; set; }

    [JsonPropertyName("acceptPlayerMessage")]
    public string AcceptPlayerMessage { get; set; }

    [JsonPropertyName("acceptanceAndFinishingSource")]
    public string AcceptanceAndFinishingSource { get; set; }

    [JsonPropertyName("declinePlayerMessage")]
    public string? DeclinePlayerMessage { get; set; }

    [JsonPropertyName("completePlayerMessage")]
    public string? CompletePlayerMessage { get; set; }

    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("rewards")]
    public Dictionary<string, List<Reward>>? Rewards { get; set; }

    /// <summary>
    ///     Becomes 'AppearStatus' inside client
    /// </summary>
    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("KeyQuest")]
    public bool? KeyQuest { get; set; }

    [JsonPropertyName("changeQuestMessageText")]
    public string? ChangeQuestMessageText { get; set; }

    /// <summary>
    ///     "Pmc" or "Scav"
    /// </summary>
    [JsonPropertyName("side")]
    public required string Side { get; set; }

    [JsonPropertyName("progressSource")]
    public string? ProgressSource { get; set; }

    [JsonPropertyName("rankingModes")]
    public List<string>? RankingModes { get; set; }

    [JsonPropertyName("gameModes")]
    public List<string>? GameModes { get; set; }

    [JsonPropertyName("arenaLocations")]
    public List<string>? ArenaLocations { get; set; }

    [JsonPropertyName("dialogueId")]
    public MongoId? DialogueId { get; set; }

    /// <summary>
    ///     Status of quest to player
    /// </summary>
    [JsonPropertyName("sptStatus")]
    public QuestStatusEnum? SptStatus { get; set; }
}

/// <summary>
///     Based on QuestDataClass in the client
/// </summary>
public record QuestStatus
{
    [JsonPropertyName("qid")]
    public required MongoId QId { get; set; }

    [JsonPropertyName("startTime")]
    public required double StartTime { get; set; }

    [JsonPropertyName("status")]
    public required QuestStatusEnum Status { get; set; }

    [JsonPropertyName("statusTimers")]
    public required Dictionary<QuestStatusEnum, double> StatusTimers { get; set; }

    [JsonPropertyName("completedConditions")]
    public List<string>? CompletedConditions { get; set; }

    [JsonPropertyName("availableAfter")]
    public double? AvailableAfter { get; set; }
}

public record QuestConditionTypes
{
    [JsonPropertyName("Started")]
    public List<QuestCondition>? Started { get; set; }

    [JsonPropertyName("AvailableForFinish")]
    public List<QuestCondition>? AvailableForFinish { get; set; }

    [JsonPropertyName("AvailableForStart")]
    public List<QuestCondition>? AvailableForStart { get; set; }

    [JsonPropertyName("Success")]
    public List<QuestCondition>? Success { get; set; }

    [JsonPropertyName("Fail")]
    public List<QuestCondition>? Fail { get; set; }
}

public record QuestCondition
{
    private string _conditionType;

    [JsonPropertyName("id")]
    public required MongoId Id { get; set; }

    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("compareMethod")]
    public string? CompareMethod { get; set; }

    [JsonPropertyName("dynamicLocale")]
    public required bool DynamicLocale { get; set; }

    [JsonPropertyName("globalQuestCounterId")]
    public string? GlobalQuestCounterId { get; set; }

    [JsonPropertyName("visibilityConditions")]
    public List<VisibilityCondition>? VisibilityConditions { get; set; }

    /// <summary>
    /// This is set as nullable in the client
    /// </summary>
    [JsonPropertyName("parentId")]
    public string? ParentId { get; set; }

    /// <summary>
    ///     Can be: string[] or string
    ///     Can be mongoId or string e.g. event_labyrinth_06_mech_place_01
    /// </summary>
    [JsonPropertyName("target")]
    public ListOrT<string>? Target { get; set; }

    [JsonPropertyName("value")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public double? Value { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("status")]
    public HashSet<QuestStatusEnum>? Status { get; set; }

    [JsonPropertyName("availableAfter")]
    public int? AvailableAfter { get; set; }

    [JsonPropertyName("dispersion")]
    public double? Dispersion { get; set; }

    [JsonPropertyName("onlyFoundInRaid")]
    public bool? OnlyFoundInRaid { get; set; }

    [JsonPropertyName("oneSessionOnly")]
    public bool? OneSessionOnly { get; set; }

    [JsonPropertyName("isResetOnConditionFailed")]
    public bool? IsResetOnConditionFailed { get; set; }

    [JsonPropertyName("isNecessary")]
    public bool? IsNecessary { get; set; }

    [JsonPropertyName("doNotResetIfCounterCompleted")]
    public bool? DoNotResetIfCounterCompleted { get; set; }

    [JsonPropertyName("dogtagLevel")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public int? DogtagLevel { get; set; }

    [JsonPropertyName("traderId")]
    public string? TraderId { get; set; }

    [JsonPropertyName("maxDurability")]
    public double? MaxDurability { get; set; }

    [JsonPropertyName("minDurability")]
    public double? MinDurability { get; set; }

    [JsonPropertyName("counter")]
    public QuestConditionCounter? Counter { get; set; }

    [JsonPropertyName("plantTime")]
    public double? PlantTime { get; set; }

    [JsonPropertyName("zoneId")]
    public string? ZoneId { get; set; }

    [JsonPropertyName("countInRaid")]
    public bool? CountInRaid { get; set; }

    [JsonPropertyName("completeInSeconds")]
    public double? CompleteInSeconds { get; set; }

    [JsonPropertyName("isEncoded")]
    public bool? IsEncoded { get; set; }

    [JsonPropertyName("conditionType")]
    public required string ConditionType
    {
        get { return _conditionType; }
        set { _conditionType = string.Intern(value); }
    }

    [JsonPropertyName("areaType")]
    public HideoutAreas? AreaType { get; set; }

    [JsonPropertyName("baseAccuracy")]
    public ValueCompare? BaseAccuracy { get; set; }

    [JsonPropertyName("containsItems")]
    public List<string>? ContainsItems { get; set; }

    [JsonPropertyName("durability")]
    public ValueCompare? Durability { get; set; }

    [JsonPropertyName("effectiveDistance")]
    public ValueCompare? EffectiveDistance { get; set; }

    [JsonPropertyName("emptyTacticalSlot")]
    public ValueCompare? EmptyTacticalSlot { get; set; }

    [JsonPropertyName("ergonomics")]
    public ValueCompare? Ergonomics { get; set; }

    [JsonPropertyName("height")]
    public ValueCompare? Height { get; set; }

    [JsonPropertyName("hasItemFromCategory")]
    public List<string>? HasItemFromCategory { get; set; }

    [JsonPropertyName("magazineCapacity")]
    public ValueCompare? MagazineCapacity { get; set; }

    [JsonPropertyName("muzzleVelocity")]
    public ValueCompare? MuzzleVelocity { get; set; }

    [JsonPropertyName("recoil")]
    public ValueCompare? Recoil { get; set; }

    [JsonPropertyName("weight")]
    public ValueCompare? Weight { get; set; }

    [JsonPropertyName("width")]
    public ValueCompare? Width { get; set; }
}

public record QuestConditionCounter
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("conditions")]
    public List<QuestConditionCounterCondition>? Conditions { get; set; }
}

public record QuestConditionCounterCondition
{
    [JsonPropertyName("id")]
    public MongoId? Id { get; set; }

    [JsonPropertyName("dynamicLocale")]
    public bool? DynamicLocale { get; set; }

    [JsonPropertyName("target")]
    public ListOrT<string>? Target { get; set; }

    [JsonPropertyName("completeInSeconds")]
    public int? CompleteInSeconds { get; set; }

    [JsonPropertyName("energy")]
    public ValueCompare? Energy { get; set; }

    [JsonPropertyName("exitName")]
    public string? ExitName { get; set; }

    [JsonPropertyName("hydration")]
    public ValueCompare? Hydration { get; set; }

    [JsonPropertyName("time")]
    public ValueCompare? Time { get; set; }

    [JsonPropertyName("compareMethod")]
    public string? CompareMethod { get; set; }

    [JsonPropertyName("value")]
    public object? Value { get; set; }

    [JsonPropertyName("weapon")]
    public HashSet<string>? Weapon { get; set; }

    [JsonPropertyName("distance")]
    public CounterConditionDistance? Distance { get; set; }

    [JsonPropertyName("equipmentInclusive")]
    public IEnumerable<List<string>>? EquipmentInclusive { get; set; }

    [JsonPropertyName("weaponModsInclusive")]
    public IEnumerable<List<string>>? WeaponModsInclusive { get; set; }

    [JsonPropertyName("weaponModsExclusive")]
    public IEnumerable<List<string>>? WeaponModsExclusive { get; set; }

    [JsonPropertyName("enemyEquipmentInclusive")]
    public IEnumerable<List<string>>? EnemyEquipmentInclusive { get; set; }

    [JsonPropertyName("enemyEquipmentExclusive")]
    public IEnumerable<List<string>>? EnemyEquipmentExclusive { get; set; }

    [JsonPropertyName("weaponCaliber")]
    public List<string>? WeaponCaliber { get; set; }

    [JsonPropertyName("savageRole")]
    public List<string>? SavageRole { get; set; }

    [JsonPropertyName("status")]
    public List<string>? Status { get; set; }

    [JsonPropertyName("bodyPart")]
    public List<string>? BodyPart { get; set; }

    [JsonPropertyName("daytime")]
    public DaytimeCounter? Daytime { get; set; }

    [JsonPropertyName("conditionType")]
    public required string ConditionType { get; set; }

    [JsonPropertyName("enemyHealthEffects")]
    public List<EnemyHealthEffect>? EnemyHealthEffects { get; set; }

    [JsonPropertyName("resetOnSessionEnd")]
    public bool? ResetOnSessionEnd { get; set; }

    [JsonPropertyName("bodyPartsWithEffects")]
    public List<EnemyHealthEffect>? BodyPartsWithEffects { get; set; }

    [JsonPropertyName("IncludeNotEquippedItems")]
    public bool? IncludeNotEquippedItems { get; set; }

    [JsonPropertyName("equipmentExclusive")]
    public List<List<string>>? EquipmentExclusive { get; set; }

    [JsonPropertyName("zoneIds")]
    public List<string>? Zones { get; set; }
}

public record EnemyHealthEffect
{
    [JsonPropertyName("bodyParts")]
    public List<string>? BodyParts { get; set; }

    [JsonPropertyName("effects")]
    public List<string>? Effects { get; set; }
}

public record ValueCompare
{
    [JsonPropertyName("compareMethod")]
    public string? CompareMethod { get; set; }

    [JsonPropertyName("value")]
    public double? Value { get; set; }
}

public record CounterConditionDistance
{
    [JsonPropertyName("value")]
    public double? Value { get; set; }

    [JsonPropertyName("compareMethod")]
    public string? CompareMethod { get; set; }
}

public record DaytimeCounter
{
    [JsonPropertyName("from")]
    public int? From { get; set; }

    [JsonPropertyName("to")]
    public int? To { get; set; }
}

public record VisibilityCondition
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("target")]
    public string? Target { get; set; }

    [JsonPropertyName("value")]
    public int? Value { get; set; }

    [JsonPropertyName("dynamicLocale")]
    public bool? DynamicLocale { get; set; }

    [JsonPropertyName("oneSessionOnly")]
    public bool? OneSessionOnly { get; set; }

    [JsonPropertyName("conditionType")]
    public required string ConditionType { get; set; }
}
