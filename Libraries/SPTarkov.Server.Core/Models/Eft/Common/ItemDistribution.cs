using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ItemDistribution
{
    [JsonPropertyName("tpl")]
    public string? Tpl
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
