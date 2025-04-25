using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Perception
{
    [JsonPropertyName("DependentSkillRatios")]
    public List<SkillRatio>? DependentSkillRatios
    {
        get;
        set;
    }

    [JsonPropertyName("OnlineAction")]
    public double? OnlineAction
    {
        get;
        set;
    }

    [JsonPropertyName("UniqueLoot")]
    public double? UniqueLoot
    {
        get;
        set;
    }
}
