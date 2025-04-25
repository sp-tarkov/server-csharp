using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ItemCountDistribution
{
    [JsonPropertyName("count")]
    public int? Count
    {
        get;
        set;
    }

    [JsonPropertyName("relativeProbability")]
    public float? RelativeProbability
    {
        get;
        set;
    }
}
