using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Utils.Json;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record QuestConditionCounterCondition
{
    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("dynamicLocale")]
    public bool? DynamicLocale
    {
        get;
        set;
    }

    [JsonPropertyName("target")]
    [JsonConverter(typeof(ListOrTConverterFactory))]
    public ListOrT<string>? Target
    {
        get;
        set;
    }

    [JsonPropertyName("completeInSeconds")]
    public int? CompleteInSeconds
    {
        get;
        set;
    }

    [JsonPropertyName("energy")]
    public ValueCompare? Energy
    {
        get;
        set;
    }

    [JsonPropertyName("exitName")]
    public string? ExitName
    {
        get;
        set;
    }

    [JsonPropertyName("hydration")]
    public ValueCompare? Hydration
    {
        get;
        set;
    }

    [JsonPropertyName("time")]
    public ValueCompare? Time
    {
        get;
        set;
    }

    [JsonPropertyName("compareMethod")]
    public string? CompareMethod
    {
        get;
        set;
    }

    [JsonPropertyName("value")]
    public object? Value
    {
        get;
        set;
    }

    [JsonPropertyName("weapon")]
    public List<string>? Weapon
    {
        get;
        set;
    }

    [JsonPropertyName("distance")]
    public CounterConditionDistance? Distance
    {
        get;
        set;
    }

    [JsonPropertyName("equipmentInclusive")]
    public List<List<string>>? EquipmentInclusive
    {
        get;
        set;
    }

    [JsonPropertyName("weaponModsInclusive")]
    public List<List<string>>? WeaponModsInclusive
    {
        get;
        set;
    }

    [JsonPropertyName("weaponModsExclusive")]
    public List<List<string>>? WeaponModsExclusive
    {
        get;
        set;
    }

    [JsonPropertyName("enemyEquipmentInclusive")]
    public List<List<string>>? EnemyEquipmentInclusive
    {
        get;
        set;
    }

    [JsonPropertyName("enemyEquipmentExclusive")]
    public List<List<string>>? EnemyEquipmentExclusive
    {
        get;
        set;
    }

    [JsonPropertyName("weaponCaliber")]
    public List<string>? WeaponCaliber
    {
        get;
        set;
    }

    [JsonPropertyName("savageRole")]
    public List<string>? SavageRole
    {
        get;
        set;
    }

    [JsonPropertyName("status")]
    public List<string>? Status
    {
        get;
        set;
    }

    [JsonPropertyName("bodyPart")]
    public List<string>? BodyPart
    {
        get;
        set;
    }

    [JsonPropertyName("daytime")]
    public DaytimeCounter? Daytime
    {
        get;
        set;
    }

    [JsonPropertyName("conditionType")]
    public string? ConditionType
    {
        get;
        set;
    }

    [JsonPropertyName("enemyHealthEffects")]
    public List<EnemyHealthEffect>? EnemyHealthEffects
    {
        get;
        set;
    }

    [JsonPropertyName("resetOnSessionEnd")]
    public bool? ResetOnSessionEnd
    {
        get;
        set;
    }

    [JsonPropertyName("bodyPartsWithEffects")]
    public List<EnemyHealthEffect>? BodyPartsWithEffects
    {
        get;
        set;
    }

    [JsonPropertyName("IncludeNotEquippedItems")]
    public bool? IncludeNotEquippedItems
    {
        get;
        set;
    }

    [JsonPropertyName("equipmentExclusive")]
    public List<List<string>>? EquipmentExclusive
    {
        get;
        set;
    }

    [JsonPropertyName("zoneIds")]
    public List<string>? Zones
    {
        get;
        set;
    }
}
