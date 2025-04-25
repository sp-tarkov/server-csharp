using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record HealthSkillProgress
{
    [JsonPropertyName("SkillProgress")]
    public double? SkillProgress
    {
        get;
        set;
    }
}
