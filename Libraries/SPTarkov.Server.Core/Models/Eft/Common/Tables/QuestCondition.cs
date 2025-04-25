using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils.Json;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record QuestCondition
{
    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("index")]
    public int? Index
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

    [JsonPropertyName("dynamicLocale")]
    public bool? DynamicLocale
    {
        get;
        set;
    }

    [JsonPropertyName("visibilityConditions")]
    public List<VisibilityCondition>? VisibilityConditions
    {
        get;
        set;
    }

    [JsonPropertyName("globalQuestCounterId")]
    public string? GlobalQuestCounterId
    {
        get;
        set;
    }

    [JsonPropertyName("parentId")]
    public string? ParentId
    {
        get;
        set;
    }

    /// <summary>
    ///     Can be: string[] or string
    /// </summary>
    [JsonPropertyName("target")]
    [JsonConverter(typeof(ListOrTConverterFactory))]
    public ListOrT<string>? Target
    {
        get;
        set;
    }

    [JsonPropertyName("value")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public double? Value
    {
        get;
        set;
    }

    [JsonPropertyName("type")]
    public object? Type
    {
        get;
        set;
    } // TODO: boolean | string

    [JsonPropertyName("status")]
    public List<QuestStatusEnum>? Status
    {
        get;
        set;
    }

    [JsonPropertyName("availableAfter")]
    public int? AvailableAfter
    {
        get;
        set;
    }

    [JsonPropertyName("dispersion")]
    public double? Dispersion
    {
        get;
        set;
    }

    [JsonPropertyName("onlyFoundInRaid")]
    public bool? OnlyFoundInRaid
    {
        get;
        set;
    }

    [JsonPropertyName("oneSessionOnly")]
    public bool? OneSessionOnly
    {
        get;
        set;
    }

    [JsonPropertyName("isResetOnConditionFailed")]
    public bool? IsResetOnConditionFailed
    {
        get;
        set;
    }

    [JsonPropertyName("isNecessary")]
    public bool? IsNecessary
    {
        get;
        set;
    }

    [JsonPropertyName("doNotResetIfCounterCompleted")]
    public bool? DoNotResetIfCounterCompleted
    {
        get;
        set;
    }

    [JsonPropertyName("dogtagLevel")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public int? DogtagLevel
    {
        get;
        set;
    }

    [JsonPropertyName("traderId")]
    public string? TraderId
    {
        get;
        set;
    }

    [JsonPropertyName("maxDurability")]
    public double? MaxDurability
    {
        get;
        set;
    }

    [JsonPropertyName("minDurability")]
    public double? MinDurability
    {
        get;
        set;
    }

    [JsonPropertyName("counter")]
    public QuestConditionCounter? Counter
    {
        get;
        set;
    }

    [JsonPropertyName("plantTime")]
    public double? PlantTime
    {
        get;
        set;
    }

    [JsonPropertyName("zoneId")]
    public string? ZoneId
    {
        get;
        set;
    }

    [JsonPropertyName("countInRaid")]
    public bool? CountInRaid
    {
        get;
        set;
    }

    [JsonPropertyName("completeInSeconds")]
    public double? CompleteInSeconds
    {
        get;
        set;
    }

    [JsonPropertyName("isEncoded")]
    public bool? IsEncoded
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

    [JsonPropertyName("areaType")]
    public HideoutAreas? AreaType
    {
        get;
        set;
    }

    [JsonPropertyName("baseAccuracy")]
    public ValueCompare? BaseAccuracy
    {
        get;
        set;
    }

    [JsonPropertyName("containsItems")]
    public List<string>? ContainsItems
    {
        get;
        set;
    }

    [JsonPropertyName("durability")]
    public ValueCompare? Durability
    {
        get;
        set;
    }

    [JsonPropertyName("effectiveDistance")]
    public ValueCompare? EffectiveDistance
    {
        get;
        set;
    }

    [JsonPropertyName("emptyTacticalSlot")]
    public ValueCompare? EmptyTacticalSlot
    {
        get;
        set;
    }

    [JsonPropertyName("ergonomics")]
    public ValueCompare? Ergonomics
    {
        get;
        set;
    }

    [JsonPropertyName("height")]
    public ValueCompare? Height
    {
        get;
        set;
    }

    [JsonPropertyName("hasItemFromCategory")]
    public List<string>? HasItemFromCategory
    {
        get;
        set;
    }

    [JsonPropertyName("magazineCapacity")]
    public ValueCompare? MagazineCapacity
    {
        get;
        set;
    }

    [JsonPropertyName("muzzleVelocity")]
    public ValueCompare? MuzzleVelocity
    {
        get;
        set;
    }

    [JsonPropertyName("recoil")]
    public ValueCompare? Recoil
    {
        get;
        set;
    }

    [JsonPropertyName("weight")]
    public ValueCompare? Weight
    {
        get;
        set;
    }

    [JsonPropertyName("width")]
    public ValueCompare? Width
    {
        get;
        set;
    }
}
