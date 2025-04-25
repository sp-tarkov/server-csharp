using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record LooseLootItemDistribution
{
    [JsonPropertyName("composedKey")]
    public ComposedKey? ComposedKey
    {
        get;
        set;
    }

    [JsonPropertyName("relativeProbability")]
    public double? RelativeProbability
    {
        get;
        set;
    }
}
