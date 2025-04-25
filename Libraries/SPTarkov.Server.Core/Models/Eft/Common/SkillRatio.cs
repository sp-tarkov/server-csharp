using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record SkillRatio
{
    [JsonPropertyName("Ratio")]
    public double? Ratio
    {
        get;
        set;
    }

    [JsonPropertyName("SkillId")]
    public string? SkillId
    {
        get;
        set;
    }
}
