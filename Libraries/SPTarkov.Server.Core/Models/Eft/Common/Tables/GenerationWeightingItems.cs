using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record GenerationWeightingItems
{
    [JsonPropertyName("grenades")]
    public GenerationData Grenades
    {
        get;
        set;
    }

    [JsonPropertyName("healing")]
    public GenerationData Healing
    {
        get;
        set;
    }

    [JsonPropertyName("drugs")]
    public GenerationData Drugs
    {
        get;
        set;
    }

    [JsonPropertyName("food")]
    public GenerationData Food
    {
        get;
        set;
    }

    [JsonPropertyName("drink")]
    public GenerationData Drink
    {
        get;
        set;
    }

    [JsonPropertyName("currency")]
    public GenerationData Currency
    {
        get;
        set;
    }

    [JsonPropertyName("stims")]
    public GenerationData Stims
    {
        get;
        set;
    }

    [JsonPropertyName("backpackLoot")]
    public GenerationData BackpackLoot
    {
        get;
        set;
    }

    [JsonPropertyName("pocketLoot")]
    public GenerationData PocketLoot
    {
        get;
        set;
    }

    [JsonPropertyName("vestLoot")]
    public GenerationData VestLoot
    {
        get;
        set;
    }

    [JsonPropertyName("magazines")]
    public GenerationData Magazines
    {
        get;
        set;
    }

    [JsonPropertyName("specialItems")]
    public GenerationData SpecialItems
    {
        get;
        set;
    }

    [JsonPropertyName("looseLoot")]
    public GenerationData LooseLoot
    {
        get;
        set;
    }
}
