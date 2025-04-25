using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record StaticLootDetails
{
    [JsonPropertyName("itemcountDistribution")]
    public ItemCountDistribution[] ItemCountDistribution
    {
        get;
        set;
    }

    [JsonPropertyName("itemDistribution")]
    public ItemDistribution[] ItemDistribution
    {
        get;
        set;
    }
}
