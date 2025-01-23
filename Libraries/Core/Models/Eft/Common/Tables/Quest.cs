using System.Text.Json.Serialization;
using Core.Models.Enums;
using Core.Utils.Json;
using Core.Utils.Json.Converters;
using SptCommon.Extensions;

namespace Core.Models.Eft.Common.Tables;

public record Quest
{
    /// <summary>
    /// SPT addition - human readable quest name
    /// </summary>
    [JsonPropertyName("QuestName")]
    public string? QuestName { get; set; }

    /// <summary>
    /// _id
    /// </summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("canShowNotificationsInGame")]
    public bool? CanShowNotificationsInGame { get; set; }

    [JsonPropertyName("conditions")]
    public QuestConditionTypes? Conditions { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("failMessageText")]
    public string? FailMessageText { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("traderId")]
    public string? TraderId { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("type")] // can be string or QuestTypeEnum
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public QuestTypeEnum? Type { get; set; }

    [JsonPropertyName("isKey")]
    public bool? IsKey { get; set; }

    [JsonPropertyName("restartable")]
    public bool? Restartable { get; set; }

    [JsonPropertyName("instantComplete")]
    public bool? InstantComplete { get; set; }

    [JsonPropertyName("secretQuest")]
    public bool? SecretQuest { get; set; }

    [JsonPropertyName("startedMessageText")]
    public string? StartedMessageText { get; set; }

    [JsonPropertyName("successMessageText")]
    public string? SuccessMessageText { get; set; }

    [JsonPropertyName("acceptPlayerMessage")]
    public string? AcceptPlayerMessage { get; set; }

    [JsonPropertyName("declinePlayerMessage")]
    public string? DeclinePlayerMessage { get; set; }

    [JsonPropertyName("completePlayerMessage")]
    public string? CompletePlayerMessage { get; set; }

    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("rewards")]
    public QuestRewards? Rewards { get; set; }

    /// <summary>
    /// Becomes 'AppearStatus' inside client
    /// </summary>
    [JsonPropertyName("status")]
    public object? Status { get; set; } // TODO: string | number

    [JsonPropertyName("KeyQuest")]
    public bool? KeyQuest { get; set; }

    [JsonPropertyName("changeQuestMessageText")]
    public string? ChangeQuestMessageText { get; set; }

    /// <summary>
    /// "Pmc" or "Scav"
    /// </summary>
    [JsonPropertyName("side")]
    public string? Side { get; set; }

    [JsonPropertyName("acceptanceAndFinishingSource")]
    public string? AcceptanceAndFinishingSource { get; set; }

    [JsonPropertyName("progressSource")]
    public string? ProgressSource { get; set; }

    [JsonPropertyName("rankingModes")]
    public List<string>? RankingModes { get; set; }

    [JsonPropertyName("gameModes")]
    public List<string>? GameModes { get; set; }

    [JsonPropertyName("arenaLocations")]
    public List<string>? ArenaLocations { get; set; }

    /// <summary>
    /// Status of quest to player
    /// </summary>
    [JsonPropertyName("sptStatus")]
    public QuestStatusEnum? SptStatus { get; set; }
    
    [JsonPropertyName("questStatus")]
    public QuestStatus? QuestStatus { get; set; }
    
    [JsonPropertyName("changeCost")]
    public List<object> ChangeCost { get; set; }
    
    [JsonPropertyName("changeStandingCost")]
    public double ChangeStandingCost { get; set; }
}

/// <summary>
/// Same as BotBase.Quests
/// </summary>
public record QuestStatus
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("uid")]
    public string? Uid { get; set; }
    
    [JsonPropertyName("qid")]
    public string? QId { get; set; }
    
    [JsonPropertyName("startTime")]
    public double? StartTime { get; set; }
    
    [JsonPropertyName("status")]
    public QuestStatusEnum? Status { get; set; }
    
    [JsonPropertyName("statusTimers")]
    public Dictionary<QuestStatusEnum, double>? StatusTimers { get; set; }
    
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
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("compareMethod")]
    public string? CompareMethod { get; set; }

    [JsonPropertyName("dynamicLocale")]
    public bool? DynamicLocale { get; set; }

    [JsonPropertyName("visibilityConditions")]
    public List<VisibilityCondition>? VisibilityConditions { get; set; }

    [JsonPropertyName("globalQuestCounterId")]
    public string? GlobalQuestCounterId { get; set; }

    [JsonPropertyName("parentId")]
    public string? ParentId { get; set; }

    /// <summary>
    /// Can be: string[] or string
    /// </summary>
    [JsonPropertyName("target")]
    [JsonConverter(typeof(ListOrTConverterFactory))]
    public ListOrT<string>? Target { get; set; } // TODO: string[] | string

    [JsonPropertyName("value")]
    public object? Value { get; set; } // TODO: string | number

    [JsonPropertyName("type")]
    public object? Type { get; set; } // TODO: boolean | string

    [JsonPropertyName("status")]
    public List<QuestStatusEnum>? Status { get; set; }

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
    public object? DogtagLevel { get; set; } // TODO: number | string

    [JsonPropertyName("traderId")]
    public string? TraderId { get; set; }

    [JsonPropertyName("maxDurability")]
    public object? MaxDurability { get; set; } // TODO: number | string

    [JsonPropertyName("minDurability")]
    public object? MinDurability { get; set; } // TODO: number | string

    [JsonPropertyName("counter")]
    public QuestConditionCounter? Counter { get; set; }

    [JsonPropertyName("plantTime")]
    public int? PlantTime { get; set; }

    [JsonPropertyName("zoneId")]
    public string? ZoneId { get; set; }

    [JsonPropertyName("countInRaid")]
    public bool? CountInRaid { get; set; }

    [JsonPropertyName("completeInSeconds")]
    public int? CompleteInSeconds { get; set; }

    [JsonPropertyName("isEncoded")]
    public bool? IsEncoded { get; set; }

    [JsonPropertyName("conditionType")]
    public string? ConditionType { get; set; }

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
    public string? Id { get; set; }

    [JsonPropertyName("dynamicLocale")]
    public bool? DynamicLocale { get; set; }

    [JsonPropertyName("target")]
    public object? Target { get; set; } // TODO: string[] | string

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
    public object? Value { get; set; } // TODO: number | string

    [JsonPropertyName("weapon")]
    public List<string>? Weapon { get; set; }

    [JsonPropertyName("distance")]
    public CounterConditionDistance? Distance { get; set; }

    [JsonPropertyName("equipmentInclusive")]
    public List<List<string>>? EquipmentInclusive { get; set; }

    [JsonPropertyName("weaponModsInclusive")]
    public List<List<string>>? WeaponModsInclusive { get; set; }

    [JsonPropertyName("weaponModsExclusive")]
    public List<List<string>>? WeaponModsExclusive { get; set; }

    [JsonPropertyName("enemyEquipmentInclusive")]
    public List<List<string>>? EnemyEquipmentInclusive { get; set; }

    [JsonPropertyName("enemyEquipmentExclusive")]
    public List<List<string>>? EnemyEquipmentExclusive { get; set; }

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
    public string? ConditionType { get; set; }

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
    public string? ConditionType { get; set; }
}

public record QuestRewards
{
    [JsonPropertyName("AvailableForStart")]
    public List<Reward>? AvailableForStart { get; set; }

    [JsonPropertyName("AvailableForFinish")]
    public List<Reward>? AvailableForFinish { get; set; }

    [JsonPropertyName("Started")]
    public List<Reward>? Started { get; set; }

    [JsonPropertyName("Success")]
    public List<Reward>? Success { get; set; }

    [JsonPropertyName("Fail")]
    public List<Reward>? Fail { get; set; }

    [JsonPropertyName("FailRestartable")]
    public List<Reward>? FailRestartable { get; set; }

    [JsonPropertyName("Expired")]
    public List<Reward>? Expired { get; set; }
}
