using System.Text.Json.Serialization;
using Core.Models.Enums;
using Core.Utils.Json.Converters;

namespace Core.Models.Eft.Common.Tables;

public class Quest
{
    /// <summary>
    /// SPT addition - human readable quest name
    /// </summary>
    [JsonPropertyName("QuestName")]
    public string? QuestName { get; set; }

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

    [JsonPropertyName("type")]
    public string? Type { get; set; }

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
    public QuestStatus? SptStatus { get; set; }
    
    [JsonPropertyName("questStatus")]
    public QuestStatus? QuestStatus { get; set; }
    
    [JsonPropertyName("changeCost")]
    public List<object> ChangeCost { get; set; }
    
    [JsonPropertyName("changeStandingCost")]
    public double ChangeStandingCost { get; set; }
}

public class QuestStatus
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("uid")]
    public string? Uid { get; set; }
    
    [JsonPropertyName("qid")]
    public string? Qid { get; set; }
    
    [JsonPropertyName("startTime")]
    public double? StartTime { get; set; }
    
    [JsonPropertyName("status")]
    public double? Status { get; set; }
    
    [JsonPropertyName("statusTimers")]
    public Dictionary<string, double>? StatusTimers { get; set; }
    
    [JsonPropertyName("completedConditions")]
    public List<string>? CompletedConditions { get; set; }
    
    [JsonPropertyName("availableAfter")]
    public double? AvailableAfter { get; set; }
}

public class QuestConditionTypes
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

public class QuestCondition
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

    [JsonPropertyName("target")]
    public object? Target { get; set; } // TODO: string[] | string

    [JsonPropertyName("value")]
    public object? Value { get; set; } // TODO: string | number

    [JsonPropertyName("type")]
    public object? Type { get; set; } // TODO: boolean | string

    [JsonPropertyName("status")]
    public List<object>? Status { get; set; }

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

public class QuestConditionCounter
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("conditions")]
    public List<QuestConditionCounterCondition>? Conditions { get; set; }
}

public class QuestConditionCounterCondition
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

public class EnemyHealthEffect
{
    [JsonPropertyName("bodyParts")]
    public List<string>? BodyParts { get; set; }

    [JsonPropertyName("effects")]
    public List<string>? Effects { get; set; }
}

public class ValueCompare
{
    [JsonPropertyName("compareMethod")]
    public string? CompareMethod { get; set; }

    [JsonPropertyName("value")]
    public double? Value { get; set; }
}

public class CounterConditionDistance
{
    [JsonPropertyName("value")]
    public int? Value { get; set; }

    [JsonPropertyName("compareMethod")]
    public string? CompareMethod { get; set; }
}

public class DaytimeCounter
{
    [JsonPropertyName("from")]
    public int? From { get; set; }

    [JsonPropertyName("to")]
    public int? To { get; set; }
}

public class VisibilityCondition
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

public class QuestRewards
{
    [JsonPropertyName("AvailableForStart")]
    public List<QuestReward>? AvailableForStart { get; set; }

    [JsonPropertyName("AvailableForFinish")]
    public List<QuestReward>? AvailableForFinish { get; set; }

    [JsonPropertyName("Started")]
    public List<QuestReward>? Started { get; set; }

    [JsonPropertyName("Success")]
    public List<QuestReward>? Success { get; set; }

    [JsonPropertyName("Fail")]
    public List<QuestReward>? Fail { get; set; }

    [JsonPropertyName("FailRestartable")]
    public List<QuestReward>? FailRestartable { get; set; }

    [JsonPropertyName("Expired")]
    public List<QuestReward>? Expired { get; set; }
}

public class QuestReward
{
    [JsonPropertyName("value")]
    public object? Value { get; set; } // TODO: Can be either string or number

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; } // QuestRewardType

    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("target")]
    public string? Target { get; set; }

    [JsonPropertyName("items")]
    public List<Item>? Items { get; set; }

    [JsonPropertyName("loyaltyLevel")]
    public int? LoyaltyLevel { get; set; }

    /** Hideout area id */
    [JsonPropertyName("traderId")]
    public object? TraderId { get; set; } // TODO: string | int

    [JsonPropertyName("isEncoded")]
    public bool? IsEncoded { get; set; }

    [JsonPropertyName("unknown")]
    public bool? Unknown { get; set; }

    [JsonPropertyName("findInRaid")]
    public bool? FindInRaid { get; set; }

    [JsonPropertyName("gameMode")]
    public List<string>? GameMode { get; set; }

    /** Game editions whitelisted to get reward */
    [JsonPropertyName("availableInGameEditions")]
    public List<string>? AvailableInGameEditions { get; set; }

    /** Game editions blacklisted from getting reward */
    [JsonPropertyName("notAvailableInGameEditions")]
    public List<string>? NotAvailableInGameEditions { get; set; }
    
    // This is always Null atm in the achievements.json
    [JsonPropertyName("illustrationConfig")]
    public object? IllustrationConfig { get; set; }
    
    [JsonPropertyName("isHidden")]
    public bool? IsHidden { get; set; }
}
