using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Spawnpoint
{
    [JsonPropertyName("locationId")]
    public string? LocationId
    {
        get;
        set;
    }

    [JsonPropertyName("probability")]
    public double? Probability
    {
        get;
        set;
    }

    [JsonPropertyName("template")]
    public SpawnpointTemplate? Template
    {
        get;
        set;
    }

    [JsonPropertyName("itemDistribution")]
    public List<LooseLootItemDistribution>? ItemDistribution
    {
        get;
        set;
    }
}
