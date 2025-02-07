using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Eft.Hideout;

public record HideoutCustomisation
{
    [JsonPropertyName("globals")]
    public List<HideoutCustomisationGlobal>? Globals
    {
        get;
        set;
    }

    [JsonPropertyName("slots")]
    public List<HideoutCustomisationSlot>? Slots
    {
        get;
        set;
    }
}

public record HideoutCustomisationGlobal
{
    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("conditions")]
    public List<QuestCondition>? Conditions
    {
        get;
        set;
    }

    [JsonPropertyName("type")]
    public string? Type
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

    [JsonPropertyName("systemName")]
    public string? SystemName
    {
        get;
        set;
    }

    [JsonPropertyName("isEnabled")]
    public bool? IsEnabled
    {
        get;
        set;
    }

    [JsonPropertyName("itemId")]
    public string? ItemId
    {
        get;
        set;
    }
}

public record HideoutCustomisationSlot
{
    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("conditions")]
    public List<QuestCondition>? Conditions
    {
        get;
        set;
    }

    [JsonPropertyName("type")]
    public string? Type
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

    [JsonPropertyName("systemName")]
    public string? SystemName
    {
        get;
        set;
    }

    [JsonPropertyName("isEnabled")]
    public bool? IsEnabled
    {
        get;
        set;
    }

    [JsonPropertyName("slotId")]
    public string? SlotId
    {
        get;
        set;
    }

    [JsonPropertyName("areaTypeId")]
    public int? AreaTypeId
    {
        get;
        set;
    }
}
