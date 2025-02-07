using System.Text.Json.Serialization;

namespace Core.Models.Eft.InRaid;

public record InsuredItemsData
{
    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("durability")]
    public int? Durability
    {
        get;
        set;
    }

    [JsonPropertyName("maxDurability")]
    public int? MaxDurability
    {
        get;
        set;
    }

    [JsonPropertyName("hits")]
    public int? Hits
    {
        get;
        set;
    }

    [JsonPropertyName("usedInQuest")]
    public bool? UsedInQuest
    {
        get;
        set;
    }
}
